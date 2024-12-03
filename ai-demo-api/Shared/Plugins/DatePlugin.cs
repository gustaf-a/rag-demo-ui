using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Shared.Plugins;

/// <summary>
/// A plugin that provides date related features.
/// </summary>
#pragma warning disable CA1822 // Mark members as static
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
    public string GetTimeFromNowToDate([Description("Date in yyyy-MM-dd format.")] string date)
    {
        if (DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime targetDate))
        {
            var difference = targetDate - DateTime.UtcNow;
            return difference.TotalDays > 0
                ? $"{difference.Days} days from now"
                : $"{Math.Abs(difference.Days)} days ago";
        }
        else
        {
            return "Invalid date format. Please use yyyy-MM-dd format.";
        }
    }

    [KernelFunction("get_days_between_dates")]
    [Description("Returns the number of days between two dates.")]
    public int GetDaysBetweenDates(
        [Description("Start date in yyyy-MM-dd format.")] string startDate,
        [Description("End date in yyyy-MM-dd format.")] string endDate)
    {
        if (DateTime.TryParseExact(startDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime start) &&
            DateTime.TryParseExact(endDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime end))
        {
            return (end - start).Days;
        }
        else
        {
            throw new ArgumentException("Invalid date format. Please use yyyy-MM-dd format.");
        }
    }

    [KernelFunction("get_day_of_week")]
    [Description("Returns the day of the week for a given date.")]
    public string GetDayOfWeek([Description("Date in yyyy-MM-dd format.")] string date)
    {
        if (DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime targetDate))
        {
            return targetDate.DayOfWeek.ToString();
        }
        else
        {
            return "Invalid date format. Please use yyyy-MM-dd format.";
        }
    }

    [KernelFunction("add_days_to_date")]
    [Description("Adds a specified number of days to a given date.")]
    public string AddDaysToDate(
        [Description("Date in yyyy-MM-dd format.")] string date,
        [Description("Number of days to add as integer.")] string days)
    {
        if(!int.TryParse(days, out var daysInt))
            return "Invalid days format. Must be integer.";

        if (DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime targetDate))
            return targetDate.AddDays(daysInt).ToString("yyyy-MM-dd");
        else
            return "Invalid date format. Please use yyyy-MM-dd format.";
    }
}
