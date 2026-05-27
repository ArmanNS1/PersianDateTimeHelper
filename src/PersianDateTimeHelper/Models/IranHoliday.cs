namespace PersianDateTimeHelper.Models;

/// <summary>Represents a single Iranian public holiday.</summary>
public sealed class PersianHoliday
{
    /// <summary>Persian (Jalali) month (1–12).</summary>
    public int Month { get; init; }

    /// <summary>Persian (Jalali) day (1–31).</summary>
    public int Day { get; init; }

    /// <summary>Holiday name in Persian.</summary>
    public string TitleFa { get; init; } = string.Empty;

    /// <summary>Holiday name in English.</summary>
    public string TitleEn { get; init; } = string.Empty;

    /// <summary>True if the holiday falls on a fixed Jalali date every year (false = lunar/Islamic).</summary>
    public bool IsFixed { get; init; }

    /// <summary>Category: National, Religious, or Custom.</summary>
    public HolidayType Type { get; init; }
}

public enum HolidayType
{
    National,
    Religious,
    Custom
}
