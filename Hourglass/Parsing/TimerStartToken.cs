// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerStartToken.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Parsing;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using Extensions;
using Properties;
using Timing;

// ReSharper disable ExceptionNotDocumented

/// <summary>
/// Represents a <see cref="TimerStart"/>.
/// </summary>
[XmlInclude(typeof(DateTimeToken))]
[XmlInclude(typeof(TimeSpanToken))]
public abstract class TimerStartToken
{
    /// <summary>
    /// Gets a value indicating whether the token is valid.
    /// </summary>
    protected abstract bool IsValid { get; }

    /// <summary>
    /// The original timer input.
    /// </summary>
    public string OriginalInput { get; set; } = null!;

    /// <summary>
    /// Returns a <see cref="TimerStartToken"/> for the specified string, or <c>null</c> if the string is not a
    /// supported representation of a <see cref="TimerStartToken"/>.
    /// </summary>
    /// <param name="str">A string.</param>
    /// <returns>A <see cref="TimerStartToken"/> for the specified string, or <c>null</c> if the string is not a
    /// supported representation of a <see cref="TimerStartToken"/>.</returns>
    public static TimerStartToken? FromString(string str)
    {
        return FromString(str, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Returns a <see cref="TimerStartToken"/> for the specified string, or <c>null</c> if the string is not a
    /// supported representation of a <see cref="TimerStartToken"/>.
    /// </summary>
    /// <param name="str">A string.</param>
    /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
    /// <returns>A <see cref="TimerStartToken"/> for the specified string, or <c>null</c> if the string is not a
    /// supported representation of a <see cref="TimerStartToken"/>.</returns>
    private static TimerStartToken? FromString(string? str, IFormatProvider provider)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return null;
        }

        str = str!.Trim();

        string preferDateTimePattern = Resources.ResourceManager.GetString(nameof(Resources.TimerStartTokenUseDateTimeParserPattern), provider);
        if (Regex.IsMatch(str, preferDateTimePattern, Parser.RegexOptions))
        {
            str = Regex.Replace(str, preferDateTimePattern, string.Empty, Parser.RegexOptions);
            return FromDateTime(str);
        }

        return FromTimeSpanOrDateTimeString(str);
    }

    /// <summary>
    /// Returns the end time for a timer started with this token at a specified time.
    /// </summary>
    /// <param name="startTime">The time the timer is started.</param>
    /// <returns>The end time for a timer started with this token at the specified time.</returns>
    public abstract DateTime GetEndTime(DateTime startTime);

    /// <summary>
    /// Returns the end time for a timer started with this token at a specified time.
    /// </summary>
    /// <param name="startTime">The time the timer is started.</param>
    /// <param name="endTime">The end time for a timer started with this token at the specified time if the end
    /// time could be computed, or <see cref="DateTime.MinValue"/> otherwise.</param>
    /// <returns><c>true</c> if the end time could be computed, or <c>false</c> otherwise.</returns>
    public bool TryGetEndTime(DateTime startTime, out DateTime endTime)
    {
        try
        {
            endTime = GetEndTime(startTime);
            return true;
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            endTime = DateTime.MinValue;
            return false;
        }
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public sealed override string ToString()
    {
        return ToString(CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <param name="provider">An <see cref="IFormatProvider"/> to use.</param>
    /// <returns>A string that represents the current object.</returns>
    protected abstract string ToString(IFormatProvider provider);

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if <see cref="IsValid"/> is <c>false</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">If not valid.</exception>
    protected void ThrowIfNotValid()
    {
        if (!IsValid)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Returns a <see cref="TimerStartToken"/> that is a <see cref="DateTimeToken"/> for the specified string, or
    /// <c>null</c> if the string is not a supported representation of a <see cref="DateTimeToken"/>.
    /// </summary>
    /// <param name="str">A string.</param>
    /// <returns>A <see cref="TimerStartToken"/> for the specified string, or <c>null</c> if the string is not a
    /// supported representation of a <see cref="DateTimeToken"/>.</returns>
    private static TimerStartToken? FromDateTime(string str)
    {
        if (DateTimeToken.Parser.Instance.TryParse(str, out var timerStartToken))
        {
            return timerStartToken;
        }

        return null;
    }

    /// <summary>
    /// Returns a <see cref="TimerStartToken"/> for the specified string, or <c>null</c> if the string is not a
    /// supported representation of a <see cref="TimerStartToken"/> favoring a <see cref="TimeSpanToken"/> over a
    /// <see cref="DateTimeToken"/> in the case ambiguity.
    /// </summary>
    /// <param name="str">A string.</param>
    /// <returns>A <see cref="TimerStartToken"/> for the specified string, or <c>null</c> if the string is not a
    /// supported representation of a <see cref="TimerStartToken"/>.</returns>
    private static TimerStartToken? FromTimeSpanOrDateTimeString(string str)
    {
        if (TimeSpanToken.Parser.Instance.TryParse(str, out var timerStartToken))
        {
            return timerStartToken;
        }

        if (DateTimeToken.Parser.Instance.TryParse(str, out timerStartToken))
        {
            return timerStartToken;
        }

        return null;
    }

    /// <summary>
    /// Parses <see cref="TimerStartToken"/> strings.
    /// </summary>
    public abstract class Parser
    {
        /// <summary>
        /// The <see cref="RegexOptions"/> used when matching regular expressions.
        /// </summary>
        public static readonly RegexOptions RegexOptions =
            RegexOptions.CultureInvariant |
            RegexOptions.IgnoreCase |
            RegexOptions.IgnorePatternWhitespace;

        /// <summary>
        /// Parses a string into a <see cref="TimerStartToken"/>.
        /// </summary>
        /// <param name="str">A string representation of a <see cref="TimerStartToken"/>.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The <see cref="TimerStartToken"/> parsed from the string.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="str"/> or <paramref name="provider"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">If <paramref name="str"/> is not a supported representation of a <see cref="TimerStartToken"/>.</exception>
        public TimerStartToken Parse(string? str, IFormatProvider provider)
        {
            if (str is null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            TimerStartToken timerStartToken = ParseInternal(str, provider);

            if (!timerStartToken.IsValid)
            {
                throw new FormatException();
            }

            timerStartToken.OriginalInput = str;

            return timerStartToken;
        }

        /// <summary>
        /// Parses a string into a <see cref="TimerStartToken"/>.
        /// </summary>
        /// <remarks>
        /// This overload uses the <see cref="CultureInfo.CurrentCulture"/> as the <see cref="IFormatProvider"/>.
        /// </remarks>
        /// <param name="str">A string representation of a <see cref="TimerStartToken"/>.</param>
        /// <param name="timerStartToken">The <see cref="TimerStartToken"/> parsed from the string, or <c>null</c>
        /// if the string is not a supported representation of a <see cref="TimerStartToken"/>.</param>
        /// <returns><c>true</c> if the <see cref="TimerStartToken"/> was successfully parsed from <paramref
        /// name="str"/>, or <c>false</c> otherwise.</returns>
        public bool TryParse(string str, out TimerStartToken? timerStartToken)
        {
            return TryParse(str, CultureInfo.CurrentCulture, out timerStartToken);
        }

        /// <summary>
        /// Parses a string into a <see cref="TimerStartToken"/>.
        /// </summary>
        /// <param name="str">A string representation of a <see cref="TimerStartToken"/>.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <param name="timerStartToken">The <see cref="TimerStartToken"/> parsed from the string, or <c>null</c>
        /// if the string is not a supported representation of a <see cref="TimerStartToken"/>.</param>
        /// <returns><c>true</c> if the <see cref="TimerStartToken"/> was successfully parsed from <paramref
        /// name="str"/>, or <c>false</c> otherwise.</returns>
        public bool TryParse(string str, IFormatProvider provider, out TimerStartToken? timerStartToken)
        {
            try
            {
                timerStartToken = Parse(str, provider);
                return true;
            }
            catch (Exception ex) when (ex.CanBeHandled())
            {
                timerStartToken = null;
                return false;
            }
        }

        /// <summary>
        /// Parses a string into a <see cref="TimerStartToken"/>.
        /// </summary>
        /// <param name="str">A string representation of a <see cref="TimerStartToken"/>.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The <see cref="TimerStartToken"/> parsed from the string.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="str"/> or <paramref name="provider"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">If <paramref name="str"/> is not a supported representation of a <see cref="TimerStartToken"/>.</exception>
        protected abstract TimerStartToken ParseInternal(string str, IFormatProvider provider);
    }
}