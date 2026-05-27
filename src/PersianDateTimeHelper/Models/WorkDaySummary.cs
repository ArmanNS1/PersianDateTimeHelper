namespace PersianDateTimeHelper.Models;

/// <summary>Summary of a single day's working hours and status.</summary>
public sealed class WorkDaySummary
{
    /// <summary>The Gregorian date.</summary>
    public DateTime Date { get; init; }

    /// <summary>Persian date string (e.g. "1403/01/15").</summary>
    public string PersianDate { get; init; } = string.Empty;

    /// <summary>Persian day name (e.g. "شنبه").</summary>
    public string DayNameFa { get; init; } = string.Empty;

    /// <summary>Whether this day is an official public holiday.</summary>
    public bool IsPublicHoliday { get; init; }

    /// <summary>Whether this day is an Iranian weekend (Friday, or Thursday+Friday).</summary>
    public bool IsWeekend { get; init; }

    /// <summary>Holiday name in Persian, if applicable.</summary>
    public string? HolidayName { get; init; }

    /// <summary>Regular working hours for this day (0 if holiday/weekend).</summary>
    public double RegularHours { get; init; }

    /// <summary>Overtime hours beyond the official daily threshold.</summary>
    public double OvertimeHours { get; init; }

    /// <summary>Overtime rate multiplier applied (e.g. 1.4).</summary>
    public double OvertimeRate { get; init; }

    /// <summary>Effective total hours including overtime premium.</summary>
    public double EffectiveHours => RegularHours + (OvertimeHours * OvertimeRate);
}
