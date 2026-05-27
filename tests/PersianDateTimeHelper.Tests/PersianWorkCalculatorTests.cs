using PersianDateTimeHelper.Models;
using PersianDateTimeHelper.Services;
using Xunit;

namespace PersianDateTimeHelper.Tests;

public class PersianWorkCalculatorTests
{
    private readonly PersianWorkCalculator _calc = new();

    // ── DailyWorkHours ────────────────────────────────────────────────────────

    [Fact]
    public void DailyWorkHours_RegularWorkday_Returns8()
        => Assert.Equal(8.0, _calc.DailyWorkHours(new DateTime(2024, 3, 25)));

    [Fact]
    public void DailyWorkHours_Friday_ReturnsZero()
        => Assert.Equal(0.0, _calc.DailyWorkHours(new DateTime(2024, 4, 5)));

    [Fact]
    public void DailyWorkHours_Nowruz_ReturnsZero()
        => Assert.Equal(0.0, _calc.DailyWorkHours(new DateTime(2024, 3, 20)));

    // ── OvertimeHours ─────────────────────────────────────────────────────────

    [Fact]
    public void OvertimeHours_10HoursOnWorkday_Returns2()
        => Assert.Equal(2.0, _calc.OvertimeHours(new DateTime(2024, 3, 25), 10.0));

    [Fact]
    public void OvertimeHours_8HoursOnWorkday_ReturnsZero()
        => Assert.Equal(0.0, _calc.OvertimeHours(new DateTime(2024, 3, 25), 8.0));

    [Fact]
    public void OvertimeHours_AnyHoursOnHoliday_AllAreOvertime()
        => Assert.Equal(5.0, _calc.OvertimeHours(new DateTime(2024, 3, 20), 5.0));

    [Fact]
    public void OvertimeHours_AnyHoursOnFriday_AllAreOvertime()
        => Assert.Equal(4.0, _calc.OvertimeHours(new DateTime(2024, 4, 5), 4.0));

    // ── EffectiveHours ────────────────────────────────────────────────────────

    [Fact]
    public void EffectiveHours_10HoursOnWorkday_IncludesOtPremium()
        // 8 regular + 2 OT × 1.4 = 10.8
        => Assert.Equal(10.8, _calc.EffectiveHours(new DateTime(2024, 3, 25), 10.0), precision: 5);

    [Fact]
    public void EffectiveHours_8HoursOnHoliday_AllAtHolidayRate()
        // 0 regular + 8 OT × 1.4 = 11.2
        => Assert.Equal(11.2, _calc.EffectiveHours(new DateTime(2024, 3, 20), 8.0), precision: 5);

    // ── GetDaySummary ─────────────────────────────────────────────────────────

    [Fact]
    public void GetDaySummary_Workday_HasCorrectFields()
    {
        var summary = _calc.GetDaySummary(new DateTime(2024, 3, 25), actualHoursWorked: 9.0);
        Assert.False(summary.IsPublicHoliday);
        Assert.False(summary.IsWeekend);
        Assert.Equal(8.0, summary.RegularHours);
        Assert.Equal(1.0, summary.OvertimeHours);
        Assert.Equal(1.4, summary.OvertimeRate);
        Assert.Equal("1403/01/06", summary.PersianDate);
        Assert.Equal("دوشنبه", summary.DayNameFa);
    }

    [Fact]
    public void GetDaySummary_Nowruz_IsHolidayWithOvertime()
    {
        var summary = _calc.GetDaySummary(new DateTime(2024, 3, 20), actualHoursWorked: 4.0);
        Assert.True(summary.IsPublicHoliday);
        Assert.Equal(0.0, summary.RegularHours);
        Assert.Equal(4.0, summary.OvertimeHours);
        Assert.NotNull(summary.HolidayName);
        Assert.Contains("نوروز", summary.HolidayName);
    }

    [Fact]
    public void GetDaySummary_NoActualHours_DefaultsToConfiguredHours()
    {
        var summary = _calc.GetDaySummary(new DateTime(2024, 3, 25));
        Assert.Equal(8.0, summary.RegularHours);
        Assert.Equal(0.0, summary.OvertimeHours);
    }

    // ── GetSchedule ───────────────────────────────────────────────────────────

    [Fact]
    public void GetSchedule_Month_Farvardin_Has31Days()
        => Assert.Equal(31, _calc.GetSchedule(1403, 1).Count);

    [Fact]
    public void GetSchedule_Month_FirstDay_IsNowruzHoliday()
    {
        var schedule = _calc.GetSchedule(1403, 1);
        Assert.True(schedule[0].IsPublicHoliday);
        Assert.Equal("1403/01/01", schedule[0].PersianDate);
    }

    // ── TotalWorkHours ────────────────────────────────────────────────────────

    [Fact]
    public void TotalWorkHours_Month_IsReasonable()
    {
        double hours = _calc.TotalWorkHours(1403, 6);
        Assert.True(hours > 100 && hours <= 248, $"Unexpected: {hours}");
    }

    [Fact]
    public void TotalWorkHours_Year_IsReasonable()
    {
        double hours = _calc.TotalWorkHours(1403);
        Assert.True(hours > 1800 && hours <= 2500, $"Unexpected: {hours}");
    }

    // ── WorkStartTime / WorkEndTime ───────────────────────────────────────────

    [Fact]
    public void WorkStartTime_Workday_ReturnsEightAm()
    {
        var start = _calc.WorkStartTime(new DateTime(2024, 3, 25));
        Assert.NotNull(start);
        Assert.Equal(new TimeSpan(8, 0, 0), start!.Value.TimeOfDay);
    }

    [Fact]
    public void WorkStartTime_Holiday_ReturnsNull()
        => Assert.Null(_calc.WorkStartTime(new DateTime(2024, 3, 20)));

    // ── Custom config ─────────────────────────────────────────────────────────

    [Fact]
    public void CustomConfig_DifferentHours_UsedInCalculation()
    {
        var config = new WorkConfig
        {
            WorkStartTime      = new TimeSpan(9, 0, 0),
            WorkEndTime        = new TimeSpan(18, 0, 0),
            BreakDuration      = new TimeSpan(0, 30, 0),
            OfficialDailyHours = 7.5,
            OvertimeRateWeekday = 1.5
        };
        var calc = new PersianWorkCalculator(config: config);
        var monday = new DateTime(2024, 3, 25);
        Assert.Equal(8.5, calc.DailyWorkHours(monday), precision: 5);
        Assert.Equal(1.0, calc.OvertimeHours(monday, 8.5), precision: 5);
    }
}
