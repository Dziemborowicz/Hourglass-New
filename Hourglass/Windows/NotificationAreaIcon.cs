// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationAreaIcon.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Threading;

using Extensions;
using Managers;
using Properties;
using Timing;

using Application = System.Windows.Application;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

// ReSharper disable ExceptionNotDocumented

/// <summary>
/// Displays an icon for the app in the notification area of the taskbar.
/// </summary>
public class NotificationAreaIcon : IDisposable
{
    /// <summary>
    /// The timeout in milliseconds for the balloon tip that is shown when a timer has expired.
    /// </summary>
    private const int TimerExpiredBalloonTipTimeout = 10000;

    /// <summary>
    /// A <see cref="NotifyIcon"/>.
    /// </summary>
    private readonly NotifyIcon _notifyIcon;

    /// <summary>
    /// A <see cref="DispatcherTimer"/> used to raise events.
    /// </summary>
    private readonly DispatcherTimer _dispatcherTimer;

    /// <summary>
    /// Normal notification area icon.
    /// </summary>
    private readonly Icon _normalIcon;

    /// <summary>
    /// Silent notification area icon.
    /// </summary>
    private readonly Lazy<Icon> _silentIcon;

    /// <summary>
    /// Paused notification area icon.
    /// </summary>
    private readonly Lazy<Icon> _pausedIcon;

    /// <summary>
    /// Silent paused notification area icon.
    /// </summary>
    private readonly Lazy<Icon> _silentPausedIcon;

    /// <summary>
    /// Expired notification area icon.
    /// </summary>
    private readonly Lazy<Icon> _expiredIcon;

    /// <summary>
    /// Silent expired notification area icon.
    /// </summary>
    private readonly Lazy<Icon> _silentExpiredIcon;

    /// <summary>
    /// Paused and expired notification area icon.
    /// </summary>
    private readonly Lazy<Icon> _pausedExpiredIcon;

    /// <summary>
    /// Silent paused and expired notification area icon.
    /// </summary>
    private readonly Lazy<Icon> _silentPausedExpiredIcon;

    /// <summary>
    /// Indicates whether this object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Last mouse click time.
    /// </summary>
    private DateTime _lastClickTime;

    /// <summary>
    /// Gets arranged windows.
    /// </summary>
    private static IEnumerable<TimerWindow> ArrangedWindows => Application.Current?.Windows.OfType<TimerWindow>().Arrange() ?? [];

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationAreaIcon"/> class.
    /// </summary>
    public NotificationAreaIcon()
    {
        _normalIcon = new(Resources.TrayIcon, SystemInformation.SmallIconSize);
        _silentIcon = new(CreateSilentIcon);

        _pausedIcon              = new(() => CreateOverlayIcon(false, true,  false));
        _silentPausedIcon        = new(() => CreateOverlayIcon(true,  true,  false));
        _expiredIcon             = new(() => CreateOverlayIcon(false, false, true));
        _silentExpiredIcon       = new(() => CreateOverlayIcon(true,  false, true));
        _pausedExpiredIcon       = new(() => CreateOverlayIcon(false, true,  true));
        _silentPausedExpiredIcon = new(() => CreateOverlayIcon(true,  true,  true));

        _notifyIcon = new()
        {
            Icon = _normalIcon,
            ContextMenu = new()
        };

        _notifyIcon.MouseUp += NotifyIconMouseUp;
        _notifyIcon.MouseMove += NotifyIconMouseMove;

        _notifyIcon.BalloonTipClicked += BalloonTipClicked;

        _notifyIcon.ContextMenu.Popup += ContextMenuPopup;
        _notifyIcon.ContextMenu.Collapse += ContextMenuCollapse;

        _dispatcherTimer = new(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _dispatcherTimer.Tick += DispatcherTimerTick;

        Settings.Default.PropertyChanged += SettingsPropertyChanged;
        IsVisible = Settings.Default.ShowInNotificationArea;

        Icon CreateSilentIcon()
        {
            using Bitmap normalIconBitmap = _normalIcon.ToBitmap();
            using Bitmap silentIconBitmap = (Bitmap)ToolStripRenderer.CreateDisabledImage(normalIconBitmap);

            return Icon.FromHandle(silentIconBitmap.GetHicon());
        }
    }

    private Icon CreateOverlayIcon(bool silent, bool paused, bool expired)
    {
        const int diameter          = 8;
        const int circleBorderWidth = 1;

        const int pauseWidth        = 1;
        const int pause1LeftOffset  = 2;
        const int pause2LeftOffset  = 4;
        const int pauseTopOffset    = 2;
        const int pauseBottomOffset = 4;

        Color circlePenColor = Color.FromArgb(unchecked((int)0xFF303030));
        Brush circleBrush    = expired ? Brushes.Crimson : Brushes.White;
        Color pauseLineColor = expired ? Color.White     : Color.Black;

        int width  = _normalIcon.Width;
        int height = _normalIcon.Height;

        using Bitmap bitmap = (silent ? _silentIcon.Value : _normalIcon).ToBitmap();
        using Graphics graphics = Graphics.FromImage(bitmap);

        graphics.SmoothingMode = SmoothingMode.HighQuality;

        int circleX = width  - diameter - circleBorderWidth;
        int circleY = height - diameter - circleBorderWidth;

        graphics.FillEllipse(circleBrush, circleX, circleY, diameter, diameter);

        using Pen circlePen = new(circlePenColor, circleBorderWidth);
        graphics.DrawEllipse(circlePen, circleX, circleY, diameter, diameter);

        graphics.SmoothingMode = SmoothingMode.Default;

        if (paused)
        {
            using SolidBrush pauseLineBrush = new(pauseLineColor);
            DrawPauseLine(pause1LeftOffset);
            DrawPauseLine(pause2LeftOffset);

            void DrawPauseLine(int leftOffset) =>
                graphics.FillRectangle(
                    pauseLineBrush,
                    width  - diameter + leftOffset,
                    height - diameter + pauseTopOffset,
                    pauseWidth,
                    diameter - pauseBottomOffset);
        }

        return Icon.FromHandle(bitmap.GetHicon());
    }

    /// <summary>
    /// Gets or sets a value indicating whether the icon is visible in the notification area of the taskbar.
    /// </summary>
    private bool IsVisible
    {
        get => _notifyIcon.Visible;
        set
        {
            _notifyIcon.Visible = value;

            if (value)
            {
                RefreshIcon();
            }
        }
    }

    /// <summary>
    /// Displays a balloon tip notifying that a timer has expired.
    /// </summary>
    public void ShowBalloonTipForExpiredTimer()
    {
        _notifyIcon.ShowBalloonTip(
            TimerExpiredBalloonTipTimeout,
            Resources.NotificationAreaIconTimerExpired,
            Resources.NotificationAreaIconYourTimerHasExpired,
            ToolTipIcon.Info);
    }

    /// <summary>
    /// Disposes the <see cref="NotificationAreaIcon"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the <see cref="NotificationAreaIcon"/>.
    /// </summary>
    /// <param name="disposing">A value indicating whether this method was invoked by an explicit call to <see
    /// cref="Dispose"/>.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (!disposing)
        {
            return;
        }

        _dispatcherTimer.Stop();

        _notifyIcon.Dispose();
        _normalIcon.Dispose();

        DisposeIcon(_silentIcon);
        DisposeIcon(_pausedIcon);
        DisposeIcon(_silentPausedIcon);
        DisposeIcon(_expiredIcon);
        DisposeIcon(_silentExpiredIcon);
        DisposeIcon(_pausedExpiredIcon);
        DisposeIcon(_silentPausedExpiredIcon);

        Settings.Default.PropertyChanged -= SettingsPropertyChanged;

        static void DisposeIcon(Lazy<Icon> lazyIcon)
        {
            if (lazyIcon.IsValueCreated)
            {
                lazyIcon.Value.Dispose();
            }
        }
    }

    /// <summary>
    /// Restores all <see cref="TimerWindow"/>s.
    /// </summary>
    private static void RestoreAllTimerWindows()
    {
        if (Application.Current is null)
        {
            return;
        }

        foreach (TimerWindow window in Application.Current.Windows.OfType<TimerWindow>().ArrangeDescending())
        {
            window.BringToFrontAndActivate();
        }
    }

    /// <summary>
    /// Restores all <see cref="TimerWindow"/>s that show expired timers.
    /// </summary>
    private static void RestoreAllExpiredTimerWindows()
    {
        if (Application.Current is null)
        {
            return;
        }

        foreach (TimerWindow window in Application.Current.Windows.OfType<TimerWindow>().Where(static w => w.Timer.State == TimerState.Expired))
        {
            window.BringToFrontAndActivate();
        }
    }

    /// <summary>
    /// Invoked after the value of an application settings property is changed.
    /// </summary>
    /// <param name="sender">The settings object.</param>
    /// <param name="e">The event data.</param>
    private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (IsVisible == Settings.Default.ShowInNotificationArea)
        {
            return;
        }

        IsVisible = Settings.Default.ShowInNotificationArea;

        if (!IsVisible)
        {
            RestoreAllTimerWindows();
        }
    }

    private bool IsDoubleClick(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return false;
        }

        DateTime clickTime = DateTime.UtcNow;
        if (_lastClickTime == DateTime.MinValue)
        {
            _lastClickTime = clickTime;
            return false;
        }

        if ((clickTime - _lastClickTime).Duration() > TimeSpan.FromMilliseconds(SystemInformation.DoubleClickTime))
        {
            _lastClickTime = clickTime;
            return false;
        }

        _lastClickTime = DateTime.MinValue;
        return true;
    }

    private void NotifyIconMouseUp(object sender, MouseEventArgs e)
    {
        if (Application.Current is null)
        {
            return;
        }

        if (!IsDoubleClick(e))
        {
            ProcessMiddleClick();
            ProcessLeftClick();

            return;
        }

        TimerWindow[] windows = Application.Current.Windows.OfType<TimerWindow>()
            .Where(static window => window.WindowState != WindowState.Minimized)
            .ToArray();

        if (windows.Any())
        {
            MinimizeAllTimerWindows();
            return;
        }

        RestoreAllTimerWindows();

        void ProcessMiddleClick()
        {
            if (e.Button != MouseButtons.Middle)
            {
                return;
            }

            ShowTimerContextMenu(show: !IsKeyDown(ModifierKeys.Shift));
        }

        void ProcessLeftClick()
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (IsKeyDown(ModifierKeys.Shift))
            {
                ArrangedWindows.FirstOrDefault()?.BringToFrontAndActivate();
                return;
            }

            if (IsKeyDown(ModifierKeys.Control))
            {
                NewTimer();
            }
        }

        void MinimizeAllTimerWindows()
        {
            int id = TimerWindow.LastActivatedID;

            try
            {
                foreach (TimerWindow window in windows)
                {
                    window.DoNotActivateNextWindow = true;
                    window.WindowState = WindowState.Minimized;
                }
            }
            finally
            {
                TimerWindow.LastActivatedID = id;
            }
        }
    }

    /// <summary>
    /// Invoked when the user moves the mouse while the pointer is over the icon in the notification area of the
    /// taskbar.
    /// </summary>
    /// <param name="sender">The <see cref="NotifyIcon"/>.</param>
    /// <param name="e">The event data.</param>
    private void NotifyIconMouseMove(object sender, MouseEventArgs e)
    {
        if (Application.Current is null)
        {
            return;
        }

        string[] windowStrings = ArrangedWindows
            .Select(static window => window.ToString())
            .Where(static windowString => !string.IsNullOrWhiteSpace(windowString))
            .ToArray();

        if (!windowStrings.Any())
        {
            _notifyIcon.Text = Resources.NoTimersNotificationAreaText;
            return;
        }

        const int maxSize = 63;

        StringBuilder builder = new(maxSize);

        foreach (string windowString in windowStrings)
        {
            if (builder.Length == 0)
            {
                builder.Append(windowString);
                continue;
            }

            builder.AppendLine();
            builder.Append(windowString);

            if (builder.Length > maxSize)
            {
                break;
            }
        }

        if (builder.Length > maxSize)
        {
            const string dots = "...";

            int maxTextSize = maxSize - dots.Length;

            builder.Remove(maxTextSize, builder.Length - maxTextSize);
            builder.Append(dots);
        }

        _notifyIcon.Text = builder.ToString();
    }

    /// <summary>
    /// Invoked when the balloon tip is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="NotifyIcon"/>.</param>
    /// <param name="e">The event data.</param>
    private static void BalloonTipClicked(object sender, EventArgs e)
    {
        RestoreAllExpiredTimerWindows();
    }

    private static Lazy<Func<TimerWindow, string>> CreateTimerMenuItemTextGenerator()
    {
        int i = 0;

        return new(() => window => string.Format(Resources.NotificationAreaIconTimerMenuItemText, ++i % 10, window));
    }

    /// <summary>
    /// Invoked before the notify icon context menu is displayed.
    /// </summary>
    /// <param name="sender">The notify icon context menu.</param>
    /// <param name="e">The event data.</param>
    private void ContextMenuPopup(object sender, EventArgs e)
    {
        _notifyIcon.ContextMenu.MenuItems.Clear();

        TimerWindow[] windows = ArrangedWindows.ToArray();

        bool hasApplication = Application.Current is not null;

        if (hasApplication)
        {
            if (IsKeyDown(ModifierKeys.Shift) && ShowTimerContextMenu())
            {
                return;
            }

            _notifyIcon.ContextMenu.MenuItems.AddRange(GetApplicationMenuItems().ToArray());
        }

        MenuItem exitMenuItem = new(Resources.NotificationAreaIconExitMenuItem);
        exitMenuItem.Click += ExitMenuItemClick;
        _notifyIcon.ContextMenu.MenuItems.Add(exitMenuItem);

        if (hasApplication)
        {
            _dispatcherTimer.Start();
        }

        IEnumerable<MenuItem> GetApplicationMenuItems()
        {
            MenuItem menuItem = new(Resources.NotificationAreaIconNewTimerMenuItem);
            menuItem.Click += NewTimerMenuItemClick;
            yield return menuItem;

            yield return NewSeparatorMenuItem();

            Lazy<Func<TimerWindow, string>> timerMenuItemTextGenerator = CreateTimerMenuItemTextGenerator();

            bool shouldAddSeparator = false;

            foreach (TimerWindow window in windows)
            {
                shouldAddSeparator = true;

                menuItem = new(timerMenuItemTextGenerator.Value(window))
                {
                    Tag = new WeakReference<TimerWindow>(window)
                };
                menuItem.Click += WindowMenuItemClick;
                yield return menuItem;
            }

            if (shouldAddSeparator)
            {
                shouldAddSeparator = false;

                yield return NewSeparatorMenuItem();

                if (TimerManager.CanPauseAll())
                {
                    shouldAddSeparator = true;
                    menuItem = new(Resources.NotificationAreaIconPauseAllMenuItem);
                    menuItem.Click += static delegate { TimerManager.PauseAll(); };
                    yield return menuItem;
                }

                if (TimerManager.CanResumeAll())
                {
                    shouldAddSeparator = true;
                    menuItem = new(Resources.NotificationAreaIconResumeAllMenuItem);
                    menuItem.Click += static delegate { TimerManager.ResumeAll(); };
                    yield return menuItem;
                }
            }

            if (shouldAddSeparator)
            {
                yield return NewSeparatorMenuItem();
            }

            if (windows.Any())
            {
                menuItem = new(Resources.NotificationAreaOptionsMenuItem);
                menuItem.Click += OpenOptionsMenuItemEventHandler;
                yield return menuItem;

                menuItem = new(Resources.NotificationAreaIconSilentModeMenuItem)
                {
                    Checked = TimerManager.Instance.SilentMode
                };
                menuItem.Click += MenuItemClickEventHandler;
                yield return menuItem;

                yield return NewSeparatorMenuItem();

                void MenuItemClickEventHandler(object sender1, EventArgs e1)
                {
                    TimerManager.Instance.ToggleSilentMode();
                    RefreshIcon();
                }

                void OpenOptionsMenuItemEventHandler(object sender1, EventArgs e1) =>
                    ShowTimerContextMenu();
            }

            menuItem = new(Resources.NotificationAreaIconAboutMenuItem);
            menuItem.Click += static delegate { AboutDialog.ShowOrActivate(); };
            yield return menuItem;

            yield return NewSeparatorMenuItem();

            static MenuItem NewSeparatorMenuItem() =>
                new("-");
        }
    }

    /// <summary>
    /// Invoked when the <see cref="_dispatcherTimer"/> interval has elapsed.
    /// </summary>
    /// <param name="sender">The <see cref="DispatcherTimer"/>.</param>
    /// <param name="e">The event data.</param>
    private void DispatcherTimerTick(object sender, EventArgs e)
    {
        Lazy<Func<TimerWindow, string>> timerMenuItemTextGenerator = CreateTimerMenuItemTextGenerator();

        foreach (MenuItem menuItem in _notifyIcon.ContextMenu.MenuItems)
        {
            TimerWindow? window = GetFromTag(menuItem);
            if (window is null)
            {
                continue;
            }

            if (!window.Timer.Disposed)
            {
                window.Timer.Update();
            }

            menuItem.Text = timerMenuItemTextGenerator.Value(window);
        }
    }

    /// <summary>
    /// Invoked when the shortcut menu collapses.
    /// </summary>
    /// <remarks>
    /// The Microsoft .NET Framework does not call this method consistently.
    /// </remarks>
    /// <param name="sender">The notify icon context menu.</param>
    /// <param name="e">The event data.</param>
    private void ContextMenuCollapse(object sender, EventArgs e)
    {
        _dispatcherTimer.Stop();
    }

    /// <summary>
    /// Invoked when the "New timer" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private static void NewTimerMenuItemClick(object sender, EventArgs e)
    {
        NewTimer();
    }

    /// <summary>
    /// Invoked when a <see cref="MenuItem"/> for a <see cref="TimerWindow"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private static void WindowMenuItemClick(object sender, EventArgs e)
    {
        GetFromTag((MenuItem)sender)?.BringToFrontAndActivate();
    }

    private static TimerWindow? GetFromTag(MenuItem menuItem) =>
        menuItem.Tag is WeakReference<TimerWindow> windowWeakReference &&
        windowWeakReference.TryGetTarget(out TimerWindow? window)
            ? window
            : null;

    /// <summary>
    /// Invoked when the "Exit" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private static void ExitMenuItemClick(object sender, EventArgs e)
    {
        if (Application.Current is null)
        {
            AppManager.Instance.Dispose();
            Environment.Exit(0);
            return;
        }

        TimerWindow? firstTimerWindow = ArrangedWindows
            .FirstOrDefault(static window => window.Options is { LockInterface: false, PromptOnExit: true } && IsTimerRunningFor(window));

        if (firstTimerWindow is not null)
        {
            WindowState windowState = firstTimerWindow.WindowState;

            firstTimerWindow.BringToFrontAndActivate();

            MessageBoxResult result = firstTimerWindow.ShowTaskDialog(
                Resources.ExitMenuTaskDialogInstruction,
                Resources.StopAndExitMenuTaskDialogCommand);

            firstTimerWindow.WindowState = windowState;

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        foreach (Window window in Application.Current.Windows)
        {
            if (window is TimerWindow timerWindow)
            {
                if (timerWindow.Options.LockInterface)
                {
                    continue;
                }

                timerWindow.DoNotActivateNextWindow = true;
                timerWindow.DoNotPromptOnExit = true;
            }

            window.Close();
        }

        static bool IsTimerRunningFor(TimerWindow window) =>
            window.Timer.State != TimerState.Stopped &&
            window.Timer.State != TimerState.Expired;
    }

    /// <summary>
    /// Refreshes notification area icon.
    /// </summary>
    public void RefreshIcon()
    {
        bool silent  = TimerManager.Instance.SilentMode;
        bool paused  = TimerManager.GetPausableTimers(TimerState.Paused ).Any();
        bool expired = TimerManager.GetTimersByState (TimerState.Expired).Any();

        _notifyIcon.Icon = true switch
        {
            _ when paused && expired =>
                GetIcon(_silentPausedExpiredIcon, _pausedExpiredIcon),
            _ when expired =>
                GetIcon(_silentExpiredIcon, _expiredIcon),
            _ when paused =>
                GetIcon(_silentPausedIcon, _pausedIcon),
            _ =>
                silent ? _silentIcon.Value : _normalIcon
        };

        Icon GetIcon(Lazy<Icon> silentIcon, Lazy<Icon> normalIcon) =>
            (silent ? silentIcon : normalIcon).Value;
    }

    /// <summary>
    /// Checks whether modifier keys are down.
    /// </summary>
    /// <param name="modifierKeys">Modifier keys.</param>
    /// <returns><c>true</c> if keys are down, <c>false</c> otherwise.</returns>
    private static bool IsKeyDown(ModifierKeys modifierKeys) =>
        (Keyboard.Modifiers ^ modifierKeys) == 0;

    /// <summary>
    /// Creates new timer.
    /// </summary>
    private static void NewTimer()
    {
        TimerWindow? existingWindow = GetExistingTimerWindow();
        TimerWindow window = new();

        if (existingWindow is not null)
        {
            window.Options.Set(existingWindow.Options);
            window.Options.Title = null;
        }

        window.RestoreFromSibling();
        window.Show();
    }

    /// <summary>
    /// Shows the timer context menu.
    /// </summary>
    /// <param name="show">Show context menu.</param>
    /// <returns><c>true</c> if the timer context menu is shown, <c>false</c> otherwise.</returns>
    private static bool ShowTimerContextMenu(bool show = true)
    {
        TimerWindow? window = GetExistingTimerWindow();
        if (window is null)
        {
            return false;
        }

        window.BringToFrontAndActivate(true, show);

        return true;
    }

    /// <summary>
    /// Returns an any existing timer window.
    /// </summary>
    /// <returns>An any existing <see cref="TimerWindow"/>.</returns>
    private static TimerWindow? GetExistingTimerWindow()
    {
        TimerWindow[] windows = ArrangedWindows.ToArray();

        return
            Array.Find(windows, static window => window.ID == TimerWindow.LastActivatedID) ??
            Array.Find(windows, static window => window.IsVisible) ??
            windows.FirstOrDefault();
    }
}