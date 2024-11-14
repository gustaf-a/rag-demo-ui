using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace RagDemoAPI.Plugins;

//TODO Register in program and test this plugin

/// <summary>
/// A plugin that provides date related features.
/// </summary>
public class DatePlugin : IPlugin
{
    [KernelFunction("get_current_date")]
    [Description("Returns current date.")]
    public string GetCurrentDate()
    {
        return $"{DateTime.UtcNow:yyyy-MM-dd}";
    }

    [KernelFunction("get_time_from_now_to_date")]
    [Description("Returns how long a date is from today.")]
    public string GetTimeFromNowToDate([Description("Date in yyyyMMdd format.")] string date)
    {
        if (DateTime.TryParseExact(date, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime targetDate))
        {
            var difference = targetDate - DateTime.UtcNow;
            return difference.TotalDays > 0
                ? $"{difference.Days} days from now"
                : $"{Math.Abs(difference.Days)} days ago";
        }
        else
        {
            return "Invalid date format. Please use yyyyMMdd format.";
        }
    }

    [KernelFunction("get_days_between_dates")]
    [Description("Returns the number of days between two dates.")]
    public int GetDaysBetweenDates(
        [Description("Start date in yyyyMMdd format.")] string startDate,
        [Description("End date in yyyyMMdd format.")] string endDate)
    {
        if (DateTime.TryParseExact(startDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime start) &&
            DateTime.TryParseExact(endDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime end))
        {
            return (end - start).Days;
        }
        else
        {
            throw new ArgumentException("Invalid date format. Please use yyyyMMdd format.");
        }
    }

    [KernelFunction("get_day_of_week")]
    [Description("Returns the day of the week for a given date.")]
    public string GetDayOfWeek([Description("Date in yyyyMMdd format.")] string date)
    {
        if (DateTime.TryParseExact(date, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime targetDate))
        {
            return targetDate.DayOfWeek.ToString();
        }
        else
        {
            return "Invalid date format. Please use yyyyMMdd format.";
        }
    }

    [KernelFunction("add_days_to_date")]
    [Description("Adds a specified number of days to a given date.")]
    public string AddDaysToDate(
        [Description("Date in yyyyMMdd format.")] string date,
        [Description("Number of days to add.")] int days)
    {
        if (DateTime.TryParseExact(date, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime targetDate))
        {
            return targetDate.AddDays(days).ToString("yyyyMMdd");
        }
        else
        {
            return "Invalid date format. Please use yyyyMMdd format.";
        }
    }
}
