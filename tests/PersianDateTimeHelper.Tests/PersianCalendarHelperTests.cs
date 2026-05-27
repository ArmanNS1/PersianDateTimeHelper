using PersianDateTimeHelper.Services;
using Xunit;

namespace PersianDateTimeHelper.Tests;

public class PersianCalendarHelperTests
{
    [Theory]
    [InlineData(2024, 3, 20, 1403, 1, 1)]
    [InlineData(2024, 6, 21, 1403, 4, 1)]
    [InlineData(2023, 3, 21, 1402, 1, 1)]
    [InlineData(2024, 12, 31, 1403, 10, 11)]
    [InlineData(2025, 3, 20, 1403, 12, 30)]
    public void ToJalali_ConvertsCorrectly(
        int gYear, int gMonth, int gDay, int jYear, int jMonth, int jDay)
    {
        var (y, m, d) = PersianCalendarHelper.ToJalali(new DateTime(gYear, gMonth, gDay));
        Assert.Equal(jYear, y);
        Assert.Equal(jMonth, m);
        Assert.Equal(jDay, d);
    }

    [Theory]
    [InlineData(1403, 1, 1,  2024, 3, 20)]
    [InlineData(1402, 1, 1,  2023, 3, 21)]
    [InlineData(1403, 6, 31, 2024, 9, 21)]
    public void ToGregorian_ConvertsCorrectly(
        int jYear, int jMonth, int jDay, int gYear, int gMonth, int gDay)
    {
        var result = PersianCalendarHelper.ToGregorian(jYear, jMonth, jDay);
        Assert.Equal(new DateTime(gYear, gMonth, gDay), result.Date);
    }

    [Fact]
    public void ToShamsi_FormatsWithSlash()
        => Assert.Equal("1403/01/01", PersianCalendarHelper.ToShamsi(new DateTime(2024, 3, 20)));

    [Fact]
    public void ToShamsi_CustomSeparator()
        => Assert.Equal("1403-01-01", PersianCalendarHelper.ToShamsi(new DateTime(2024, 3, 20), "-"));

    [Theory]
    [InlineData(1,  "فروردین")]
    [InlineData(6,  "شهریور")]
    [InlineData(12, "اسفند")]
    public void MonthName_ReturnsCorrectPersianName(int month, string expected)
        => Assert.Equal(expected, PersianCalendarHelper.MonthName(month));

    [Theory]
    [InlineData(1,  "Farvardin")]
    [InlineData(7,  "Mehr")]
    [InlineData(12, "Esfand")]
    public void MonthNameEn_ReturnsCorrectEnglishName(int month, string expected)
        => Assert.Equal(expected, PersianCalendarHelper.MonthNameEn(month));

    [Fact]
    public void DayName_Friday_ReturnsJomeh()
        => Assert.Equal("جمعه", PersianCalendarHelper.DayName(new DateTime(2024, 4, 5)));

    [Fact]
    public void MonthName_InvalidMonth_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() => PersianCalendarHelper.MonthName(0));

    [Theory]
    [InlineData(1403, true)]
    [InlineData(1402, false)]
    [InlineData(1399, true)]
    public void IsLeapYear_ReturnsCorrectValue(int year, bool expected)
        => Assert.Equal(expected, PersianCalendarHelper.IsLeapYear(year));

    [Fact]
    public void IsWeekend_Friday_IsTrue()
        => Assert.True(PersianCalendarHelper.IsWeekend(new DateTime(2024, 4, 5)));

    [Fact]
    public void IsWeekend_Saturday_IsFalse_ByDefault()
        => Assert.False(PersianCalendarHelper.IsWeekend(new DateTime(2024, 3, 23)));

    [Fact]
    public void IsWeekend_Thursday_TrueWhenIncluded()
        => Assert.True(PersianCalendarHelper.IsWeekend(new DateTime(2024, 3, 21), includeThursday: true));

    [Fact]
    public void MonthRange_Farvardin1403_Has31Days()
    {
        var (first, last) = PersianCalendarHelper.MonthRange(1403, 1);
        Assert.Equal(new DateTime(2024, 3, 20), first.Date);
        Assert.Equal(31, (last - first).Days + 1);
    }

    [Fact]
    public void Year_Month_Day_ReturnCorrectComponents()
    {
        var date = new DateTime(2024, 3, 20);
        Assert.Equal(1403, PersianCalendarHelper.Year(date));
        Assert.Equal(1,    PersianCalendarHelper.Month(date));
        Assert.Equal(1,    PersianCalendarHelper.Day(date));
    }
}
