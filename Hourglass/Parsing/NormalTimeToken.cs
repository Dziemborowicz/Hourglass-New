// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NormalTimeToken.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Parsing;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Extensions;
using Properties;

// ReSharper disable ExceptionNotDocumented

/// <summary>
/// Represents the period of an hour.
/// </summary>
public enum HourPeriod
{
    /// <summary>
    /// Ante meridiem.
    /// </summary>
    Am,

    /// <summary>
    /// Post meridiem.
    /// </summary>
    Pm,

    /// <summary>
    /// Ante or post meridiem, whichever is closest to the current time.
    /// </summary>
    Undefined
}

/// <summary>
/// Represents the time part of an instant in time specified as an hour, minute, and second.
/// </summary>
public sealed class NormalTimeToken : TimeToken
{
    /// <summary>
    /// Gets or sets the period of an hour (AM or PM).
    /// </summary>
    public HourPeriod HourPeriod { get; set; }

    /// <summary>
    /// Gets or sets the hour.
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// Gets or sets the minute.
    /// </summary>
    public int Minute { get; set; }

    /// <summary>
    /// Gets or sets the second.
    /// </summary>
    public int Second { get; set; }

    /// <summary>
    /// Gets a value indicating whether this token represents midnight.
    /// </summary>
    public bool IsMidnight => Hour == 12 && Minute == 0 && Second == 0 && HourPeriod == HourPeriod.Am;

    /// <summary>
    /// Gets a value indicating whether this token represents noon.
    /// </summary>
    public bool IsMidday => Hour == 12 && Minute == 0 && Second == 0 && HourPeriod == HourPeriod.Pm;

    /// <summary>
    /// Gets a value indicating whether the token is valid.
    /// </summary>
    public override bool IsValid =>
        Hour is >= 1 and <= 12
        && Minute is >= 0 and <= 59
        && Second >= 0 && Minute <= 59;

    /// <summary>
    /// Returns the next date and time after <paramref name="minDate"/> that is represented by this token.
    /// </summary>
    /// <remarks>
    /// This method may return a date and time that is before <paramref name="minDate"/> if there is no date and
    /// time after <paramref name="minDate"/> that is represented by this token.
    /// </remarks>
    /// <param name="minDate">The minimum date and time to return.</param>
    /// <param name="datePart">The date part of the date and time to return.</param>
    /// <returns>The next date and time after <paramref name="minDate"/> that is represented by this token.</returns>
    /// <exception cref="InvalidOperationException">If this token is not valid.</exception>
    public override DateTime ToDateTime(DateTime minDate, DateTime datePart)
    {
        ThrowIfNotValid();

        DateTime earlyDateTime = new(
            datePart.Year,
            datePart.Month,
            datePart.Day,
            Hour == 12 ? 0 : Hour,
            Minute,
            Second);

        DateTime lateDateTime = new(
            datePart.Year,
            datePart.Month,
            datePart.Day,
            Hour < 12 ? Hour + 12 : Hour,
            Minute,
            Second);

        return HourPeriod switch
        {
            HourPeriod.Am => earlyDateTime,
            HourPeriod.Pm => lateDateTime,
            HourPeriod.Undefined => earlyDateTime < minDate ? lateDateTime : earlyDateTime,
            _ => throw new InvalidOperationException()
        };
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <param name="provider">An <see cref="IFormatProvider"/> to use.</param>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString(IFormatProvider provider)
    {
        try
        {
            ThrowIfNotValid();

            StringBuilder stringBuilder = new();

            // Hour
            int adjustedHour;
            if (Settings.Default.Prefer24HourTime)
            {
                // This class stores its data in 12-hour format, so adjust it back to 24-hour time.
                if (Hour == 12 && HourPeriod == HourPeriod.Am)
                {
                    adjustedHour = 0;
                }
                else if (HourPeriod == HourPeriod.Pm)
                {
                    adjustedHour = Hour + 12;
                }
                else
                {
                    adjustedHour = Hour;
                }
            }
            else
            {
                adjustedHour = Hour;
            }

            stringBuilder.AppendFormat(
                Resources.ResourceManager.GetEffectiveProvider(provider),
                Resources.ResourceManager.GetString(nameof(Resources.NormalTimeTokenHourPartFormatString), provider),
                adjustedHour);

            // Minute
            if (Minute != 0 || Second != 0 || HourPeriod == HourPeriod.Undefined || Settings.Default.Prefer24HourTime)
            {
                stringBuilder.AppendFormat(
                    Resources.ResourceManager.GetEffectiveProvider(provider),
                    Resources.ResourceManager.GetString(nameof(Resources.NormalTimeTokenMinutePartFormatString), provider),
                    Minute);

                // Second
                if (Second != 0)
                {
                    stringBuilder.AppendFormat(
                        Resources.ResourceManager.GetEffectiveProvider(provider),
                        Resources.ResourceManager.GetString(nameof(Resources.NormalTimeTokenSecondPartFormatString), provider),
                        Second);
                }
            }

            // Hour period
            if (Settings.Default.Prefer24HourTime)
            {
                // No suffix when outputting 24-hour time.
            }
            else if (IsMidday)
            {
                stringBuilder.Append(Resources.ResourceManager.GetString(nameof(Resources.NormalTimeTokenMiddaySuffix), provider));
            }
            else if (IsMidnight)
            {
                stringBuilder.Append(Resources.ResourceManager.GetString(nameof(Resources.NormalTimeTokenMidnightSuffix), provider));
            }
            else if (HourPeriod == HourPeriod.Am)
            {
                stringBuilder.Append(Resources.ResourceManager.GetString(nameof(Resources.NormalTimeTokenAmSuffix), provider));
            }
            else if (HourPeriod == HourPeriod.Pm)
            {
                stringBuilder.Append(Resources.ResourceManager.GetString(nameof(Resources.NormalTimeTokenPmSuffix), provider));
            }

            return stringBuilder.ToString();
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            return GetType().ToString();
        }
    }

    /// <summary>
    /// Parses <see cref="NormalTimeToken"/> strings.
    /// </summary>
    public new sealed class Parser : TimeToken.Parser
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
        /// Returns a set of regular expressions supported by this parser.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>A set of regular expressions supported by this parser.</returns>
        public override IEnumerable<string> GetPatterns(IFormatProvider provider)
        {
            yield return Resources.ResourceManager.GetString(nameof(Resources.NormalTimeTokenMilitaryTimePattern), provider);
            yield return Resources.ResourceManager.GetString(nameof(Resources.NormalTimeTokenTimeWithSeparatorsPattern), provider);
            yield return Resources.ResourceManager.GetString(nameof(Resources.NormalTimeTokenTimeWithoutSeparatorsPattern), provider);
        }

        /// <summary>
        /// Parses a <see cref="Match"/> into a <see cref="TimeToken"/>.
        /// </summary>
        /// <param name="match">A <see cref="Match"/> representation of a <see cref="TimeToken"/>.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The <see cref="TimeToken"/> parsed from the <see cref="Match"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="match"/> or <paramref name="provider"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">If the <paramref name="match"/> is not a supported representation of a <see cref="TimeToken"/>.</exception>
        protected override TimeToken ParseInternal(Match match, IFormatProvider provider)
        {
            NormalTimeToken timeToken = new();

            provider = Resources.ResourceManager.GetEffectiveProvider(provider);

            // Parse hour
            if (match.Groups["hour"].Success)
            {
                timeToken.Hour = int.Parse(match.Groups["hour"].Value, provider);
            }

            // Parse minute
            if (match.Groups["minute"].Success)
            {
                timeToken.Minute = int.Parse(match.Groups["minute"].Value, provider);
            }

            // Parse second
            if (match.Groups["second"].Success)
            {
                timeToken.Second = int.Parse(match.Groups["second"].Value, provider);
            }

            // Parse hour period
            if (match.Groups["am"].Success)
            {
                timeToken.HourPeriod = HourPeriod.Am;
            }
            else if (match.Groups["pm"].Success)
            {
                timeToken.HourPeriod = HourPeriod.Pm;
            }
            else if (match.Groups["military"].Success || Settings.Default.Prefer24HourTime)
            {
                if (timeToken.Hour == 0)
                {
                    timeToken.Hour = 12;
                    timeToken.HourPeriod = HourPeriod.Am;
                }
                else if (timeToken.Hour < 12)
                {
                    timeToken.HourPeriod = HourPeriod.Am;
                }
                else if (timeToken.Hour == 12)
                {
                    timeToken.HourPeriod = HourPeriod.Pm;
                }
                else
                {
                    timeToken.Hour -= 12;
                    timeToken.HourPeriod = HourPeriod.Pm;
                }
            }
            else
            {
                if (timeToken.Hour == 0)
                {
                    timeToken.Hour = 12;
                    timeToken.HourPeriod = HourPeriod.Am;
                }
                else if (timeToken.Hour <= 12)
                {
                    timeToken.HourPeriod = HourPeriod.Undefined;
                }
                else
                {
                    timeToken.Hour -= 12;
                    timeToken.HourPeriod = HourPeriod.Pm;
                }
            }

            return timeToken;
        }
    }
}