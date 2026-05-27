using System.Globalization;

namespace PersianDateTimeHelper.Services;

/// <summary>
/// Core utilities for Persian (Jalali/Shamsi) date conversion and formatting.
/// </summary>
public static class PersianCalendarHelper
{
    private static readonly PersianCalendar _pc = new();

    // ── Conversion ────────────────────────────────────────────────────────────

    /// <summary>Convert a Gregorian <see cref="DateTime"/> to Jalali (year, month, day).</summary>
    public static (int Year, int Month, int Day) ToJalali(DateTime date)
        => (_pc.GetYear(date), _pc.GetMonth(date), _pc.GetDayOfMonth(date));

    /// <summary>Convert Jalali components to a Gregorian <see cref="DateTime"/>.</summary>
    public static DateTime ToGregorian(int jalaliYear, int jalaliMonth, int jalaliDay)
        => _pc.ToDateTime(jalaliYear, jalaliMonth, jalaliDay, 0, 0, 0, 0);

    /// <summary>Format a Gregorian date as a Shamsi string e.g. "1403/01/15".</summary>
    public static string ToShamsi(DateTime date, string separator = "/")
    {
        var (y, m, d) = ToJalali(date);
        return $"{y}{separator}{m:D2}{separator}{d:D2}";
    }

    // ── Year / Month / Day ────────────────────────────────────────────────────

    /// <summary>Jalali year for a Gregorian date.</summary>
    public static int Year(DateTime date) => _pc.GetYear(date);

    /// <summary>Jalali month (1–12) for a Gregorian date.</summary>
    public static int Month(DateTime date) => _pc.GetMonth(date);

    /// <summary>Jalali day-of-month for a Gregorian date.</summary>
    public static int Day(DateTime date) => _pc.GetDayOfMonth(date);

    /// <summary>Number of days in the given Jalali month.</summary>
    public static int DaysInMonth(int jalaliYear, int jalaliMonth)
        => _pc.GetDaysInMonth(jalaliYear, jalaliMonth);

    /// <summary>True if the given Jalali year is a leap year.</summary>
    public static bool IsLeapYear(int jalaliYear) => _pc.IsLeapYear(jalaliYear);

    // ── Names ─────────────────────────────────────────────────────────────────

    private static readonly string[] _monthNamesFa =
    [
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    ];

    private static readonly string[] _monthNamesEn =
    [
        "Farvardin", "Ordibehesht", "Khordad", "Tir", "Mordad", "Shahrivar",
        "Mehr", "Aban", "Azar", "Dey", "Bahman", "Esfand"
    ];

    private static readonly string[] _dayNamesFa =
    [
        "یکشنبه",   // Sunday
        "دوشنبه",   // Monday
        "سه‌شنبه",  // Tuesday
        "چهارشنبه", // Wednesday
        "پنجشنبه",  // Thursday
        "جمعه",     // Friday
        "شنبه"      // Saturday
    ];

    /// <summary>Persian name of a Jalali month (1–12). e.g. "فروردین".</summary>
    public static string MonthName(int jalaliMonth)
    {
        if (jalaliMonth < 1 || jalaliMonth > 12)
            throw new ArgumentOutOfRangeException(nameof(jalaliMonth), "Month must be 1–12.");
        return _monthNamesFa[jalaliMonth - 1];
    }

    /// <summary>English name of a Jalali month (1–12). e.g. "Farvardin".</summary>
    public static string MonthNameEn(int jalaliMonth)
    {
        if (jalaliMonth < 1 || jalaliMonth > 12)
            throw new ArgumentOutOfRangeException(nameof(jalaliMonth), "Month must be 1–12.");
        return _monthNamesEn[jalaliMonth - 1];
    }

    /// <summary>Persian day-of-week name for a Gregorian date. e.g. "شنبه".</summary>
    public static string DayName(DateTime date) => _dayNamesFa[(int)date.DayOfWeek];

    /// <summary>
    /// True if the given date is an Iranian weekend.
    /// Friday is always a weekend. Pass <paramref name="includeThursday"/> = true
    /// for organisations that observe a Thursday+Friday weekend.
    /// </summary>
    public static bool IsWeekend(DateTime date, bool includeThursday = false)
        => date.DayOfWeek == DayOfWeek.Friday
           || (includeThursday && date.DayOfWeek == DayOfWeek.Thursday);

    // ── Range helpers ─────────────────────────────────────────────────────────

    /// <summary>First and last Gregorian dates of a Jalali month.</summary>
    public static (DateTime First, DateTime Last) MonthRange(int jalaliYear, int jalaliMonth)
    {
        var first = ToGregorian(jalaliYear, jalaliMonth, 1);
        var last  = ToGregorian(jalaliYear, jalaliMonth, DaysInMonth(jalaliYear, jalaliMonth));
        return (first, last);
    }

    /// <summary>First and last Gregorian dates of a Jalali year.</summary>
    public static (DateTime First, DateTime Last) YearRange(int jalaliYear)
    {
        var first = ToGregorian(jalaliYear, 1, 1);
        var last  = ToGregorian(jalaliYear, 12, IsLeapYear(jalaliYear) ? 30 : 29);
        return (first, last);
    }
}
