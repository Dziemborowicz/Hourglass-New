// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpecialDateToken.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Parsing;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using Extensions;
using Properties;

/// <summary>
/// Represents a special date.
/// </summary>
public enum SpecialDate
{
    /// <summary>
    /// Represents the New Year (January 1).
    /// </summary>
    NewYear,

    /// <summary>
    /// Represents Christmas Day (December 25).
    /// </summary>
    ChristmasDay,

    /// <summary>
    /// Represents New Year's Eve (December 31).
    /// </summary>
    NewYearsEve
}

/// <summary>
/// Represents a special date.
/// </summary>
public sealed class SpecialDateToken : DateToken
{
    /// <summary>
    /// A list of supported special dates.
    /// </summary>
    private static readonly SpecialDateDefinition[] SpecialDates =
    [
        new(
            SpecialDate.NewYear,
            1 /* month */,
            1 /* day */),

        new(
            SpecialDate.ChristmasDay,
            12 /* month */,
            25 /* day */),

        new(
            SpecialDate.NewYearsEve,
            12 /* month */,
            31 /* day */)
    ];

    /// <summary>
    /// Gets or sets the <see cref="SpecialDate"/> represented by this token.
    /// </summary>
    public SpecialDate SpecialDate { get; set; }

    /// <summary>
    /// Gets a value indicating whether the token is valid.
    /// </summary>
    public override bool IsValid => GetSpecialDateDefinition() is not null;

    /// <summary>
    /// Returns the next date after <paramref name="minDate"/> that is represented by this token.
    /// </summary>
    /// <remarks>
    /// This method may return a date that is before <paramref name="minDate"/> if there is no date after <paramref
    /// name="minDate"/> that is represented by this token.
    /// </remarks>
    /// <param name="minDate">The minimum date to return. The time part is ignored.</param>
    /// <param name="inclusive">A value indicating whether the returned date should be on or after rather than
    /// strictly after <paramref name="minDate"/>.</param>
    /// <returns>The next date after <paramref name="minDate"/> that is represented by this token.</returns>
    public override DateTime ToDateTime(DateTime minDate, bool inclusive)
    {
        ThrowIfNotValid();

        SpecialDateDefinition specialDateDefinition = GetSpecialDateDefinition()!;

        DateTime date = new(
            minDate.Year,
            specialDateDefinition.Month,
            specialDateDefinition.Day);

        if (date < minDate.Date ||
            (date == minDate.Date && !inclusive))
        {
            date = date.AddYears(1);
        }

        return date;
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

            SpecialDateDefinition specialDateDefinition = GetSpecialDateDefinition()!;
            return specialDateDefinition.GetName(provider);
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            return GetType().ToString();
        }
    }

    /// <summary>
    /// Returns the <see cref="SpecialDateDefinition"/> object for a <see cref="Match"/>.
    /// </summary>
    /// <param name="match">A <see cref="Match"/>.</param>
    /// <returns>The <see cref="SpecialDateDefinition"/> object for a <see cref="Match"/>.</returns>
#pragma warning disable S3398
    private static SpecialDateDefinition? GetSpecialDateDefinitionForMatch(Match match)
#pragma warning restore S3398
    {
        return Array.Find(SpecialDates, e => match.Groups[e.MatchGroup].Success);
    }

    /// <summary>
    /// Returns the <see cref="SpecialDateDefinition"/> object for this part.
    /// </summary>
    /// <returns>The <see cref="SpecialDateDefinition"/> object for this part.</returns>
    private SpecialDateDefinition? GetSpecialDateDefinition()
    {
        return Array.Find(SpecialDates, e => e.SpecialDate == SpecialDate);
    }

    /// <summary>
    /// Parses <see cref="SpecialDateToken"/> strings.
    /// </summary>
    public new sealed class Parser : DateToken.Parser
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
            return SpecialDates.Select(e => e.GetPattern(provider));
        }

        /// <summary>
        /// Parses a <see cref="Match"/> into a <see cref="DateToken"/>.
        /// </summary>
        /// <param name="match">A <see cref="Match"/> representation of a <see cref="DateToken"/>.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The <see cref="DateToken"/> parsed from the <see cref="Match"/>.</returns>
        protected override DateToken ParseInternal(Match match, IFormatProvider provider)
        {
            SpecialDateDefinition? specialDateDefinition = GetSpecialDateDefinitionForMatch(match);

            return specialDateDefinition is not null
                ? new SpecialDateToken { SpecialDate = specialDateDefinition.SpecialDate }
                : throw new FormatException();
        }
    }

    /// <summary>
    /// Defines a special date.
    /// </summary>
    private sealed class SpecialDateDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecialDateDefinition"/> class.
        /// </summary>
        /// <param name="specialDate">The <see cref="SpecialDate"/>.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        public SpecialDateDefinition(SpecialDate specialDate, int month, int day)
        {
            SpecialDate = specialDate;

            Month = month;
            Day = day;

            MatchGroup = specialDate.ToString();
        }

        /// <summary>
        /// Gets the <see cref="SpecialDate"/>.
        /// </summary>
        public SpecialDate SpecialDate { get; }

        /// <summary>
        /// Gets the month.
        /// </summary>
        public int Month { get; }

        /// <summary>
        /// Gets the day.
        /// </summary>
        public int Day { get; }

        /// <summary>
        /// Gets the name of the regular expression match group that identifies the special date in a match.
        /// </summary>
        public string MatchGroup { get; }

        /// <summary>
        /// Returns the friendly name for the special date.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The friendly name for the special date.</returns>
        public string GetName(IFormatProvider provider)
        {
            string resourceName = string.Format(
                CultureInfo.InvariantCulture,
                "SpecialDateToken{0}Name",
                SpecialDate);

            return Resources.ResourceManager.GetString(resourceName, provider);
        }

        /// <summary>
        /// Returns the regular expression that matches the special date.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The regular expression that matches the special date.</returns>
        public string GetPattern(IFormatProvider provider)
        {
            string resourceName = string.Format(
                CultureInfo.InvariantCulture,
                "SpecialDateToken{0}Pattern",
                SpecialDate);

            string pattern = Resources.ResourceManager.GetString(resourceName, provider);
            return string.Format(CultureInfo.InvariantCulture, @"(?<{0}>{1})", SpecialDate, pattern);
        }
    }
}