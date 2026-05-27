using System.Globalization;

namespace IranDateTime.Services;

/// <summary>
/// Utilities for converting between Gregorian and Persian (Jalali/Shamsi) dates,
/// and for retrieving Persian day/month names.
/// </summary>
public static class PersianDateHelper
{
    private static readonly PersianCalendar _pc = new();

    // ── Conversion ──────────────────────────────────────────────────────────

    /// <summary>Convert a Gregorian <see cref="DateTime"/> to its Jalali components.</summary>
    public static (int Year, int Month, int Day) ToJalali(DateTime date)
    {
        int y = _pc.GetYear(date);
        int m = _pc.GetMonth(date);
        int d = _pc.GetDayOfMonth(date);
        return (y, m, d);
    }

    /// <summary>
    /// Convert Jalali date components to a Gregorian <see cref="DateTime"/>.
    /// Time component defaults to midnight.
    /// </summary>
    public static DateTime ToGregorian(int jalaliYear, int jalaliMonth, int jalaliDay)
        => _pc.ToDateTime(jalaliYear, jalaliMonth, jalaliDay, 0, 0, 0, 0);

    /// <summary>Format a Gregorian date as a Persian date string (e.g. "1403/01/15").</summary>
    public static string ToShamsiString(DateTime date, string separator = "/")
    {
        var (y, m, d) = ToJalali(date);
        return $"{y}{separator}{m:D2}{separator}{d:D2}";
    }

    // ── Year / Month / Day helpers ──────────────────────────────────────────

    /// <summary>Return the Jalali year for a Gregorian date.</summary>
    public static int GetJalaliYear(DateTime date) => _pc.GetYear(date);

    /// <summary>Return the Jalali month (1–12) for a Gregorian date.</summary>
    public static int GetJalaliMonth(DateTime date) => _pc.GetMonth(date);

    /// <summary>Return the Jalali day-of-month for a Gregorian date.</summary>
    public static int GetJalaliDay(DateTime date) => _pc.GetDayOfMonth(date);

    /// <summary>Return the number of days in a given Jalali month.</summary>
    public static int GetDaysInMonth(int jalaliYear, int jalaliMonth)
        => _pc.GetDaysInMonth(jalaliYear, jalaliMonth);

    /// <summary>Return true if the given Jalali year is a leap year.</summary>
    public static bool IsLeapYear(int jalaliYear) => _pc.IsLeapYear(jalaliYear);

    // ── Names ───────────────────────────────────────────────────────────────

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

    /// <summary>Persian name of the given Jalali month (1–12).</summary>
    public static string GetMonthNameFa(int jalaliMonth)
    {
        if (jalaliMonth < 1 || jalaliMonth > 12)
            throw new ArgumentOutOfRangeException(nameof(jalaliMonth), "Month must be 1–12.");
        return _monthNamesFa[jalaliMonth - 1];
    }

    /// <summary>English name of the given Jalali month (1–12).</summary>
    public static string GetMonthNameEn(int jalaliMonth)
    {
        if (jalaliMonth < 1 || jalaliMonth > 12)
            throw new ArgumentOutOfRangeException(nameof(jalaliMonth), "Month must be 1–12.");
        return _monthNamesEn[jalaliMonth - 1];
    }

    /// <summary>Persian day-of-week name for a Gregorian date.</summary>
    public static string GetDayNameFa(DateTime date)
        => _dayNamesFa[(int)date.DayOfWeek];

    /// <summary>
    /// Return true if the given day-of-week is an Iranian weekend.
    /// Iran's official weekend is Friday only (government) or Thursday+Friday (some sectors).
    /// This method flags Friday as always a weekend; set <paramref name="includeThursday"/> = true
    /// to also flag Thursday.
    /// </summary>
    public static bool IsWeekend(DateTime date, bool includeThursday = false)
        => date.DayOfWeek == DayOfWeek.Friday
           || (includeThursday && date.DayOfWeek == DayOfWeek.Thursday);

    // ── Range helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Return the first and last Gregorian dates of a given Jalali month/year.
    /// </summary>
    public static (DateTime First, DateTime Last) GetJalaliMonthRange(int jalaliYear, int jalaliMonth)
    {
        var first = ToGregorian(jalaliYear, jalaliMonth, 1);
        int days  = GetDaysInMonth(jalaliYear, jalaliMonth);
        var last  = ToGregorian(jalaliYear, jalaliMonth, days);
        return (first, last);
    }

    /// <summary>
    /// Return the first and last Gregorian dates of a given Jalali year.
    /// </summary>
    public static (DateTime First, DateTime Last) GetJalaliYearRange(int jalaliYear)
    {
        var first = ToGregorian(jalaliYear, 1, 1);
        var last  = ToGregorian(jalaliYear, 12, IsLeapYear(jalaliYear) ? 30 : 29);
        return (first, last);
    }
}
