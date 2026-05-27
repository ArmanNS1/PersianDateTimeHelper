# PersianDateTimeHelper

A comprehensive .NET 8 library for working with the **Persian (Jalali/Shamsi) calendar**.

Built for Iranian developers who need reliable holiday detection, working hours calculation, overtime, and Jalali date utilities — all in one package with zero heavy dependencies.

[![NuGet](https://img.shields.io/nuget/v/PersianDateTimeHelper.svg)](https://www.nuget.org/packages/PersianDateTimeHelper)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com)

---

## Why PersianDateTimeHelper?

Working with Iranian dates in .NET is painful:
- The built-in `PersianCalendar` gives you raw conversion but nothing else
- No holiday awareness, no working-hours logic, no overtime calculation
- Scattered utility code that every team rewrites from scratch

**PersianDateTimeHelper** solves all of that in one clean, testable package.

---

## Installation

```bash
dotnet add package PersianDateTimeHelper
```

---

## Quick Start

```csharp
using PersianDateTimeHelper.Extensions;

DateTime today = DateTime.Now;

bool working  = today.IsWorkday();         // is today a working day?
string shamsi = today.ToShamsi();          // "1403/07/15"
double hours  = today.DailyWorkHours();    // 8.0 or 0.0
double ot     = today.OvertimeHours(10.5); // 2.5
```

---


## Features at a Glance

| Category | What you get |
|---|---|
| **Holiday detection** | Public holidays, weekends, non-working days |
| **Holiday queries** | Get / count holidays by month, year, or date range |
| **Navigation** | Next/previous holiday, next working day |
| **Working hours** | Daily scheduled hours, overtime, effective (payable) hours |
| **Schedules** | Full month or year work schedule |
| **Totals** | Total work hours per month/year, workday counts |
| **Date conversion** | Jalali ↔ Gregorian, Shamsi string formatting |
| **Persian names** | Month names (FA/EN), day names, leap year |
| **DI support** | `services.AddPersianDateTimeHelper()` for ASP.NET Core |
| **Extensible** | Plug in your own holiday source via `IHolidayProvider` |

---

## Holiday Detection

```csharp
var svc = new PersianHolidayService();

svc.IsNonWorkingDay(date)  // true if public holiday OR weekend
svc.IsPublicHoliday(date)  // true if official holiday only
svc.IsWeekend(date)        // true if Friday (or Thu+Fri if configured)
svc.IsWorkday(date)        // true if a regular working day

// Fluent extension methods
date.IsNonWorkingDay()
date.IsPublicHoliday()
date.IsWeekend()
date.IsWorkday()
date.HolidayName()         // Persian name, or null
```

---

## Querying Holidays

```csharp
// By month / year / range
svc.GetHolidays(1403, 1)                          // Farvardin holidays
svc.GetHolidays(1403)                             // full year
svc.GetHolidays(new DateTime(2024,3,1), new DateTime(2024,4,30))

// Details for a specific date
PersianHoliday? info = svc.GetHolidayDetails(date);
Console.WriteLine(info?.TitleFa);  // "جشن نوروز"
Console.WriteLine(info?.TitleEn);  // "Nowruz (New Year)"
Console.WriteLine(info?.Type);     // National / Religious / Custom
```

---

## Counting Days

```csharp
svc.CountPublicHolidays(1403)           // whole year (no weekends)
svc.CountPublicHolidays(1403, 1)        // just Farvardin
svc.CountNonWorkingDays(1403)           // holidays + weekends, whole year
svc.CountNonWorkingDays(1403, 6)        // Shahrivar
svc.CountWorkdays(1403)
svc.CountWorkdays(1403, 6)
svc.CountWorkdays(from, to)             // between two Gregorian dates
```

---

## Navigation

```csharp
var (date, holiday) = svc.NextPublicHoliday(DateTime.Now)!.Value;
var (date, holiday) = svc.PreviousPublicHoliday(DateTime.Now)!.Value;

DateTime workday = svc.NextWorkday(DateTime.Now);
DateTime workday = DateTime.Now.NextWorkday();   // extension method
```

---

## Working Hours & Overtime

```csharp
var calc = new PersianWorkCalculator();

calc.DailyWorkHours(date)                // 8.0 (0 on non-working days)
calc.OvertimeHours(date, actual)         // max(0, actual - 8) on weekdays
calc.OvertimeHours(holiday, 6.0)         // 6.0 — ALL hours are OT on holidays
calc.OvertimeRate(date)                  // 1.4
calc.EffectiveHours(date, 10.0)          // 8 + 2×1.4 = 10.8

DateTime? start = calc.WorkStartTime(date);  // null on holidays
DateTime? end   = calc.WorkEndTime(date);
```

---

## Daily Summary

```csharp
WorkDaySummary s = calc.GetDaySummary(date, actualHoursWorked: 10.0);
// or via extension:
WorkDaySummary s = date.GetDaySummary(10.0);

s.PersianDate      // "1403/07/15"
s.DayNameFa        // "شنبه"
s.IsPublicHoliday  // false
s.IsWeekend        // false
s.HolidayName      // null or "جشن نوروز"
s.RegularHours     // 8.0
s.OvertimeHours    // 2.0
s.OvertimeRate     // 1.4
s.EffectiveHours   // 10.8
```

---

## Schedules & Totals

```csharp
IReadOnlyList<WorkDaySummary> month = calc.GetSchedule(1403, 6);
IReadOnlyList<WorkDaySummary> year  = calc.GetSchedule(1403);

double monthHours = calc.TotalWorkHours(1403, 6);
double yearHours  = calc.TotalWorkHours(1403);
```

---

## Persian Date Utilities

```csharp
// Conversion
var (y, m, d) = PersianCalendarHelper.ToJalali(DateTime.Now);
DateTime greg = PersianCalendarHelper.ToGregorian(1403, 1, 1);

// Formatting
PersianCalendarHelper.ToShamsi(date)        // "1403/01/01"
PersianCalendarHelper.ToShamsi(date, "-")   // "1403-01-01"
date.ToShamsi()                             // extension method

// Names
PersianCalendarHelper.MonthName(1)          // "فروردین"
PersianCalendarHelper.MonthNameEn(1)        // "Farvardin"
PersianCalendarHelper.DayName(date)         // "شنبه"
date.PersianDayName()
date.PersianMonthName()

// Helpers
PersianCalendarHelper.IsLeapYear(1403)          // true
PersianCalendarHelper.DaysInMonth(1403, 1)      // 31
PersianCalendarHelper.MonthRange(1403, 1)       // (DateTime First, DateTime Last)
PersianCalendarHelper.YearRange(1403)
PersianCalendarHelper.Year(date)                // 1403
PersianCalendarHelper.Month(date)               // 7
PersianCalendarHelper.Day(date)                 // 15
```

---

## Custom Work Configuration

```csharp
var config = new WorkConfig
{
    WorkStartTime       = new TimeSpan(9, 0, 0),
    WorkEndTime         = new TimeSpan(18, 0, 0),
    BreakDuration       = new TimeSpan(0, 30, 0),
    OfficialDailyHours  = 7.5,
    OvertimeRateWeekday = 1.5,
    OvertimeRateHoliday = 1.4,
};

var calc = new PersianWorkCalculator(config: config);
```

---

## Custom Holiday Provider

```csharp
public class MyHolidayProvider : IHolidayProvider
{
    public IEnumerable<PersianHoliday> GetHolidays(int jalaliYear)
        => _db.GetHolidays(jalaliYear);
}

var svc  = new PersianHolidayService(provider: new MyHolidayProvider());
var calc = new PersianWorkCalculator(holidayService: svc);
```

---

## ASP.NET Core Dependency Injection

```csharp
// Program.cs
builder.Services.AddPersianDateTimeHelper();

// With custom config
builder.Services.AddPersianDateTimeHelper(cfg =>
{
    cfg.WorkStartTime       = new TimeSpan(8, 30, 0);
    cfg.WorkEndTime         = new TimeSpan(17, 30, 0);
    cfg.OvertimeRateHoliday = 1.5;
});

// With custom holiday provider
builder.Services.AddPersianDateTimeHelper(
    cfg => cfg.OfficialDailyHours = 7.5,
    provider: new MyHolidayProvider()
);

// Inject normally
public class PayrollService(PersianHolidayService holidays, PersianWorkCalculator work) { }
```

---

## About the Holiday Data

**Fixed holidays** (same Jalali date every year): Nowruz × 4 days, Islamic Republic Day, Nature Day, Death of Imam Khomeini, Uprising of Khordad 15, Revolution Anniversary, Oil Nationalisation Day.

**Religious/lunar holidays**: Dates are approximate — Islamic holidays shift each year with the lunar calendar. For production payroll systems, override with confirmed government announcements via `IHolidayProvider`.

**Weekend**: Friday is always treated as weekend. Pass `includeThursdayWeekend: true` to also flag Thursday.

---

## PersianDateTimeHelper In Action

Want to see PersianDateTimeHelper in action? Check out our **[PersianPayroll-Example repository](https://github.com/ArmanNS1/PersianPayroll-Example)** with real-world scenarios:

A fully functional REST API demonstrating:
- **Monthly payroll calculation** with regular + overtime pay
- **Attendance processing** with automatic holiday detection
- **Calendar API** for workday counting and holiday lookup
- **Year summary** with month-by-month breakdown
- **Swagger documentation** for all endpoints

```bash
git clone https://github.com/ArmanNS1/PersianPayroll-Example.git
```

---

## License

MIT © 2026
