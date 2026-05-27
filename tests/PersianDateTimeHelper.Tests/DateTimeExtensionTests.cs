using PersianDateTimeHelper.Extensions;
using Xunit;

namespace PersianDateTimeHelper.Tests;

public class DateTimeExtensionTests
{
    [Fact] public void IsNonWorkingDay_Nowruz_True()
        => Assert.True(new DateTime(2024, 3, 20).IsNonWorkingDay());

    [Fact] public void IsNonWorkingDay_Friday_True()
        => Assert.True(new DateTime(2024, 4, 5).IsNonWorkingDay());

    [Fact] public void IsNonWorkingDay_Workday_False()
        => Assert.False(new DateTime(2024, 3, 25).IsNonWorkingDay());

    [Fact] public void IsPublicHoliday_Nowruz_True()
        => Assert.True(new DateTime(2024, 3, 20).IsPublicHoliday());

    [Fact] public void IsPublicHoliday_PlainFriday_False()
        => Assert.False(new DateTime(2024, 4, 5).IsPublicHoliday());

    [Fact] public void IsWorkday_Monday_True()
        => Assert.True(new DateTime(2024, 3, 25).IsWorkday());

    [Fact] public void ToShamsi_Nowruz_Correct()
        => Assert.Equal("1403/01/01", new DateTime(2024, 3, 20).ToShamsi());

    [Fact] public void PersianDayName_Wednesday_Correct()
        => Assert.Equal("چهارشنبه", new DateTime(2024, 3, 27).PersianDayName());

    [Fact] public void PersianMonthName_Nowruz_IsFarvardin()
        => Assert.Equal("فروردین", new DateTime(2024, 3, 20).PersianMonthName());

    [Fact] public void DailyWorkHours_RegularDay_Returns8()
        => Assert.Equal(8.0, new DateTime(2024, 3, 25).DailyWorkHours());

    [Fact] public void DailyWorkHours_Holiday_ReturnsZero()
        => Assert.Equal(0.0, new DateTime(2024, 3, 20).DailyWorkHours());

    [Fact] public void OvertimeHours_10HoursOnWorkday_Returns2()
        => Assert.Equal(2.0, new DateTime(2024, 3, 25).OvertimeHours(10.0));

    [Fact] public void HolidayName_Nowruz_NotNull()
    {
        var name = new DateTime(2024, 3, 20).HolidayName();
        Assert.NotNull(name);
        Assert.Contains("نوروز", name);
    }

    [Fact] public void HolidayName_Workday_Null()
        => Assert.Null(new DateTime(2024, 3, 25).HolidayName());

    [Fact] public void NextWorkday_OnNowruz_SkipsHolidays()
    {
        var next = new DateTime(2024, 3, 20).NextWorkday();
        Assert.True(next > new DateTime(2024, 3, 20));
        Assert.True(next.IsWorkday());
    }

    [Fact] public void GetDaySummary_HasCorrectPersianDate()
        => Assert.Equal("1403/01/06", new DateTime(2024, 3, 25).GetDaySummary().PersianDate);
}
