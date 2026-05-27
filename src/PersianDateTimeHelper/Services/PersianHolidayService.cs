using PersianDateTimeHelper.Abstractions;
using PersianDateTimeHelper.Models;

namespace PersianDateTimeHelper.Services;

/// <summary>Service for querying Iranian public holidays.</summary>
public sealed class PersianHolidayService
{
    private readonly IHolidayProvider _provider;
    private readonly bool _includeThursdayWeekend;

    public PersianHolidayService(
        IHolidayProvider? provider = null,
        bool includeThursdayWeekend = false)
    {
        _provider = provider ?? new EmbeddedHolidayProvider();
        _includeThursdayWeekend = includeThursdayWeekend;
    }

    // ── Core checks ───────────────────────────────────────────────────────────

    /// <summary>True if the date is a public holiday OR a weekend day.</summary>
    public bool IsNonWorkingDay(DateTime date)
        => IsPublicHoliday(date) || IsWeekend(date);

    /// <summary>True if the date is an official public holiday (excluding weekends).</summary>
    public bool IsPublicHoliday(DateTime date)
    {
        var (_, month, day) = PersianCalendarHelper.ToJalali(date);
        int jalaliYear = PersianCalendarHelper.Year(date);
        return _provider.GetHolidays(jalaliYear)
            .Any(h => h.Month == month && h.Day == day);
    }

    /// <summary>True if the date is an Iranian weekend day (Friday, or Thursday+Friday).</summary>
    public bool IsWeekend(DateTime date)
        => PersianCalendarHelper.IsWeekend(date, _includeThursdayWeekend);

    /// <summary>True if the date is a regular working day.</summary>
    public bool IsWorkday(DateTime date) => !IsNonWorkingDay(date);

    // ── Get holidays ──────────────────────────────────────────────────────────

    /// <summary>Get all public holidays in a Jalali month.</summary>
    public IReadOnlyList<PersianHoliday> GetHolidays(int jalaliYear, int jalaliMonth)
    {
        if (jalaliMonth < 1 || jalaliMonth > 12)
            throw new ArgumentOutOfRangeException(nameof(jalaliMonth), "Month must be 1–12.");

        return _provider.GetHolidays(jalaliYear)
            .Where(h => h.Month == jalaliMonth)
            .OrderBy(h => h.Day)
            .ToList();
    }

    /// <summary>Get all public holidays in a Jalali year.</summary>
    public IReadOnlyList<PersianHoliday> GetHolidays(int jalaliYear)
        => _provider.GetHolidays(jalaliYear)
            .OrderBy(h => h.Month).ThenBy(h => h.Day)
            .ToList();

    /// <summary>Get all public holidays between two Gregorian dates (inclusive).</summary>
    public IReadOnlyList<(DateTime Date, PersianHoliday Holiday)> GetHolidays(
        DateTime from, DateTime to)
    {
        if (from > to) throw new ArgumentException("'from' must be ≤ 'to'.");

        var result = new List<(DateTime, PersianHoliday)>();
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
        {
            if (!IsPublicHoliday(d)) continue;
            var (_, month, day) = PersianCalendarHelper.ToJalali(d);
            int jalaliYear = PersianCalendarHelper.Year(d);
            var holiday = _provider.GetHolidays(jalaliYear)
                .First(h => h.Month == month && h.Day == day);
            result.Add((d, holiday));
        }
        return result;
    }

    /// <summary>Get the holiday details for a specific date, or null if not a holiday.</summary>
    public PersianHoliday? GetHolidayDetails(DateTime date)
    {
        if (!IsPublicHoliday(date)) return null;
        var (_, month, day) = PersianCalendarHelper.ToJalali(date);
        int jalaliYear = PersianCalendarHelper.Year(date);
        return _provider.GetHolidays(jalaliYear)
            .FirstOrDefault(h => h.Month == month && h.Day == day);
    }

    // ── Count ─────────────────────────────────────────────────────────────────

    /// <summary>Count official public holidays in a Jalali month (weekends not included).</summary>
    public int CountPublicHolidays(int jalaliYear, int jalaliMonth)
        => GetHolidays(jalaliYear, jalaliMonth).Count;

    /// <summary>Count official public holidays in a Jalali year (weekends not included).</summary>
    public int CountPublicHolidays(int jalaliYear)
        => GetHolidays(jalaliYear).Count;

    /// <summary>Count total non-working days (holidays + weekends) in a Jalali month.</summary>
    public int CountNonWorkingDays(int jalaliYear, int jalaliMonth)
    {
        var (first, last) = PersianCalendarHelper.MonthRange(jalaliYear, jalaliMonth);
        return Enumerable.Range(0, (last - first).Days + 1)
            .Select(i => first.AddDays(i))
            .Count(IsNonWorkingDay);
    }

    /// <summary>Count total non-working days (holidays + weekends) in a Jalali year.</summary>
    public int CountNonWorkingDays(int jalaliYear)
    {
        var (first, last) = PersianCalendarHelper.YearRange(jalaliYear);
        return Enumerable.Range(0, (last - first).Days + 1)
            .Select(i => first.AddDays(i))
            .Count(IsNonWorkingDay);
    }

    /// <summary>Count working days in a Jalali month.</summary>
    public int CountWorkdays(int jalaliYear, int jalaliMonth)
        => PersianCalendarHelper.DaysInMonth(jalaliYear, jalaliMonth)
           - CountNonWorkingDays(jalaliYear, jalaliMonth);

    /// <summary>Count working days in a Jalali year.</summary>
    public int CountWorkdays(int jalaliYear)
    {
        int daysInYear = PersianCalendarHelper.IsLeapYear(jalaliYear) ? 366 : 365;
        return daysInYear - CountNonWorkingDays(jalaliYear);
    }

    /// <summary>Count working days between two Gregorian dates (inclusive).</summary>
    public int CountWorkdays(DateTime from, DateTime to)
    {
        if (from > to) throw new ArgumentException("'from' must be ≤ 'to'.");
        return Enumerable.Range(0, (to.Date - from.Date).Days + 1)
            .Select(i => from.Date.AddDays(i))
            .Count(IsWorkday);
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>
    /// Next public holiday on or after the given date.
    /// Returns null if none found within 365 days.
    /// </summary>
    public (DateTime Date, PersianHoliday Holiday)? NextPublicHoliday(DateTime date)
    {
        for (var d = date.Date; d <= date.Date.AddDays(365); d = d.AddDays(1))
        {
            if (!IsPublicHoliday(d)) continue;
            var (_, month, day) = PersianCalendarHelper.ToJalali(d);
            int jalaliYear = PersianCalendarHelper.Year(d);
            var holiday = _provider.GetHolidays(jalaliYear)
                .First(h => h.Month == month && h.Day == day);
            return (d, holiday);
        }
        return null;
    }

    /// <summary>
    /// Previous public holiday on or before the given date.
    /// Returns null if none found within 365 days.
    /// </summary>
    public (DateTime Date, PersianHoliday Holiday)? PreviousPublicHoliday(DateTime date)
    {
        for (var d = date.Date; d >= date.Date.AddDays(-365); d = d.AddDays(-1))
        {
            if (!IsPublicHoliday(d)) continue;
            var (_, month, day) = PersianCalendarHelper.ToJalali(d);
            int jalaliYear = PersianCalendarHelper.Year(d);
            var holiday = _provider.GetHolidays(jalaliYear)
                .First(h => h.Month == month && h.Day == day);
            return (d, holiday);
        }
        return null;
    }

    /// <summary>
    /// If the date is a non-working day, returns the next working day.
    /// Otherwise returns the same date.
    /// </summary>
    public DateTime NextWorkday(DateTime date)
    {
        var d = date.Date;
        while (IsNonWorkingDay(d)) d = d.AddDays(1);
        return d;
    }
}
