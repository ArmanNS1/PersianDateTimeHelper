using PersianDateTimeHelper.Models;

namespace PersianDateTimeHelper.Abstractions;

/// <summary>
/// Implement this interface to supply a custom holiday data source
/// (e.g. a database, API, or custom JSON file).
/// </summary>
public interface IHolidayProvider
{
    /// <summary>Return all holidays for the given Jalali year.</summary>
    IEnumerable<PersianHoliday> GetHolidays(int jalaliYear);
}
