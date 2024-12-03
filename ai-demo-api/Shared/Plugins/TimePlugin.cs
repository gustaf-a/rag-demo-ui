using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Shared.Plugins;

/// <summary>
/// A plugin that provides time-related features.
/// </summary>
public class TimePlugin : IPlugin
{
    [KernelFunction]
    [Description("Returns the current time in UTC.")]
    public string GetCurrentTime()
    {
        return DateTime.UtcNow.ToString("HH:mm:ss");
    }

    [KernelFunction]
    [Description("Calculates the difference in hours and minutes between two times.")]
    public string GetTimeDifference(
        [Description("Start time in HH:mm format.")] string startTime,
        [Description("End time in HH:mm format.")] string endTime)
    {
        if (TimeSpan.TryParse(startTime, out TimeSpan start) && TimeSpan.TryParse(endTime, out TimeSpan end))
        {
            TimeSpan difference = end - start;
            if (difference < TimeSpan.Zero)
            {
                difference = difference.Add(TimeSpan.FromDays(1)); // Handles cases where the end time is after midnight
            }
            return $"{difference.Hours} hours and {difference.Minutes} minutes";
        }
        else
        {
            return "Invalid time format. Please use HH:mm format.";
        }
    }

    [KernelFunction]
    [Description("Adds a specified number of minutes to a given time.")]
    public string AddMinutesToTime(
        [Description("Time in HH:mm format.")] string time,
        [Description("Number of minutes to add.")] int minutes)
    {
        if (TimeSpan.TryParse(time, out TimeSpan timeSpan))
        {
            var newTime = timeSpan.Add(TimeSpan.FromMinutes(minutes));
            return newTime.ToString(@"hh\:mm");
        }
        else
        {
            return "Invalid time format. Please use HH:mm format.";
        }
    }

    [KernelFunction]
    [Description("Returns the time left until the next occurrence of a given hour and minute.")]
    public string TimeUntilNextOccurrence(
        [Description("Hour in 24-hour format (0-23).")] int hour,
        [Description("Minute (0-59).")] int minute)
    {
        var now = DateTime.UtcNow;
        var targetTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

        if (targetTime <= now)
        {
            targetTime = targetTime.AddDays(1);
        }

        var timeUntil = targetTime - now;
        return $"{timeUntil.Hours} hours and {timeUntil.Minutes} minutes";
    }
}

