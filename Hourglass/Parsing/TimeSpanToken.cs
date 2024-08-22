// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeSpanToken.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Parsing;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

using Extensions;
using Properties;

/// <summary>
/// Represents a time interval.
/// </summary>
public sealed class TimeSpanToken : TimerStartToken
{
    /// <summary>
    /// Gets or sets the number of years.
    /// </summary>
    public double Years { get; set; }

    /// <summary>
    /// Gets or sets the number of months.
    /// </summary>
    public double Months { get; set; }

    /// <summary>
    /// Gets or sets the number of weeks.
    /// </summary>
    public double Weeks { get; set; }

    /// <summary>
    /// Gets or sets the number of days.
    /// </summary>
    public double Days { get; set; }

    /// <summary>
    /// Gets or sets the number of hours.
    /// </summary>
    public double Hours { get; set; }

    /// <summary>
    /// Gets or sets the number of minutes.
    /// </summary>
    public double Minutes { get; set; }

    /// <summary>
    /// Gets or sets the number of seconds.
    /// </summary>
    public double Seconds { get; set; }

    /// <summary>
    /// Gets a value indicating whether the token is valid.
    /// </summary>
    protected override bool IsValid =>
        Years >= 0
        && Months >= 0
        && Weeks >= 0
        && Days >= 0
        && Hours >= 0
        && Minutes >= 0
        && Seconds >= 0;

    /// <summary>
    /// Returns the end time for a timer started with this token at a specified time.
    /// </summary>
    /// <param name="startTime">The time the timer is started.</param>
    /// <returns>The end time for a timer started with this token at the specified time.</returns>
    /// <exception cref="InvalidOperationException">The end time is less than start time.</exception>
    public override DateTime GetEndTime(DateTime startTime)
    {
        ThrowIfNotValid();

        DateTime endTime = startTime;

        endTime = endTime.AddSeconds(Seconds);
        endTime = endTime.AddMinutes(Minutes);
        endTime = endTime.AddHours(Hours);
        endTime = endTime.AddDays(Days);
        endTime = endTime.AddWeeks(Weeks);
        endTime = endTime.AddMonths(Months);
        endTime = endTime.AddYears(Years);

        if (endTime < startTime)
        {
            throw new InvalidOperationException(@"The end time is less than start time");
        }

        return endTime;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <param name="provider">An <see cref="IFormatProvider"/> to use.</param>
    /// <returns>A string that represents the current object.</returns>
    protected override string ToString(IFormatProvider provider)
    {
        try
        {
            ThrowIfNotValid();

            List<string> parts = [];

            // Years
            if (!Equals(Years, 0.0))
            {
                parts.Add(GetStringWithUnits(Years, "Year", provider));
            }

            // Months
            if (!Equals(Months, 0.0))
            {
                parts.Add(GetStringWithUnits(Months, "Month", provider));
            }

            // Weeks
            if (!Equals(Weeks, 0.0))
            {
                parts.Add(GetStringWithUnits(Weeks, "Week", provider));
            }

            // Days
            if (!Equals(Days, 0.0))
            {
                parts.Add(GetStringWithUnits(Days, "Day", provider));
            }

            // Hours
            if (!Equals(Hours, 0.0))
            {
                parts.Add(GetStringWithUnits(Hours, "Hour", provider));
            }

            // Minutes
            if (!Equals(Minutes, 0.0))
            {
                parts.Add(GetStringWithUnits(Minutes, "Minute", provider));
            }

            // Seconds
            if (!Equals(Seconds, 0.0) || parts.Count == 0)
            {
                parts.Add(GetStringWithUnits(Seconds, "Second", provider));
            }

            // Join parts
            return string.Join(
                Resources.ResourceManager.GetString(nameof(Resources.TimeSpanTokenUnitSeparator), provider),
                parts);
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            return GetType().ToString();
        }
    }

    /// <summary>
    /// Returns a string for the specified value with the specified unit (e.g., "5 minutes").
    /// </summary>
    /// <param name="value">A value.</param>
    /// <param name="unit">The unit part of the resource name for the unit string.</param>
    /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
    /// <returns>A string for the specified value with the specified unit.</returns>
    private static string GetStringWithUnits(double value, string unit, IFormatProvider provider)
    {
        string resourceName = string.Format(
            CultureInfo.InvariantCulture,
            "TimeSpanToken{0}{1}FormatString",
            Equals(value, 1.0) ? "1" : "N",
            Equals(value, 1.0) ? unit : unit + "s");

        return string.Format(
            Resources.ResourceManager.GetEffectiveProvider(provider),
            Resources.ResourceManager.GetString(resourceName, provider),
            value);
    }

    /// <summary>
    /// Parses <see cref="TimeSpanToken"/> strings.
    /// </summary>
    public new sealed class Parser : TimerStartToken.Parser
    {
        /// <summary>
        /// Singleton instance of the <see cref="Parser"/> class.
        /// </summary>
        public static readonly Parser Instance = new();

        /// <summary>
        /// Prevents a default instance of the <see cref="Parser"/> class from being created.
        /// </summary>
        private Parser()
        {
        }

        /// <summary>
        /// Parses a string into a <see cref="TimerStartToken"/>.
        /// </summary>
        /// <param name="str">A string representation of a <see cref="TimerStartToken"/>.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The <see cref="TimerStartToken"/> parsed from the string.</returns>
        /// <exception cref="FormatException">If <paramref name="str"/> is not a supported representation of a <see cref="TimerStartToken"/>.</exception>
        protected override TimerStartToken ParseInternal(string str, IFormatProvider provider)
        {
            provider = Resources.ResourceManager.GetEffectiveProvider(provider);

            foreach (string pattern in GetPatterns(provider))
            {
                try
                {
                    Match match = Regex.Match(str, pattern, RegexOptions);
                    if (match.Success)
                    {
                        TimeSpanToken timeSpanToken = new();

                        // Years
                        foreach (Capture capture in match.Groups["years"].Captures)
                        {
                            timeSpanToken.Years += double.Parse(capture.Value, provider);
                        }

                        // Months
                        foreach (Capture capture in match.Groups["months"].Captures)
                        {
                            timeSpanToken.Months += double.Parse(capture.Value, provider);
                        }

                        // Weeks
                        foreach (Capture capture in match.Groups["weeks"].Captures)
                        {
                            timeSpanToken.Weeks += double.Parse(capture.Value, provider);
                        }

                        // Days
                        foreach (Capture capture in match.Groups["days"].Captures)
                        {
                            timeSpanToken.Days += double.Parse(capture.Value, provider);
                        }

                        // Hours
                        foreach (Capture capture in match.Groups["hours"].Captures)
                        {
                            timeSpanToken.Hours += double.Parse(capture.Value, provider);
                        }

                        // Minutes
                        foreach (Capture capture in match.Groups["minutes"].Captures)
                        {
                            timeSpanToken.Minutes += double.Parse(capture.Value, provider);
                        }

                        // Seconds
                        foreach (Capture capture in match.Groups["seconds"].Captures)
                        {
                            timeSpanToken.Seconds += double.Parse(capture.Value, provider);
                        }

                        return timeSpanToken;
                    }
                }
                catch (Exception ex) when (ex.CanBeHandled())
                {
                    // Try the next pattern
                }
            }

            // Could not find a matching pattern
            throw new FormatException();
        }

        /// <summary>
        /// Returns a set of regular expressions for matching time spans.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>A set of regular expressions for matching time spans.</returns>
        private static IEnumerable<string> GetPatterns(IFormatProvider provider)
        {
            yield return Resources.ResourceManager.GetString(nameof(Resources.TimeSpanTokenMinutesOnlyPattern), provider);
            yield return Resources.ResourceManager.GetString(nameof(Resources.TimeSpanTokenShortFormPattern), provider);
            yield return Resources.ResourceManager.GetString(nameof(Resources.TimeSpanTokenLongFormPattern), provider);
        }
    }
}