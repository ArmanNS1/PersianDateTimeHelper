using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using PersianDateTimeHelper.Abstractions;
using PersianDateTimeHelper.Models;

namespace PersianDateTimeHelper.Services;

/// <summary>Default holiday provider — reads the embedded holidays.json resource.</summary>
internal sealed class EmbeddedHolidayProvider : IHolidayProvider
{
    private static readonly Lazy<IReadOnlyList<PersianHoliday>> _holidays =
        new(LoadHolidays, LazyThreadSafetyMode.ExecutionAndPublication);

    public IEnumerable<PersianHoliday> GetHolidays(int jalaliYear) => _holidays.Value;

    private static IReadOnlyList<PersianHoliday> LoadHolidays()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "PersianDateTimeHelper.Data.holidays.json";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' not found.");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        return JsonSerializer.Deserialize<List<PersianHoliday>>(stream, options)
               ?? throw new InvalidOperationException("Failed to deserialize holidays.json.");
    }
}
