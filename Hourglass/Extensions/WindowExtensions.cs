// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowExtensions.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Extensions;

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

using KPreisser.UI;

using Managers;
using Windows;

/// <summary>
/// Specifies options for restoring a <see cref="WindowSize"/> to a <see cref="Window"/>.
/// </summary>
[Flags]
public enum RestoreOptions
{
    /// <summary>
    /// Specifies that no options are set.
    /// </summary>
    None,

    /// <summary>
    /// Allows restoring a window to a minimized state.
    /// </summary>
    AllowMinimized
}

/// <summary>
/// Provides extensions methods for the <see cref="Window"/> class, and helper methods for manipulating the size,
/// position, and state of windows through the <see cref="WindowSize"/> class.
/// </summary>
public static class WindowExtensions
{
    #region Public Methods

    /// <summary>
    /// Sets the Desktop Window Manager (DWM) non-client rendering attributes on the window to use (or not use)
    /// immersive dark mode. This method has no effect on versions of windows that do not support it.
    /// </summary>
    /// <param name="window">A <see cref="TimerWindow"/>.</param>
    /// <param name="useImmersiveDarkMode">A value indicating whether to use immersive dark mode.</param>
    public static void SetImmersiveDarkMode(this TimerWindow window, bool useImmersiveDarkMode)
    {
        if (!EnvironmentExtensions.IsWindows10BuildOrNewer(17763))
        {
            return;
        }

        IntPtr handle = new WindowInteropHelper(window).Handle;

        if (handle == IntPtr.Zero)
        {
            return;
        }

        DwmWindowAttribute attribute = EnvironmentExtensions.IsWindows10BuildOrNewer(18985)
            ? DwmWindowAttribute.UseImmersiveDarkMode
            : DwmWindowAttribute.UseImmersiveDarkModeBefore20H1;

        if (NativeMethods.DwmGetWindowAttribute(handle, attribute, out bool prevUseImmersiveDarkMode, sizeof(int)) != 0)
        {
            return;
        }

        if (prevUseImmersiveDarkMode == useImmersiveDarkMode)
        {
            return;
        }

        if (NativeMethods.DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) != 0)
        {
            return;
        }

        // This hack appears to be the only way to get the non-client area of the window to redraw correctly.
        if (window is { IsVisible: true, IsFullScreen: false })
        {
            if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
                window.WindowState = WindowState.Maximized;
            }
            else
            {
                double width = window.Width;
                window.Width += 1;
                window.Width = width;
            }
        }
    }

    /// <summary>
    /// Returns a new <see cref="Rect"/> with the merged properties of <paramref name="rect"/> and <paramref
    /// name="otherRect"/>. Each property in the returned <see cref="Rect"/> is set from <paramref name="otherRect"/>,
    /// except where that property is <see cref="double.PositiveInfinity"/>, <see cref="double.NegativeInfinity"/>,
    /// or <see cref="double.NaN"/>, in which case the property is set from <paramref name="rect"/>.
    /// </summary>
    /// <param name="rect">A <see cref="Rect"/>.</param>
    /// <param name="otherRect">Another <see cref="Rect"/>.</param>
    /// <returns>A new <see cref="Rect"/> with the merged properties of <paramref name="rect"/> and <paramref
    /// name="otherRect"/>.</returns>
    public static Rect Merge(this Rect rect, Rect otherRect)
    {
        return new(
            IsValidDouble(otherRect.X) ? otherRect.X : rect.X,
            IsValidDouble(otherRect.Y) ? otherRect.Y : rect.Y,
            IsValidDouble(otherRect.Width) ? otherRect.Width : rect.Width,
            IsValidDouble(otherRect.Height) ? otherRect.Height : rect.Height);
    }

    /// <summary>
    /// Offsets a <see cref="WindowSize"/> slightly from its current position.
    /// </summary>
    /// <param name="windowSize">A <see cref="WindowSize"/>.</param>
    /// <returns>The offset <see cref="WindowSize"/>.</returns>
    public static WindowSize? Offset(this WindowSize? windowSize)
    {
        return windowSize is null
            ? null
            : new(
                windowSize.RestoreBounds.Offset(),
                windowSize.WindowState,
                windowSize.RestoreWindowState,
                windowSize.IsFullScreen);
    }

    /// <summary>
    /// Restores the size, position, and state of a window from its persisted size, position, and state.
    /// </summary>
    /// <typeparam name="T">The type of the window.</typeparam>
    /// <param name="window">A window.</param>
    /// <param name="options">Options for performing the restore. (Optional.)</param>
    public static void RestoreFromSettings<T>(this T window, RestoreOptions options = RestoreOptions.None)
        where T : Window, IRestorableWindow
    {
        WindowSize windowSize = window.PersistedSize;
        window.Restore(windowSize, options);
    }

    /// <summary>
    /// Restores the size, position, and state of a window from another window of the same type.
    /// </summary>
    /// <remarks>
    /// This method offsets the window position slightly so that the two windows do not overlap.
    /// </remarks>
    /// <typeparam name="T">The type of the window.</typeparam>
    /// <param name="window">A window.</param>
    /// <param name="otherWindow">The window from which to copy the size, position, and state.</param>
    /// <param name="options">Options for performing the restore. (Optional.)</param>
    public static void RestoreFromWindow<T>(this T window, T otherWindow, RestoreOptions options = RestoreOptions.None)
        where T : Window, IRestorableWindow
    {
        WindowSize? windowSize = WindowSize.FromWindow(otherWindow);
        WindowSize? offsetWindowSize = windowSize.Offset();
        window.Restore(offsetWindowSize, options);
    }

    /// <summary>
    /// Restores the size, position, and state of a window from another visible window of the same type, or from
    /// the app settings if there is no other visible window of the same type.
    /// </summary>
    /// <remarks>
    /// This method offsets the window position slightly so that the two windows do not overlap.
    /// </remarks>
    /// <typeparam name="T">The type of the window.</typeparam>
    /// <param name="window">A window.</param>
    /// <param name="options">Options for performing the restore. (Optional.)</param>
    public static void RestoreFromSibling<T>(this T window, RestoreOptions options = RestoreOptions.None)
        where T : Window, IRestorableWindow
    {
        WindowSize? windowSize = WindowSize.FromSiblingOfWindow(window);
        if (windowSize is not null)
        {
            WindowSize? offsetWindowSize = windowSize.Offset();
            window.Restore(offsetWindowSize, options);
        }
        else
        {
            window.RestoreFromSettings(options);
        }
    }

    /// <summary>
    /// Restores the size, position, and state of a window.
    /// </summary>
    /// <typeparam name="T">The type of the window.</typeparam>
    /// <param name="window">A window.</param>
    /// <param name="windowSize">The size, position, and state to restore.</param>
    /// <param name="options">Options for performing the restore. (Optional.)</param>
    public static void Restore<T>(this T? window, WindowSize? windowSize, RestoreOptions options = RestoreOptions.None)
        where T : Window, IRestorableWindow
    {
        if (window is null || windowSize is null)
        {
            return;
        }

        // Restore size and position
        window.RestoreBounds(windowSize.RestoreBounds);

        // Restore state
        if (windowSize.WindowState == WindowState.Minimized && !options.HasFlag(RestoreOptions.AllowMinimized))
        {
            window.RestoreState(
                windowSize.RestoreWindowState,
                windowSize.RestoreWindowState,
                windowSize.IsFullScreen);
        }
        else
        {
            window.RestoreState(
                windowSize.WindowState,
                windowSize.RestoreWindowState,
                windowSize.IsFullScreen);
        }

        // If the window is restored to a size or position that is not on the screen, center the window
        if (windowSize.RestoreBounds.IsEmpty || !window.IsOnScreen())
        {
            window.CenterOnScreen();
        }
    }

    #endregion

    #region Private Methods (Restore)

    /// <summary>
    /// Restores the size and position of a window.
    /// </summary>
    /// <param name="window">A window.</param>
    /// <param name="restoreBounds">The size and location of the window before being either minimized or maximized.
    /// </param>
    private static void RestoreBounds(this Window window, Rect restoreBounds)
    {
        if (restoreBounds.HasLocation())
        {
            window.Left = restoreBounds.Left;
            window.Top = restoreBounds.Top;
        }

        if (restoreBounds.HasSize())
        {
            window.Width = restoreBounds.Width;
            window.Height = restoreBounds.Height;
        }
    }

    /// <summary>
    /// Restores the state of a window.
    /// </summary>
    /// <typeparam name="T">The type of the window.</typeparam>
    /// <param name="window">A window.</param>
    /// <param name="windowState">A value that indicates whether the window is restored, minimized, or maximized.
    /// </param>
    /// <param name="restoreWindowState">The window's <see cref="Window.WindowState"/> before the window was
    /// minimized.</param>
    /// <param name="isFullScreen">A value indicating whether the window is in full-screen mode.</param>
    /// <seealso cref="RestoreStateToNotVisibleWindow{T}"/>
    private static void RestoreState<T>(this T window, WindowState windowState, WindowState restoreWindowState, bool isFullScreen)
        where T : Window, IRestorableWindow
    {
        if (window.IsVisible)
        {
            RestoreStateToVisibleWindow(
                window,
                windowState,
                restoreWindowState,
                isFullScreen);
        }
        else
        {
            RestoreStateToNotVisibleWindow(
                window,
                windowState,
                restoreWindowState,
                isFullScreen);
        }
    }

    /// <summary>
    /// Restores the state of a window that is visible.
    /// </summary>
    /// <remarks>
    /// Setting the state to maximized or full-screen before the has loaded will maximize or full-screen the window
    /// on the primary display rather than the display where the window was originally maximized or full-screened,
    /// so we need to handle those cases differently.
    /// </remarks>
    /// <typeparam name="T">The type of the window.</typeparam>
    /// <param name="window">A window.</param>
    /// <param name="windowState">A value that indicates whether the window is restored, minimized, or maximized.
    /// </param>
    /// <param name="restoreWindowState">The window's <see cref="Window.WindowState"/> before the window was
    /// minimized.</param>
    /// <param name="isFullScreen">A value indicating whether the window is in full-screen mode.</param>
    /// <seealso cref="RestoreStateToNotVisibleWindow{T}"/>
    private static void RestoreStateToVisibleWindow<T>(T window, WindowState windowState, WindowState restoreWindowState, bool isFullScreen)
        where T : Window, IRestorableWindow
    {
        if (isFullScreen)
        {
            window.IsFullScreen = true;
            window.RestoreWindowState = restoreWindowState;
        }
        else
        {
            window.IsFullScreen = false;
            window.WindowState = windowState;
            window.RestoreWindowState = restoreWindowState;
        }
    }

    /// <summary>
    /// Restores the state of a window that is not visible.
    /// </summary>
    /// <remarks>
    /// Setting the state to maximized or full-screen before the has loaded will maximize or full-screen the window
    /// on the primary display rather than the display where the window was originally maximized or full-screened,
    /// so we need to handle those cases differently.
    /// </remarks>
    /// <typeparam name="T">The type of the window.</typeparam>
    /// <param name="window">A window.</param>
    /// <param name="windowState">A value that indicates whether the window is restored, minimized, or maximized.
    /// </param>
    /// <param name="restoreWindowState">The window's <see cref="Window.WindowState"/> before the window was
    /// minimized.</param>
    /// <param name="isFullScreen">A value indicating whether the window is in full-screen mode.</param>
    /// <seealso cref="RestoreStateToVisibleWindow{T}"/>
    private static void RestoreStateToNotVisibleWindow<T>(T window, WindowState windowState, WindowState restoreWindowState, bool isFullScreen)
        where T : Window, IRestorableWindow
    {
        // Remove old handlers (if any)
        window.Loaded -= FullScreenSenderWindow;
        window.Loaded -= MaximizeSenderWindow;

        if (isFullScreen)
        {
            // Handle this on window load
            window.Loaded += FullScreenSenderWindow;
            window.RestoreWindowState = restoreWindowState;
        }
        else if (windowState == WindowState.Maximized)
        {
            // Handle this on window load
            window.Loaded += MaximizeSenderWindow;
            window.RestoreWindowState = restoreWindowState;
        }
        else
        {
            // Handle this now
            window.IsFullScreen = false;
            window.WindowState = windowState;
            window.RestoreWindowState = restoreWindowState;
        }
    }

    /// <summary>
    /// Invoked when a window is laid out, rendered, and ready for interaction in order to restore it to a
    /// full-screen state.
    /// </summary>
    /// <param name="sender">A window.</param>
    /// <param name="e">The event data.</param>
    private static void FullScreenSenderWindow(object sender, RoutedEventArgs e)
    {
        Window window = (Window)sender;
        IRestorableWindow restorableWindow = (IRestorableWindow)sender;

        restorableWindow.IsFullScreen = true;
        window.Loaded -= FullScreenSenderWindow;
    }

    /// <summary>
    /// Invoked when a window is laid out, rendered, and ready for interaction in order to restore it to a
    /// maximized state.
    /// </summary>
    /// <param name="sender">A window.</param>
    /// <param name="e">The event data.</param>
    private static void MaximizeSenderWindow(object sender, RoutedEventArgs e)
    {
        Window window = (Window)sender;
        IRestorableWindow restorableWindow = (IRestorableWindow)sender;

        restorableWindow.IsFullScreen = false;
        window.WindowState = WindowState.Maximized;
        window.Loaded -= MaximizeSenderWindow;
    }

    #endregion

    #region Private Methods (Size Manipulation)

    /// <summary>
    /// Positions a <see cref="Rect"/> in the center of the screen. If the <see cref="Rect"/> is larger than the
    /// work area, the <see cref="Rect"/> is decreased to fit in the work area.
    /// </summary>
    /// <param name="rect">A <see cref="Rect"/>.</param>
    /// <returns>The centered <see cref="Rect"/>.</returns>
    private static Rect CenterOnScreen(this Rect rect)
    {
        Rect offsetRect = rect;

        if (offsetRect.HasSize())
        {
            offsetRect.Width = Math.Min(rect.Width, SystemParameters.WorkArea.Width);
            offsetRect.Height = Math.Min(rect.Height, SystemParameters.WorkArea.Height);
            offsetRect.X = (SystemParameters.WorkArea.Width - offsetRect.Width) / 2 + SystemParameters.WorkArea.Left;
            offsetRect.Y = (SystemParameters.WorkArea.Height - offsetRect.Height) / 2 + SystemParameters.WorkArea.Top;
        }

        return offsetRect;
    }

    /// <summary>
    /// Positions a window in the center of the screen. If the window is larger than the work area, the window size
    /// is decreased to fit in the work area.
    /// </summary>
    /// <param name="window">A window.</param>
    public static void CenterOnScreen(this Window window)
    {
        Rect windowRect = window.GetBoundsForNormalState();
        Rect centeredRect = windowRect.CenterOnScreen();

        window.RestoreBounds(centeredRect);
    }

    /// <summary>
    /// Returns the bounds of a window, or its restore bounds if it is minimized or maximized.
    /// </summary>
    /// <param name="window">A window.</param>
    /// <returns>The bounds of a window, or its restore bounds if it is minimized or maximized.</returns>
    private static Rect GetBoundsForNormalState(this Window window)
    {
        if (window.WindowState != WindowState.Normal && window.RestoreBounds.HasSizeAndLocation())
        {
            return window.RestoreBounds;
        }

        return new(window.Left, window.Top, window.Width, window.Height);
    }

    /// <summary>
    /// Returns the <see cref="Point"/> at the center of a <see cref="Rect"/>.
    /// </summary>
    /// <param name="rect">A <see cref="Rect"/>.</param>
    /// <returns>The <see cref="Point"/> at the center of a <see cref="Rect"/>.</returns>
    private static Point GetCenter(this Rect rect)
    {
        if (rect.HasSizeAndLocation())
        {
            return new(
                (int)(rect.X + rect.Width / 2),
                (int)(rect.Y + rect.Height / 2));
        }

        return rect.Location;
    }

    /// <summary>
    /// Returns a value indicating whether the <see cref="Rect"/> has a valid location.
    /// </summary>
    /// <param name="rect">A <see cref="Rect"/>.</param>
    /// <returns>A value indicating whether the <see cref="Rect"/> has a valid location.</returns>
    private static bool HasLocation(this Rect rect)
    {
        return IsValidDouble(rect.X) && IsValidDouble(rect.Y);
    }

    /// <summary>
    /// Returns a value indicating whether the <see cref="Rect"/> has a valid size.
    /// </summary>
    /// <param name="rect">A <see cref="Rect"/>.</param>
    /// <returns>A value indicating whether the <see cref="Rect"/> has a valid size.</returns>
    private static bool HasSize(this Rect rect)
    {
        return IsValidDouble(rect.Width) && IsValidDouble(rect.Height);
    }

    /// <summary>
    /// Returns a value indicating whether the <see cref="Rect"/> has a valid size and location.
    /// </summary>
    /// <param name="rect">A <see cref="Rect"/>.</param>
    /// <returns>A value indicating whether the <see cref="Rect"/> has a valid size and location.</returns>
    private static bool HasSizeAndLocation(this Rect rect)
    {
        return rect.HasSize() && rect.HasLocation();
    }

    /// <summary>
    /// Returns a value indicating whether the size and position of the <see cref="Rect"/> are such that the center
    /// of the <see cref="Rect"/> is visible on the screen.
    /// </summary>
    /// <param name="rect">A <see cref="Rect"/>.</param>
    /// <returns>A value indicating whether the size and position of the <see cref="Rect"/> are such that the center
    /// of the <see cref="Rect"/> is visible on the screen.</returns>
    private static bool IsOnScreen(this Rect rect)
    {
        if (rect.HasLocation())
        {
            Rect virtualScreenRect = new(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight);

            return virtualScreenRect.Contains(rect.GetCenter());
        }

        return true;
    }

    /// <summary>
    /// Returns a value indicating whether the size and position of the window are such that the center of the
    /// window is visible on the screen when in its normal state.
    /// </summary>
    /// <param name="window">A window.</param>
    /// <returns>A value indicating whether the size and position of the window are such that the center of the window is visible on the screen when in its normal state.</returns>
    private static bool IsOnScreen(this Window window)
    {
        Rect windowRect = window.GetBoundsForNormalState();
        return windowRect.IsOnScreen();
    }

    /// <summary>
    /// Returns a value indicating whether a <see cref="double"/> is a value other than <see
    /// cref="double.PositiveInfinity"/>, <see cref="double.NegativeInfinity"/>, and <see cref="double.NaN"/>.
    /// </summary>
    /// <param name="value">A <see cref="double"/>.</param>
    /// <returns>A value indicating whether a <see cref="double"/> is a value other than <see cref="double.PositiveInfinity"/>, <see cref="double.NegativeInfinity"/>, and <see cref="double.NaN"/>.</returns>
    private static bool IsValidDouble(double value)
    {
        return !double.IsInfinity(value) && !double.IsNaN(value);
    }

    /// <summary>
    /// Offsets a <see cref="Rect"/> slightly from its current position.
    /// </summary>
    /// <param name="rect">A <see cref="Rect"/>.</param>
    /// <returns>The offset <see cref="Rect"/>.</returns>
    private static Rect Offset(this Rect rect)
    {
        if (!rect.HasSizeAndLocation())
        {
            return rect;
        }

        const int OffsetAmount = 0;

        Rect offsetRect = rect;

        // Move the rect down and to the right
        offsetRect.X += OffsetAmount;
        offsetRect.Y += OffsetAmount;

        if (offsetRect.IsOnScreen())
        {
            return offsetRect;
        }

        // Move the rect to the top and to the right
        offsetRect.X += OffsetAmount - Math.Floor((offsetRect.Y - SystemParameters.VirtualScreenTop) / OffsetAmount) * OffsetAmount;
        offsetRect.Y = SystemParameters.VirtualScreenTop;

        if (offsetRect.IsOnScreen())
        {
            return offsetRect;
        }

        // Move the rect to the far top-left
        offsetRect.X = SystemParameters.VirtualScreenLeft;
        offsetRect.Y = SystemParameters.VirtualScreenTop;

        if (offsetRect.IsOnScreen())
        {
            return offsetRect;
        }

        // Center the rect as a fallback
        return rect.CenterOnScreen();
    }

    private static readonly Action EmptyAction = delegate { };

    /// <summary>
    /// Force window layout update.
    /// </summary>
    /// <param name="window">The window.</param>
    public static void ForceUpdateLayout(this Window window)
    {
        try
        {
            window.Dispatcher.Invoke(EmptyAction, DispatcherPriority.Render);
        }
        catch(InvalidOperationException)
        {
            // Ignore.
        }
    }

    /// <summary>
    /// Open window context menu.
    /// </summary>
    /// <param name="window">The window.</param>
    public static void OpenContextMenu(this Window window)
    {
        try
        {
            if (window.ContextMenu is null)
            {
                return;
            }

            var contextMenuEventArgs = (ContextMenuEventArgs)Activator.CreateInstance(
                typeof(ContextMenuEventArgs),
#pragma warning disable S3011
                BindingFlags.Instance | BindingFlags.NonPublic,
#pragma warning restore S3011
                null,
                [window, true],
                null);

            contextMenuEventArgs.RoutedEvent = FrameworkElement.ContextMenuOpeningEvent;
            window.RaiseEvent(contextMenuEventArgs);

            window.ContextMenu.IsOpen = !contextMenuEventArgs.Handled;
        }
        catch
        {
            // Fallback.
            System.Windows.Forms.SendKeys.SendWait("+{F10}");
        }
    }

    public static void MoveToCurrentVirtualDesktop(this Window window) =>
        VirtualDesktopManager.Instance.MoveToCurrentVirtualDesktop(window);

    private static TaskDialog? _taskDialogInstance;

    public static MessageBoxResult ShowTaskDialog(this Window? window, string instruction, string yesText, string? noText = null, TaskDialogCheckBox? taskDialogCheckBox = null)
    {
        if (_taskDialogInstance is not null)
        {
            SetForegroundWindow(_taskDialogInstance.Handle);
            return MessageBoxResult.Cancel;
        }

        TaskDialogPage dialogPage = new()
        {
            Title = Properties.Resources.MessageBoxTitle,
            Text = Properties.Resources.WhatWouldYouDoTaskDialogText,
            Instruction = instruction,
            Icon = TaskDialogStandardIcon.Warning,
            CustomButtonStyle = TaskDialogCustomButtonStyle.CommandLinks,
            AllowCancel = true,
            CheckBox = taskDialogCheckBox
        };

        TaskDialogCustomButton yesButton = dialogPage.CustomButtons.Add(yesText);
        TaskDialogCustomButton? noButton = string.IsNullOrWhiteSpace(noText) ? null : dialogPage.CustomButtons.Add(noText);
        dialogPage.CustomButtons.Add(Properties.Resources.BackTaskDialogCommand);

        bool hasOwner = window is not null;

        TaskDialog dialog = new(dialogPage)
        {
            StartupLocation = hasOwner ? TaskDialogStartupLocation.CenterParent : TaskDialogStartupLocation.CenterScreen
        };

        dialog.Shown += OnShown;

        IntPtr handle = hasOwner ? new WindowInteropHelper(window!).Handle : IntPtr.Zero;
        TaskDialogButton result = dialog.Show(handle);

        MessageBoxResult messageBoxResult = ReferenceEquals(result, yesButton)
            ? MessageBoxResult.Yes
            : ReferenceEquals(result, noButton)
                ? MessageBoxResult.No
                : MessageBoxResult.Cancel;

        _taskDialogInstance = null;

        return messageBoxResult;

        void OnShown(object sender, EventArgs e)
        {
            dialog.Shown -= OnShown;
            _taskDialogInstance = dialog;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
#pragma warning disable S3241
        static extern bool SetForegroundWindow(IntPtr hWnd);
#pragma warning restore S3241
    }

    public static void Clean()
    {
        _taskDialogInstance?.Close();
        _taskDialogInstance = null;
    }

    public static double GetMinTrackWidth(this Visual visual) =>
        Math.Round(
                   GetSystemMetrics(34 /* SM_CXMINTRACK */) /
                   (PresentationSource.FromVisual(visual)?.CompositionTarget?.TransformToDevice.M11 ?? 1));

    public static double GetMinTrackHeight(this Visual visual) =>
        Math.Round(1+
                   GetSystemMetrics(35 /* SM_CYMINTRACK */) /
                   (PresentationSource.FromVisual(visual)?.CompositionTarget?.TransformToDevice.M22 ?? 1));

    public static bool IsTextBoxView(this object o) =>
        StringComparer.Ordinal.Equals(o.GetType().FullName, "System.Windows.Controls.TextBoxView");

    #endregion

    #region DllImport

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    #endregion
}