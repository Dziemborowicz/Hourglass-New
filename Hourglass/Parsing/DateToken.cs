// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateToken.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Parsing;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

/// <summary>
/// Represents the date part of an instant in time.
/// </summary>
[XmlInclude(typeof(DayOfWeekDateToken))]
[XmlInclude(typeof(EmptyDateToken))]
[XmlInclude(typeof(NormalDateToken))]
[XmlInclude(typeof(RelativeDateToken))]
[XmlInclude(typeof(SpecialDateToken))]
public abstract class DateToken
{
    /// <summary>
    /// Gets a list of all supported <see cref="Parser"/>s.
    /// </summary>
    public static IEnumerable<Parser> Parsers
    {
        get
        {
            yield return EmptyDateToken.Parser.Instance;
            yield return NormalDateToken.Parser.Instance;
            yield return DayOfWeekDateToken.Parser.Instance;
            yield return RelativeDateToken.Parser.Instance;
            yield return SpecialDateToken.Parser.Instance;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the token is valid.
    /// </summary>
    public abstract bool IsValid { get; }

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
    /// <exception cref="InvalidOperationException">If this token is not valid.</exception>
    public abstract DateTime ToDateTime(DateTime minDate, bool inclusive);

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
    public abstract string ToString(IFormatProvider provider);

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
    /// Parses <see cref="DateToken"/> strings.
    /// </summary>
    public abstract class Parser
    {
        /// <summary>
        /// Returns a value indicating whether this parser can be used in conjunction with a specified <see
        /// cref="TimeToken.Parser"/>.
        /// </summary>
        /// <param name="timeTokenParser">A <see cref="TimeToken.Parser"/>.</param>
        /// <returns>A value indicating whether this parser can be used in conjunction with the specified <see cref="TimeToken.Parser"/>.</returns>
        public virtual bool IsCompatibleWith(TimeToken.Parser timeTokenParser)
        {
            return true;
        }

        /// <summary>
        /// Returns a set of regular expressions supported by this parser.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>A set of regular expressions supported by this parser.</returns>
        public abstract IEnumerable<string> GetPatterns(IFormatProvider provider);

        /// <summary>
        /// Parses a <see cref="Match"/> into a <see cref="DateToken"/>.
        /// </summary>
        /// <param name="match">A <see cref="Match"/> representation of a <see cref="DateToken"/>.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The <see cref="DateToken"/> parsed from the <see cref="Match"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="match"/> or <paramref name="provider"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">If the <paramref name="match"/> is not a supported representation of a <see cref="DateToken"/>.</exception>
        public DateToken Parse(Match match, IFormatProvider provider)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            if (!match.Success)
            {
                throw new FormatException();
            }

            DateToken dateToken = ParseInternal(match, provider);

            if (!dateToken.IsValid)
            {
                throw new FormatException();
            }

            return dateToken;
        }

        /// <summary>
        /// Parses a <see cref="Match"/> into a <see cref="DateToken"/>.
        /// </summary>
        /// <param name="match">A <see cref="Match"/> representation of a <see cref="DateToken"/>.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The <see cref="DateToken"/> parsed from the <see cref="Match"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="match"/> or <paramref name="provider"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">If the <paramref name="match"/> is not a supported representation of a <see cref="DateToken"/>.</exception>
        protected abstract DateToken ParseInternal(Match match, IFormatProvider provider);
    }
}