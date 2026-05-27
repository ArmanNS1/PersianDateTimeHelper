using PersianDateTimeHelper.Models;

namespace PersianDateTimeHelper.Services;

/// <summary>
/// Service for calculating working hours, overtime, and work schedules
/// according to Iranian labour law.
/// </summary>
public sealed class PersianWorkCalculator
{
    private readonly PersianHolidayService _holidayService;
    private readonly WorkConfig _config;

    public PersianWorkCalculator(
        PersianHolidayService? holidayService = null,
        WorkConfig? config = null)
    {
        _holidayService = holidayService ?? new PersianHolidayService();
        _config = config ?? new WorkConfig();
    }

    /// <summary>The WorkConfig in use.</summary>
    public WorkConfig Config => _config;

    // ── Daily summary ─────────────────────────────────────────────────────────

    /// <summary>
    /// Full work-day summary for the given date.
    /// Pass <paramref name="actualHoursWorked"/> to calculate overtime;
    /// omit to use the configured net daily hours.
    /// </summary>
    public WorkDaySummary GetDaySummary(DateTime date, double? actualHoursWorked = null)
    {
        bool isWeekend    = _holidayService.IsWeekend(date);
        bool isHoliday    = _holidayService.IsPublicHoliday(date);
        bool isNonWorking = isWeekend || isHoliday;
        var  holidayInfo  = _holidayService.GetHolidayDetails(date);

        double actual = actualHoursWorked ?? (isNonWorking ? 0 : _config.NetWorkingHoursPerDay);
        double regular, overtime, overtimeRate;

        if (isNonWorking)
        {
            regular      = 0;
            overtime     = actual;
            overtimeRate = _config.OvertimeRateHoliday;
        }
        else
        {
            regular      = Math.Min(actual, _config.OfficialDailyHours);
            overtime     = Math.Max(0, actual - _config.OfficialDailyHours);
            overtimeRate = _config.OvertimeRateWeekday;
        }

        var (y, m, d) = PersianCalendarHelper.ToJalali(date);

        return new WorkDaySummary
        {
            Date          = date.Date,
            PersianDate   = $"{y}/{m:D2}/{d:D2}",
            DayNameFa     = PersianCalendarHelper.DayName(date),
            IsPublicHoliday = isHoliday,
            IsWeekend     = isWeekend,
            HolidayName   = holidayInfo?.TitleFa
                            ?? (isWeekend ? (date.DayOfWeek == DayOfWeek.Friday ? "جمعه" : "پنجشنبه") : null),
            RegularHours  = regular,
            OvertimeHours = overtime,
            OvertimeRate  = overtimeRate
        };
    }

    // ── Daily hours ───────────────────────────────────────────────────────────

    /// <summary>Scheduled working hours for a day (0 if holiday/weekend).</summary>
    public double DailyWorkHours(DateTime date)
        => _holidayService.IsNonWorkingDay(date) ? 0 : _config.NetWorkingHoursPerDay;

    /// <summary>
    /// Overtime hours for a day given actual hours worked.
    /// On holidays/weekends all hours are overtime.
    /// On weekdays: max(0, actual - OfficialDailyHours).
    /// </summary>
    public double OvertimeHours(DateTime date, double actualHoursWorked)
        => _holidayService.IsNonWorkingDay(date)
            ? actualHoursWorked
            : Math.Max(0, actualHoursWorked - _config.OfficialDailyHours);

    /// <summary>Overtime rate multiplier for the given day.</summary>
    public double OvertimeRate(DateTime date)
        => _holidayService.IsNonWorkingDay(date)
            ? _config.OvertimeRateHoliday
            : _config.OvertimeRateWeekday;

    /// <summary>
    /// Effective (payable) hours for a day including overtime premium.
    /// Formula: regular + (overtime × rate).
    /// </summary>
    public double EffectiveHours(DateTime date, double actualHoursWorked)
        => GetDaySummary(date, actualHoursWorked).EffectiveHours;

    // ── Work start / end ──────────────────────────────────────────────────────

    /// <summary>Work-start DateTime for a date, or null if non-working day.</summary>
    public DateTime? WorkStartTime(DateTime date)
        => _holidayService.IsNonWorkingDay(date) ? null : date.Date + _config.WorkStartTime;

    /// <summary>Work-end DateTime for a date, or null if non-working day.</summary>
    public DateTime? WorkEndTime(DateTime date)
        => _holidayService.IsNonWorkingDay(date) ? null : date.Date + _config.WorkEndTime;

    // ── Schedules ─────────────────────────────────────────────────────────────

    /// <summary>Work-day summary for every day in a Jalali month.</summary>
    public IReadOnlyList<WorkDaySummary> GetSchedule(int jalaliYear, int jalaliMonth)
    {
        var (first, last) = PersianCalendarHelper.MonthRange(jalaliYear, jalaliMonth);
        return Enumerable.Range(0, (last - first).Days + 1)
            .Select(i => GetDaySummary(first.AddDays(i)))
            .ToList();
    }

    /// <summary>Work-day summary for every day in a Jalali year.</summary>
    public IReadOnlyList<WorkDaySummary> GetSchedule(int jalaliYear)
    {
        var (first, last) = PersianCalendarHelper.YearRange(jalaliYear);
        return Enumerable.Range(0, (last - first).Days + 1)
            .Select(i => GetDaySummary(first.AddDays(i)))
            .ToList();
    }

    // ── Totals ────────────────────────────────────────────────────────────────

    /// <summary>Total scheduled working hours in a Jalali month.</summary>
    public double TotalWorkHours(int jalaliYear, int jalaliMonth)
        => GetSchedule(jalaliYear, jalaliMonth).Sum(d => d.RegularHours);

    /// <summary>Total scheduled working hours in a Jalali year.</summary>
    public double TotalWorkHours(int jalaliYear)
        => GetSchedule(jalaliYear).Sum(d => d.RegularHours);

    /// <summary>Count working days between two Gregorian dates (inclusive).</summary>
    public int CountWorkdays(DateTime from, DateTime to)
        => _holidayService.CountWorkdays(from, to);
}
