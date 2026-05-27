using PersianDateTimeHelper.Models;
using PersianDateTimeHelper.Services;

namespace PersianDateTimeHelper.Extensions;

/// <summary>
/// Fluent extension methods on <see cref="DateTime"/> for Persian calendar operations.
/// </summary>
public static class DateTimeExtensions
{
    private static readonly PersianHolidayService _holiday = new();
    private static readonly PersianWorkCalculator _work    = new();

    // ── Holiday checks ────────────────────────────────────────────────────────

    /// <summary>True if this date is a public holiday OR weekend.</summary>
    public static bool IsNonWorkingDay(this DateTime date) => _holiday.IsNonWorkingDay(date);

    /// <summary>True if this date is an official public holiday (excluding weekends).</summary>
    public static bool IsPublicHoliday(this DateTime date) => _holiday.IsPublicHoliday(date);

    /// <summary>True if this date is an Iranian weekend (Friday).</summary>
    public static bool IsWeekend(this DateTime date) => _holiday.IsWeekend(date);

    /// <summary>True if this date is a regular working day.</summary>
    public static bool IsWorkday(this DateTime date) => _holiday.IsWorkday(date);

    /// <summary>Persian name of the holiday if today is a public holiday, otherwise null.</summary>
    public static string? HolidayName(this DateTime date)
        => _holiday.GetHolidayDetails(date)?.TitleFa;

    // ── Persian date ──────────────────────────────────────────────────────────

    /// <summary>Persian (Shamsi) date string: "1403/01/15".</summary>
    public static string ToShamsi(this DateTime date)
        => PersianCalendarHelper.ToShamsi(date);

    /// <summary>Jalali (year, month, day) tuple.</summary>
    public static (int Year, int Month, int Day) ToJalali(this DateTime date)
        => PersianCalendarHelper.ToJalali(date);

    /// <summary>Persian day-of-week name e.g. "شنبه".</summary>
    public static string PersianDayName(this DateTime date)
        => PersianCalendarHelper.DayName(date);

    /// <summary>Persian month name e.g. "فروردین".</summary>
    public static string PersianMonthName(this DateTime date)
        => PersianCalendarHelper.MonthName(PersianCalendarHelper.Month(date));

    // ── Working hours ─────────────────────────────────────────────────────────

    /// <summary>Scheduled working hours on this day (0 if holiday/weekend).</summary>
    public static double DailyWorkHours(this DateTime date)
        => _work.DailyWorkHours(date);

    /// <summary>Overtime hours for this day given actual hours worked.</summary>
    public static double OvertimeHours(this DateTime date, double actualHoursWorked)
        => _work.OvertimeHours(date, actualHoursWorked);

    /// <summary>Full work-day summary including overtime and effective hours.</summary>
    public static WorkDaySummary GetDaySummary(this DateTime date, double? actualHoursWorked = null)
        => _work.GetDaySummary(date, actualHoursWorked);

    /// <summary>If non-working day, returns the next working day. Otherwise returns same date.</summary>
    public static DateTime NextWorkday(this DateTime date)
        => _holiday.NextWorkday(date);
}
