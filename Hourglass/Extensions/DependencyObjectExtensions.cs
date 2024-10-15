// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependencyObjectExtensions.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

/// <summary>
/// Provides extensions methods for the <see cref="DependencyObject"/> class.
/// </summary>
public static class DependencyObjectExtensions
{
    /// <summary>
    /// Returns the first visual child of a <see cref="DependencyObject"/> that matches the specified predicate.
    /// </summary>
    /// <param name="parent">A <see cref="DependencyObject"/>.</param>
    /// <param name="predicate">A predicate.</param>
    /// <returns>The first visual child of a <see cref="DependencyObject"/> that matches the specified predicate.</returns>
    public static DependencyObject? FindVisualChild(this DependencyObject parent, Func<DependencyObject, bool> predicate)
    {
        return GetAllVisualChildren(parent).FirstOrDefault(predicate!);
    }

    /// <summary>
    /// Returns all the visual children of a <see cref="DependencyObject"/>.
    /// </summary>
    /// <param name="parent">A <see cref="DependencyObject"/>.</param>
    /// <returns>All the visual children of a <see cref="DependencyObject"/>.</returns>
    public static IEnumerable<DependencyObject?> GetAllVisualChildren(this DependencyObject parent)
    {
        foreach (DependencyObject child in parent.GetVisualChildren())
        {
            yield return child;

            foreach (DependencyObject? childOfChild in GetAllVisualChildren(child))
            {
                yield return childOfChild;
            }
        }
    }

    /// <summary>
    /// Returns the immediate visual children of a <see cref="DependencyObject"/>.
    /// </summary>
    /// <param name="parent">A <see cref="DependencyObject"/>.</param>
    /// <returns>The immediate visual children of a <see cref="DependencyObject"/>.</returns>
    public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject parent)
    {
        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            yield return VisualTreeHelper.GetChild(parent, i);
        }
    }
}