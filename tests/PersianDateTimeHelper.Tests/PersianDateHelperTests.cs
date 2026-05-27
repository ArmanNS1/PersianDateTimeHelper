using IranDateTime.Services;
using Xunit;

namespace IranDateTime.Tests;

public class PersianDateHelperTests
{
    // ── ToJalali ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(2024, 3, 20, 1403, 1, 1)]   // Nowruz 1403
    [InlineData(2024, 6, 21, 1403, 4, 1)]   // Start of Tir
    [InlineData(2023, 3, 21, 1402, 1, 1)]   // Nowruz 1402
    [InlineData(2024, 12, 31, 1403, 10, 11)] // End of 2024
    [InlineData(2025, 3, 20, 1403, 12, 30)] // Last day of 1403
    public void ToJalali_ConvertsCorrectly(
        int gYear, int gMonth, int gDay,
        int jYear, int jMonth, int jDay)
    {
        var gregorian = new DateTime(gYear, gMonth, gDay);
        var (y, m, d) = PersianDateHelper.ToJalali(gregorian);
        Assert.Equal(jYear,  y);
        Assert.Equal(jMonth, m);
        Assert.Equal(jDay,   d);
    }

    // ── ToGregorian ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1403, 1, 1,   2024, 3, 20)]
    [InlineData(1402, 1, 1,   2023, 3, 21)]
    [InlineData(1403, 6, 31,  2024, 9, 21)]
    public void ToGregorian_ConvertsCorrectly(
        int jYear, int jMonth, int jDay,
        int gYear, int gMonth, int gDay)
    {
        var result = PersianDateHelper.ToGregorian(jYear, jMonth, jDay);
        Assert.Equal(new DateTime(gYear, gMonth, gDay), result.Date);
    }

    // ── ToShamsiString ────────────────────────────────────────────────────────

    [Fact]
    public void ToShamsiString_FormatsWithSlash()
    {
        var date = new DateTime(2024, 3, 20); // Nowruz
        Assert.Equal("1403/01/01", PersianDateHelper.ToShamsiString(date));
    }

    [Fact]
    public void ToShamsiString_CustomSeparator()
    {
        var date = new DateTime(2024, 3, 20);
        Assert.Equal("1403-01-01", PersianDateHelper.ToShamsiString(date, "-"));
    }

    // ── Names ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1,  "فروردین")]
    [InlineData(6,  "شهریور")]
    [InlineData(12, "اسفند")]
    public void GetMonthNameFa_ReturnsCorrectName(int month, string expected)
        => Assert.Equal(expected, PersianDateHelper.GetMonthNameFa(month));

    [Theory]
    [InlineData(1,  "Farvardin")]
    [InlineData(7,  "Mehr")]
    [InlineData(12, "Esfand")]
    public void GetMonthNameEn_ReturnsCorrectName(int month, string expected)
        => Assert.Equal(expected, PersianDateHelper.GetMonthNameEn(month));

    [Fact]
    public void GetDayNameFa_Friday_ReturnsJomeh()
    {
        // March 22 2024 is a Friday
        var friday = new DateTime(2024, 3, 22);
        Assert.Equal("جمعه", PersianDateHelper.GetDayNameFa(friday));
    }

    [Fact]
    public void GetMonthNameFa_InvalidMonth_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() => PersianDateHelper.GetMonthNameFa(0));

    // ── Leap year ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1403, true)]   // 1403 is a leap year
    [InlineData(1402, false)]
    [InlineData(1399, true)]
    public void IsLeapYear_ReturnsCorrectValue(int year, bool expected)
        => Assert.Equal(expected, PersianDateHelper.IsLeapYear(year));

    // ── Weekend ───────────────────────────────────────────────────────────────

    [Fact]
    public void IsWeekend_Friday_IsTrue()
    {
        var friday = new DateTime(2024, 3, 22); // Friday
        Assert.True(PersianDateHelper.IsWeekend(friday));
    }

    [Fact]
    public void IsWeekend_Saturday_IsFalse_WhenThursdayNotIncluded()
    {
        var saturday = new DateTime(2024, 3, 23);
        Assert.False(PersianDateHelper.IsWeekend(saturday));
    }

    [Fact]
    public void IsWeekend_Thursday_IsTrueWhenIncluded()
    {
        var thursday = new DateTime(2024, 3, 21);
        Assert.True(PersianDateHelper.IsWeekend(thursday, includeThursday: true));
    }

    // ── Month range ───────────────────────────────────────────────────────────

    [Fact]
    public void GetJalaliMonthRange_Farvardin1403_Correct()
    {
        var (first, last) = PersianDateHelper.GetJalaliMonthRange(1403, 1);
        Assert.Equal(new DateTime(2024, 3, 20), first.Date); // 1403/01/01
        Assert.Equal(31, (last - first).Days + 1);           // Farvardin has 31 days
    }
}
