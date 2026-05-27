using PersianDateTimeHelper.Models;
using PersianDateTimeHelper.Services;
using Xunit;

namespace PersianDateTimeHelper.Tests;

public class PersianHolidayServiceTests
{
    private readonly PersianHolidayService _svc = new();

    // ── Core checks ───────────────────────────────────────────────────────────

    [Fact]
    public void IsNonWorkingDay_Nowruz_True()
        => Assert.True(_svc.IsNonWorkingDay(new DateTime(2024, 3, 20)));

    [Fact]
    public void IsNonWorkingDay_Friday_True()
        => Assert.True(_svc.IsNonWorkingDay(new DateTime(2024, 4, 5)));

    [Fact]
    public void IsPublicHoliday_Nowruz_True()
        => Assert.True(_svc.IsPublicHoliday(new DateTime(2024, 3, 20)));

    [Fact]
    public void IsPublicHoliday_PlainFriday_False()
    {
        // 2024-04-05 = 1403/01/17 — Friday with no official holiday
        var friday = new DateTime(2024, 4, 5);
        Assert.False(_svc.IsPublicHoliday(friday));
        Assert.True(_svc.IsWeekend(friday));
    }

    [Fact]
    public void IsWorkday_Monday_True()
        => Assert.True(_svc.IsWorkday(new DateTime(2024, 3, 25)));

    [Fact]
    public void IsWorkday_Nowruz_False()
        => Assert.False(_svc.IsWorkday(new DateTime(2024, 3, 20)));

    [Fact]
    public void IsPublicHoliday_IslamicRepublicDay_True()
        // 1403/01/12 = March 31, 2024
        => Assert.True(_svc.IsPublicHoliday(new DateTime(2024, 3, 31)));

    [Fact]
    public void IsPublicHoliday_NatureDay_True()
        // 1403/01/13 = April 1, 2024
        => Assert.True(_svc.IsPublicHoliday(new DateTime(2024, 4, 1)));

    [Fact]
    public void IsPublicHoliday_RevolutionDay_True()
        // 1403/11/22 = February 10, 2025
        => Assert.True(_svc.IsPublicHoliday(new DateTime(2025, 2, 10)));

    // ── GetHolidays ───────────────────────────────────────────────────────────

    [Fact]
    public void GetHolidays_Farvardin_HasAtLeastSixDays()
        => Assert.True(_svc.GetHolidays(1403, 1).Count >= 6);

    [Fact]
    public void GetHolidays_InvalidMonth_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => _svc.GetHolidays(1403, 13));

    [Fact]
    public void GetHolidays_Bahman_HasRevolutionDay()
    {
        var holidays = _svc.GetHolidays(1403, 11);
        Assert.Contains(holidays, h => h.Day == 22 && h.TitleEn.Contains("Revolution"));
    }

    [Fact]
    public void GetHolidays_Year_ReturnsNonEmpty()
        => Assert.NotEmpty(_svc.GetHolidays(1403));

    [Fact]
    public void GetHolidays_Year_AreOrdered()
    {
        var holidays = _svc.GetHolidays(1403).ToList();
        for (int i = 1; i < holidays.Count; i++)
        {
            var prev = holidays[i - 1];
            var curr = holidays[i];
            Assert.True(curr.Month > prev.Month ||
                        (curr.Month == prev.Month && curr.Day >= prev.Day));
        }
    }

    [Fact]
    public void GetHolidays_Range_NowruzWeek_ReturnsFourDays()
    {
        // 1403/01/01–04 = March 20–23, 2024
        var result = _svc.GetHolidays(new DateTime(2024, 3, 20), new DateTime(2024, 3, 23));
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void GetHolidays_InvalidRange_Throws()
        => Assert.Throws<ArgumentException>(
            () => _svc.GetHolidays(new DateTime(2024, 4, 1), new DateTime(2024, 3, 1)));

    // ── Count ─────────────────────────────────────────────────────────────────

    [Fact]
    public void CountPublicHolidays_Year_AtLeast20()
        => Assert.True(_svc.CountPublicHolidays(1403) >= 20);

    [Fact]
    public void CountWorkdays_Month_LessThanDaysInMonth()
    {
        int workdays = _svc.CountWorkdays(1403, 1);
        Assert.True(workdays < 31 && workdays > 0);
    }

    [Fact]
    public void CountWorkdays_Plus_NonWorkingDays_EqualsTotalDays()
    {
        int workdays   = _svc.CountWorkdays(1403);
        int nonWorking = _svc.CountNonWorkingDays(1403);
        int total      = PersianCalendarHelper.IsLeapYear(1403) ? 366 : 365;
        Assert.Equal(total, workdays + nonWorking);
    }

    [Fact]
    public void CountWorkdays_Range_OneWeek()
    {
        // Mon–Fri March 25–29 2024, no holidays → 4 workdays (Fri is weekend)
        Assert.Equal(4, _svc.CountWorkdays(new DateTime(2024, 3, 25), new DateTime(2024, 3, 29)));
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    [Fact]
    public void NextPublicHoliday_FromBeforeOilDay_ReturnsOilDay()
    {
        // 2024-03-18 = 1402/12/28 — next holiday is Oil Day (1402/12/29 = 2024-03-19)
        var result = _svc.NextPublicHoliday(new DateTime(2024, 3, 18));
        Assert.NotNull(result);
        Assert.Equal(new DateTime(2024, 3, 19), result!.Value.Date.Date);
    }

    [Fact]
    public void NextPublicHoliday_FromNowruz_ReturnsNowruz()
    {
        var nowruz = new DateTime(2024, 3, 20);
        var result = _svc.NextPublicHoliday(nowruz);
        Assert.NotNull(result);
        Assert.Equal(nowruz.Date, result!.Value.Date.Date);
    }

    [Fact]
    public void NextWorkday_OnNowruz_SkipsHolidays()
    {
        var next = _svc.NextWorkday(new DateTime(2024, 3, 20));
        Assert.True(next > new DateTime(2024, 3, 20));
        Assert.True(_svc.IsWorkday(next));
    }

    // ── GetHolidayDetails ─────────────────────────────────────────────────────

    [Fact]
    public void GetHolidayDetails_Nowruz_ReturnsPersianTitle()
    {
        var info = _svc.GetHolidayDetails(new DateTime(2024, 3, 20));
        Assert.NotNull(info);
        Assert.Contains("نوروز", info!.TitleFa);
    }

    [Fact]
    public void GetHolidayDetails_RegularDay_ReturnsNull()
        => Assert.Null(_svc.GetHolidayDetails(new DateTime(2024, 3, 25)));

    // ── Custom provider ───────────────────────────────────────────────────────

    [Fact]
    public void CustomProvider_OverridesDefault()
    {
        var provider = new TestHolidayProvider(new[]
        {
            new PersianHoliday { Month = 5, Day = 5, TitleFa = "تعطیل شرکت",
                TitleEn = "Company Day", IsFixed = true, Type = HolidayType.Custom }
        });
        var svc = new PersianHolidayService(provider);
        // 1403/05/05 = July 26, 2024
        Assert.True(svc.IsPublicHoliday(new DateTime(2024, 7, 26)));
    }
}

file sealed class TestHolidayProvider : PersianDateTimeHelper.Abstractions.IHolidayProvider
{
    private readonly IEnumerable<PersianHoliday> _holidays;
    public TestHolidayProvider(IEnumerable<PersianHoliday> holidays) => _holidays = holidays;
    public IEnumerable<PersianHoliday> GetHolidays(int jalaliYear) => _holidays;
}
