// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeExtensions.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Extensions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Properties;

/// <summary>
/// Provides extensions methods for the <see cref="DateTime"/> class.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Returns a new <see cref="DateTime"/> that adds the specified number of weeks to the value of this instance.
    /// </summary>
    /// <param name="dateTime">A <see cref="DateTime"/>.</param>
    /// <param name="weeks">A number of weeks. The <paramref name="weeks"/> parameter can be negative or positive.
    /// </param>
    /// <returns>An object whose value is the sum of the date and time represented by this instance and <paramref
    /// name="weeks"/>.</returns>
    public static DateTime AddWeeks(this DateTime dateTime, double weeks)
    {
        return dateTime.AddDays(7 * weeks);
    }

    /// <summary>
    /// Returns a new <see cref="DateTime"/> that adds the specified number of months to the value of this instance.
    /// </summary>
    /// <param name="dateTime">A <see cref="DateTime"/>.</param>
    /// <param name="months">A number of months. The <paramref name="months"/> parameter can be negative or
    /// positive.</param>
    /// <returns>An object whose value is the sum of the date and time represented by this instance and <paramref
    /// name="months"/>.</returns>
    public static DateTime AddMonths(this DateTime dateTime, double months)
    {
        int wholeMonths = (int)months;
        double partMonth = months % 1.0;

        // Add whole months
        dateTime = dateTime.AddMonths(wholeMonths);

        // Add part month if required
        if (partMonth > 0.0)
        {
            int monthInDays = (dateTime.AddMonths(1) - dateTime).Days;
            dateTime = dateTime.AddDays(Math.Round(monthInDays * partMonth));
        }
        else if (partMonth < 0.0)
        {
            int monthInDays = (dateTime - dateTime.AddMonths(-1)).Days;
            dateTime = dateTime.AddDays(Math.Round(monthInDays * partMonth));
        }

        return dateTime;
    }

    /// <summary>
    /// Returns a new <see cref="DateTime"/> that adds the specified number of years to the value of this instance.
    /// </summary>
    /// <param name="dateTime">A <see cref="DateTime"/>.</param>
    /// <param name="years">A number of years. The <paramref name="years"/> parameter can be negative or positive.
    /// </param>
    /// <returns>An object whose value is the sum of the date and time represented by this instance and <paramref
    /// name="years"/>.</returns>
    public static DateTime AddYears(this DateTime dateTime, double years)
    {
        return dateTime.AddMonths(12 * years);
    }

    /// <summary>
    /// Converts a month number to a string representation of the month.
    /// </summary>
    /// <param name="month">A month number between 1 and 12 inclusive.</param>
    /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
    /// <returns>The string representation of the month.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the month is out of range.</exception>
    public static string GetMonthString(int month, IFormatProvider provider)
    {
        IDictionary<int, string> months = GetMonthStrings(provider);

        if (!months.TryGetValue(month, out var monthString))
        {
            throw new ArgumentOutOfRangeException(nameof(month));
        }

        return monthString;
    }

    /// <summary>
    /// Converts a day number to an ordinal string representation of the day.
    /// </summary>
    /// <param name="day">A day number between 1 and 31 inclusive.</param>
    /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
    /// <returns>The ordinal string representation of the day.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the day is out of range.</exception>
    public static string GetOrdinalDayString(int day, IFormatProvider provider)
    {
        if (day is < 1 or > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(day));
        }

        if (day % 10 == 1 && day / 10 != 1)
        {
            return string.Format(
                Resources.ResourceManager.GetEffectiveProvider(provider),
                Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsNstFormatString), provider),
                day);
        }

        if (day % 10 == 2 && day / 10 != 1)
        {
            return string.Format(
                Resources.ResourceManager.GetEffectiveProvider(provider),
                Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsNndFormatString), provider),
                day);
        }

        if (day % 10 == 3 && day / 10 != 1)
        {
            return string.Format(
                Resources.ResourceManager.GetEffectiveProvider(provider),
                Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsNrdFormatString), provider),
                day);
        }

        return string.Format(
            Resources.ResourceManager.GetEffectiveProvider(provider),
            Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsNthFormatString), provider),
            day);
    }

    /// <summary>
    /// Increments a month by one.
    /// </summary>
    /// <param name="year">A year.</param>
    /// <param name="month">A month number between 1 and 12 inclusive.</param>
    /// <exception cref="ArgumentOutOfRangeException">If the month is out of range.</exception>
    public static void IncrementMonth(ref int year, ref int month)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month));
        }

        if (month < 12)
        {
            month++;
        }
        else
        {
            month = 1;
            year++;
        }
    }

    /// <summary>
    /// Returns a value indicating whether a year, month, and day form a valid representation of a date.
    /// </summary>
    /// <param name="year">A year.</param>
    /// <param name="month">A month.</param>
    /// <param name="day">A day.</param>
    /// <returns>A value indicating whether a year, month, and day form a valid representation of a date.</returns>
    public static bool IsValid(int? year, int? month, int? day)
    {
        try
        {
            DateTime date = new(
                year ?? 2000,
                month ?? 1,
                day ?? 1);

            return date != DateTime.MinValue && date != DateTime.MaxValue;
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            return false;
        }
    }

    /// <summary>
    /// Parses a string into a month number between 1 and 12 inclusive.
    /// </summary>
    /// <param name="str">A string representation of a month.</param>
    /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
    /// <returns>The month number parsed from the string.</returns>
    /// <exception cref="FormatException">If <paramref name="str"/> is not a supported representation of a month. </exception>
    public static int ParseMonth(string str, IFormatProvider provider)
    {
        KeyValuePair<int, string>[] matches = GetMonthStrings(provider)
            .Where(e => e.Value.StartsWith(str, true /* ignoreCase */, (CultureInfo)provider))
            .ToArray();

        if (matches.Length != 1)
        {
            throw new FormatException();
        }

        return matches[0].Key;
    }

    /// <summary>
    /// Tries to create a new <see cref="DateTime"/> from a year, month, and day.
    /// </summary>
    /// <param name="year">A year.</param>
    /// <param name="month">A month.</param>
    /// <param name="day">A day.</param>
    /// <param name="dateTime">The <see cref="DateTime"/>.</param>
    /// <returns><c>true</c> if a new <see cref="DateTime"/> is successfully created from the year, month, and day,
    /// or <c>false</c> otherwise.</returns>
    public static bool TryToDateTime(int year, int month, int day, out DateTime dateTime)
    {
        try
        {
            dateTime = new(year, month, day);
            return true;
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            dateTime = DateTime.MinValue;
            return false;
        }
    }

    /// <summary>
    /// Returns a dictionary mapping month values to their localized string representations.
    /// </summary>
    /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
    /// <returns>A dictionary mapping month values to their localized string representations.</returns>
    private static IDictionary<int, string> GetMonthStrings(IFormatProvider provider)
    {
        return new Dictionary<int, string>
        {
            { 1, Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsJanuary), provider) },
            { 2, Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsFebruary), provider) },
            { 3, Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsMarch), provider) },
            { 4, Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsApril), provider) },
            { 5, Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsMay), provider) },
            { 6, Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsJune), provider) },
            { 7, Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsJuly), provider) },
            { 8, Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsAugust), provider) },
            { 9, Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsSeptember), provider) },
            { 10, Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsOctober), provider) },
            { 11, Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsNovember), provider) },
            { 12, Resources.ResourceManager.GetString(nameof(Resources.DateTimeExtensionsDecember), provider) }
        };
    }
}