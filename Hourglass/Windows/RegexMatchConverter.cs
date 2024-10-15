// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegexMatchConverter.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

// ReSharper disable ExceptionNotDocumented

/// <summary>
/// Matches the string representation of objects against a regular expression pattern.
/// </summary>
public sealed class RegexMatchConverter : IValueConverter
{
    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A value indicating whether the value produced by the binding source matches the regular expression passed as the converter parameter.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Regex.IsMatch(value!.ToString(), (string)parameter!);
    }

    /// <summary>
    /// Converts a value back. This method is not implemented.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>Nothing. This method is not implemented.</returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}