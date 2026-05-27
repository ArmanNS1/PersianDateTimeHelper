using PersianDateTimeHelper.Abstractions;
using PersianDateTimeHelper.Models;
using PersianDateTimeHelper.Services;
using Microsoft.Extensions.DependencyInjection;

namespace PersianDateTimeHelper.Extensions;

/// <summary>DI registration extensions for ASP.NET Core.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register PersianDateTimeHelper services with default configuration.
    /// <code>services.AddPersianDateTimeHelper();</code>
    /// </summary>
    public static IServiceCollection AddPersianDateTimeHelper(this IServiceCollection services)
        => services.AddPersianDateTimeHelper(_ => { });

    /// <summary>
    /// Register PersianDateTimeHelper services with custom work configuration.
    /// <code>
    /// services.AddPersianDateTimeHelper(cfg => {
    ///     cfg.WorkStartTime = new TimeSpan(8, 30, 0);
    ///     cfg.WorkEndTime   = new TimeSpan(17, 30, 0);
    /// });
    /// </code>
    /// </summary>
    public static IServiceCollection AddPersianDateTimeHelper(
        this IServiceCollection services,
        Action<WorkConfig> configureWork)
        => services.AddPersianDateTimeHelper(configureWork, null);

    /// <summary>
    /// Register PersianDateTimeHelper services with a custom holiday provider and/or work config.
    /// <code>
    /// services.AddPersianDateTimeHelper(
    ///     cfg => cfg.OvertimeRateHoliday = 1.5,
    ///     provider: new MyDatabaseHolidayProvider());
    /// </code>
    /// </summary>
    public static IServiceCollection AddPersianDateTimeHelper(
        this IServiceCollection services,
        Action<WorkConfig>? configureWork,
        IHolidayProvider? provider)
    {
        services.AddSingleton<WorkConfig>(sp =>
        {
            var cfg = new WorkConfig();
            configureWork?.Invoke(cfg);
            return cfg;
        });

        if (provider != null)
            services.AddSingleton<IHolidayProvider>(provider);

        services.AddSingleton<PersianHolidayService>(sp =>
        {
            var holidayProvider = sp.GetService<IHolidayProvider>();
            return new PersianHolidayService(holidayProvider);
        });

        services.AddSingleton<PersianWorkCalculator>(sp =>
        {
            var holidayService = sp.GetRequiredService<PersianHolidayService>();
            var config         = sp.GetRequiredService<WorkConfig>();
            return new PersianWorkCalculator(holidayService, config);
        });

        return services;
    }
}
