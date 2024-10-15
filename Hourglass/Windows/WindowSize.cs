// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowSize.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Linq;
using System.Windows;

using Extensions;
using Serialization;

/// <summary>
/// The size, position, and state of a window.
/// </summary>
public sealed class WindowSize
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WindowSize"/> class.
    /// </summary>
    public WindowSize()
    {
        RestoreBounds = Rect.Empty;
        WindowState = WindowState.Normal;
        RestoreWindowState = WindowState.Normal;
        IsFullScreen = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowSize"/> class.
    /// </summary>
    /// <param name="restoreBounds">The size and location of the window before being either minimized or maximized.
    /// </param>
    /// <param name="windowState">A value that indicates whether the window is restored, minimized, or maximized.
    /// </param>
    /// <param name="restoreWindowState">The window's <see cref="Window.WindowState"/> before the window was
    /// minimized.</param>
    /// <param name="isFullScreen">A value indicating whether the window is in full-screen mode.</param>
    public WindowSize(Rect restoreBounds, WindowState windowState, WindowState restoreWindowState, bool isFullScreen)
    {
        RestoreBounds = restoreBounds;
        WindowState = windowState;
        RestoreWindowState = restoreWindowState;
        IsFullScreen = isFullScreen;
    }

    /// <summary>
    /// Gets the size and location of the window before being either minimized or maximized.
    /// </summary>
    public Rect RestoreBounds { get; }

    /// <summary>
    /// Gets a value that indicates whether the window is restored, minimized, or maximized.
    /// </summary>
    public WindowState WindowState { get; }

    /// <summary>
    /// Gets the window's <see cref="Window.WindowState"/> before the window was minimized.
    /// </summary>
    public WindowState RestoreWindowState { get; }

    /// <summary>
    /// Gets a value indicating whether the window is in full-screen mode.
    /// </summary>
    public bool IsFullScreen { get; }

    /// <summary>
    /// Returns a <see cref="WindowSize"/> for the specified <see cref="WindowSize"/>, or <c>null</c> if the
    /// specified <see cref="WindowSize"/> is <c>null</c>.
    /// </summary>
    /// <param name="windowSize">A <see cref="WindowSize"/>.</param>
    /// <returns>A <see cref="WindowSize"/> for the specified <see cref="WindowSize"/>, or <c>null</c> if the specified <see cref="WindowSize"/> is <c>null</c>.</returns>
    public static WindowSize? FromWindowSize(WindowSize? windowSize)
    {
        return windowSize is null
            ? null
            : new(
                windowSize.RestoreBounds,
                windowSize.WindowState,
                windowSize.RestoreWindowState,
                windowSize.IsFullScreen);
    }

    /// <summary>
    /// Returns a <see cref="WindowSize"/> for the specified <see cref="WindowSizeInfo"/>, or <c>null</c> if the
    /// specified <see cref="WindowSizeInfo"/> is <c>null</c>.
    /// </summary>
    /// <param name="info">A <see cref="WindowSizeInfo"/>.</param>
    /// <returns>A <see cref="WindowSize"/> for the specified <see cref="WindowSizeInfo"/>, or <c>null</c> if the specified <see cref="WindowSizeInfo"/> is <c>null</c>.</returns>
    public static WindowSize? FromWindowSizeInfo(WindowSizeInfo? info)
    {
        return info is null
            ? null
            : new(
                info.RestoreBounds,
                info.WindowState,
                info.RestoreWindowState,
                info.IsFullScreen);
    }

    /// <summary>
    /// Returns a <see cref="WindowSize"/> for the specified window, or <c>null</c> if the specified window is
    /// <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The type of the window.</typeparam>
    /// <param name="window">A window.</param>
    /// <returns>A <see cref="WindowSize"/> for the specified window, or <c>null</c> if the specified window is <c>null</c>.</returns>
    public static WindowSize? FromWindow<T>(T? window)
        where T : Window, IRestorableWindow
    {
        return window is null
            ? null
            : new(
                window.WindowState == WindowState.Normal
                    ? new(window.Left, window.Top, window.Width, window.Height)
                    : window.RestoreBounds,
                window.WindowState,
                window.RestoreWindowState,
                window.IsFullScreen);
    }

    /// <summary>
    /// Returns a <see cref="WindowSize"/> for a visible window of type <typeparamref name="T"/>, or <c>null</c> if
    /// there is no visible window of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the window.</typeparam>
    /// <returns>A <see cref="WindowSize"/> for a visible window of type <typeparamref name="T"/>, or <c>null</c> if there is no visible window of type <typeparamref name="T"/>.</returns>
    public static WindowSize? FromWindowOfType<T>()
        where T : Window, IRestorableWindow
    {
        if (Application.Current is null)
        {
            return null;
        }

        T? window = Application.Current.Windows
            .OfType<T>()
            .LastOrDefault(static w => w.IsVisible);

        return FromWindow(window);
    }

    /// <summary>
    /// Returns a <see cref="WindowSize"/> for another visible or last window of the same type, or <c>null</c> if there is
    /// no other visible or last window of the same type.
    /// </summary>
    /// <typeparam name="T">The type of the window.</typeparam>
    /// <param name="window">A window.</param>
    /// <returns>A <see cref="WindowSize"/> for another visible or last window of the same type, or <c>null</c> if there is no other visible or last window of the same type.</returns>
    public static WindowSize? FromSiblingOfWindow<T>(T window)
        where T : Window, IRestorableWindow
    {
        if (Application.Current is null)
        {
            return null;
        }

        T[] otherWindows = Application.Current.Windows
            .OfType<T>()
            .Where(w => !ReferenceEquals(w, window))
            .ToArray();

        return FromWindow(Array.FindLast(otherWindows, static w => w.IsVisible) ?? otherWindows.LastOrDefault());
    }

    /// <summary>
    /// Returns the representation of the <see cref="WindowSizeInfo"/> used for XML serialization.
    /// </summary>
    /// <returns>The representation of the <see cref="WindowSizeInfo"/> used for XML serialization.</returns>
    public WindowSizeInfo ToWindowSizeInfo()
    {
        return new()
        {
            RestoreBounds = RestoreBounds,
            WindowState = WindowState,
            RestoreWindowState = RestoreWindowState,
            IsFullScreen = IsFullScreen
        };
    }
}