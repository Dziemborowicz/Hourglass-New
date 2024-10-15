// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeToken.cs" company="Chris Dziemborowicz">
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
/// Represents the time part of an instant in time.
/// </summary>
[XmlInclude(typeof(EmptyTimeToken))]
[XmlInclude(typeof(NormalTimeToken))]
[XmlInclude(typeof(SpecialTimeToken))]
public abstract class TimeToken
{
    /// <summary>
    /// Gets a list of all supported <see cref="Parser"/>s.
    /// </summary>
    public static IEnumerable<Parser> Parsers
    {
        get
        {
            yield return EmptyTimeToken.Parser.Instance;
            yield return NormalTimeToken.Parser.Instance;
            yield return SpecialTimeToken.Parser.Instance;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the token is valid.
    /// </summary>
    public abstract bool IsValid { get; }

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
    public abstract DateTime ToDateTime(DateTime minDate, DateTime datePart);

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
    /// Parses <see cref="TimeToken"/> strings.
    /// </summary>
    public abstract class Parser
    {
        /// <summary>
        /// Returns a value indicating whether this parser can be used in conjunction with a specified <see
        /// cref="DateToken.Parser"/>.
        /// </summary>
        /// <param name="dateTokenParser">A <see cref="DateToken.Parser"/>.</param>
        /// <returns>A value indicating whether this parser can be used in conjunction with the specified <see
        /// cref="DateToken.Parser"/>.</returns>
        public virtual bool IsCompatibleWith(DateToken.Parser dateTokenParser)
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
        /// Parses a <see cref="Match"/> into a <see cref="TimeToken"/>.
        /// </summary>
        /// <param name="match">A <see cref="Match"/> representation of a <see cref="TimeToken"/>.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The <see cref="TimeToken"/> parsed from the <see cref="Match"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="match"/> or <paramref name="provider"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">If the <paramref name="match"/> is not a supported representation of a <see cref="TimeToken"/>.</exception>
        public TimeToken Parse(Match match, IFormatProvider provider)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            if (!match.Success)
            {
                throw new FormatException();
            }

            TimeToken timeToken = ParseInternal(match, provider);

            if (!timeToken.IsValid)
            {
                throw new FormatException();
            }

            return timeToken;
        }

        /// <summary>
        /// Parses a <see cref="Match"/> into a <see cref="TimeToken"/>.
        /// </summary>
        /// <param name="match">A <see cref="Match"/> representation of a <see cref="TimeToken"/>.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The <see cref="TimeToken"/> parsed from the <see cref="Match"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="match"/> or <paramref name="provider"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">If the <paramref name="match"/> is not a supported representation of a <see cref="TimeToken"/>.</exception>
        protected abstract TimeToken ParseInternal(Match match, IFormatProvider provider);
    }
}