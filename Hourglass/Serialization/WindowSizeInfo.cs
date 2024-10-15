// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowSizeInfo.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Serialization;

using System.Windows;

using Windows;

/// <summary>
/// The representation of a <see cref="WindowSize"/> used for XML serialization.
/// </summary>
public sealed class WindowSizeInfo
{
    /// <summary>
    /// Gets or sets the size and location of the window before being either minimized or maximized.
    /// </summary>
    public Rect RestoreBounds { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the window is restored, minimized, or maximized.
    /// </summary>
    public WindowState WindowState { get; set; }

    /// <summary>
    /// Gets or sets the window's <see cref="Window.WindowState"/> before the window was minimized.
    /// </summary>
    public WindowState RestoreWindowState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the window is in full-screen mode.
    /// </summary>
    public bool IsFullScreen { get; set; }

    /// <summary>
    /// Returns a <see cref="WindowSizeInfo"/> for the specified <see cref="WindowSize"/>, or <c>null</c> if the
    /// specified <see cref="WindowSize"/> is <c>null</c>.
    /// </summary>
    /// <param name="windowSize">A <see cref="WindowSize"/>.</param>
    /// <returns>A <see cref="WindowSizeInfo"/> for the specified <see cref="WindowSize"/>, or <c>null</c> if the specified <see cref="WindowSize"/> is <c>null</c>.</returns>
    public static WindowSizeInfo? FromWindowSize(WindowSize? windowSize)
    {
        return windowSize?.ToWindowSizeInfo();
    }
}