namespace PersianDateTimeHelper.Models;

/// <summary>Configuration for working hours and overtime calculation.</summary>
public sealed class WorkConfig
{
    /// <summary>Time the work day starts. Default: 08:00.</summary>
    public TimeSpan WorkStartTime { get; set; } = new TimeSpan(8, 0, 0);

    /// <summary>Time the work day ends. Default: 17:00.</summary>
    public TimeSpan WorkEndTime { get; set; } = new TimeSpan(17, 0, 0);

    /// <summary>Duration of unpaid break per day. Default: 1 hour.</summary>
    public TimeSpan BreakDuration { get; set; } = new TimeSpan(1, 0, 0);

    /// <summary>Official working hours per day (overtime threshold). Default: 8 hours.</summary>
    public double OfficialDailyHours { get; set; } = 8.0;

    /// <summary>Overtime rate multiplier for weekdays. Default: 1.4 (Iranian labour law).</summary>
    public double OvertimeRateWeekday { get; set; } = 1.4;

    /// <summary>Overtime rate multiplier for holidays/weekends. Default: 1.4.</summary>
    public double OvertimeRateHoliday { get; set; } = 1.4;

    /// <summary>Net working hours per day (end - start - break).</summary>
    public double NetWorkingHoursPerDay =>
        (WorkEndTime - WorkStartTime - BreakDuration).TotalHours;
}
