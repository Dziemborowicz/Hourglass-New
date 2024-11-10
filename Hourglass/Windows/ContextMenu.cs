// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContextMenu.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using Extensions;
using Managers;
using Properties;
using Timing;

// ReSharper disable ExceptionNotDocumented

/// <summary>
/// A <see cref="System.Windows.Controls.ContextMenu"/> for the <see cref="TimerWindow"/>.
/// </summary>
public sealed class ContextMenu : System.Windows.Controls.ContextMenu
{
    #region Private Members

    /// <summary>
    /// The <see cref="TimerWindow"/> that uses this context menu.
    /// </summary>
    private TimerWindow _timerWindow = null!;

    /// <summary>
    /// A <see cref="DispatcherTimer"/> used to raise events.
    /// </summary>
    private DispatcherTimer _dispatcherTimer = null!;

    /// <summary>
    /// The "Pause all" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _pauseAllMenuItem = null!;

    /// <summary>
    /// The "Pause all" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _resumeAllMenuItem = null!;

    /// <summary>
    /// The "Pause all", "Resume all" menu items <see cref="Separator"/>.
    /// </summary>
    private Separator _pauseResumeAllSeparator = null!;

    /// <summary>
    /// The "Always on top" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _alwaysOnTopMenuItem = null!;

    /// <summary>
    /// The "Full screen" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _fullScreenMenuItem = null!;

    /// <summary>
    /// The "Prompt on exit" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _promptOnExitMenuItem = null!;

    /// <summary>
    /// The "Show progress in taskbar" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _showProgressInTaskbarMenuItem = null!;

    /// <summary>
    /// The "Show in notification area" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _showInNotificationAreaMenuItem = null!;

    /// <summary>
    /// The "Loop timer" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _loopTimerMenuItem = null!;

    /// <summary>
    /// The "Pause before looping" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _pauseBeforeLoopTimerMenuItem = null!;

    /// <summary>
    /// The "Pop up when expired" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _popUpWhenExpiredMenuItem = null!;

    /// <summary>
    /// The "Close when expired" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _closeWhenExpiredMenuItem = null!;

    /// <summary>
    /// The "Recent inputs" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _recentInputsMenuItem = null!;

    /// <summary>
    /// The "Clear recent inputs" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem? _clearRecentInputsMenuItem;

    /// <summary>
    /// The "Saved timers" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _savedTimersMenuItem = null!;

    /// <summary>
    /// The "Open all saved timers" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem? _openAllSavedTimersMenuItem;

    /// <summary>
    /// The "Clear saved timers" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem? _clearSavedTimersMenuItem;

    /// <summary>
    /// The "Theme" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _themeMenuItem = null!;

    /// <summary>
    /// The "Light theme" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem? _lightThemeMenuItem;

    /// <summary>
    /// The "Dark theme" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem? _darkThemeMenuItem;

    /// <summary>
    /// The "Manage themes" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem? _manageThemesMenuItem;

    /// <summary>
    /// The "Theme" <see cref="MenuItem"/>s associated with <see cref="Theme"/>s.
    /// </summary>
    private IList<MenuItem> _selectableThemeMenuItems = null!;

    /// <summary>
    /// The "Sound" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _soundMenuItem = null!;

    /// <summary>
    /// The "Sound" <see cref="MenuItem"/>s associated with <see cref="Sound"/>s.
    /// </summary>
    private IList<MenuItem> _selectableSoundMenuItems = null!;

    /// <summary>
    /// The "Loop sound" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem? _loopSoundMenuItem;

    /// <summary>
    /// The "Do not keep computer awake" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _doNotKeepComputerAwakeMenuItem = null!;

    /// <summary>
    /// The "Open saved timers on startup" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _openSavedTimersOnStartupMenuItem = null!;

    /// <summary>
    /// The "Display time in the digital clock format" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _digitalClockTimeMenuItem = null!;

    /// <summary>
    /// The "Prefer 24-hour time when parsing" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _prefer24HourTimeMenuItem = null!;

    /// <summary>
    /// The "Activate next window when minimized or closed" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _activateNextWindowMenuItem = null!;

    /// <summary>
    /// The "Order by the title first" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _orderByTitleFirstMenuItem = null!;

    /// <summary>
    /// The "Reverse progress bar" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _reverseProgressBarMenuItem = null!;

    /// <summary>
    /// The "Show time elapsed" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _showTimeElapsedMenuItem = null!;

    /// <summary>
    /// The "Show trigger time" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _showTriggerTimeMenuItem = null!;

    /// <summary>
    /// The "Shut down when expired" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _shutDownWhenExpiredMenuItem = null!;

    /// <summary>
    /// The "Window title" <see cref="MenuItem"/>s associated with <see cref="WindowTitleMode"/>s.
    /// </summary>
    private IList<MenuItem> _selectableWindowTitleMenuItems = null!;

    /// <summary>
    /// The "Restore" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _restoreMenuItem = null!;

    /// <summary>
    /// The "Minimize" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _minimizeMenuItem = null!;

    /// <summary>
    /// The "Maximize" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _maximizeMenuItem = null!;

    #endregion

    /// <summary>
    /// Gets the date and time for the menu being last visible.
    /// </summary>
    public DateTime LastShown { get; private set; }

    /// <summary>
    /// Binds the <see cref="ContextMenu"/> to a <see cref="TimerWindow"/>.
    /// </summary>
    /// <param name="window">A <see cref="TimerWindow"/>.</param>
    /// <exception cref="InvalidOperationException">The timer window is already created.</exception>
    public void Bind(TimerWindow window)
    {
        // Validate state
        if (_timerWindow is not null)
        {
            throw new InvalidOperationException(@"The timer window is already created");
        }

        SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);

        // Initialize members
        _timerWindow = window ?? throw new ArgumentNullException(nameof(window));

        _timerWindow.ContextMenuOpening += WindowContextMenuOpening;
        _timerWindow.ContextMenuClosing += WindowContextMenuClosing;
        _timerWindow.ContextMenu = this;

        _dispatcherTimer = new(DispatcherPriority.Normal, Dispatcher)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _dispatcherTimer.Tick += DispatcherTimerTick;

        _selectableThemeMenuItems = [];
        _selectableSoundMenuItems = [];
        _selectableWindowTitleMenuItems = [];

        // Build the menu
        BuildMenu();
    }

    /// <summary>
    /// Unbind menu.
    /// </summary>
    public void Unbind()
    {
        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_dispatcherTimer is not null)
        {
            _dispatcherTimer.Stop();
            _dispatcherTimer.Tick -= DispatcherTimerTick;

            _dispatcherTimer = null!;
        }

        if (_timerWindow is not null)
        {
            _timerWindow.ContextMenuOpening -= WindowContextMenuOpening;
            _timerWindow.ContextMenuClosing -= WindowContextMenuClosing;

            _timerWindow = null!;
        }
        // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    }

    #region Private Methods (Lifecycle)

    /// <summary>
    /// Invoked when the context menu is opened.
    /// </summary>
    /// <param name="sender">The bound <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void WindowContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        // Do not show the context menu if the user interface is locked
        if (_timerWindow.Options.LockInterface)
        {
            e.Handled = true;
            return;
        }

        // Update dynamic items
        UpdateRecentInputsMenuItem();
        UpdateSavedTimersMenuItem();
        UpdateThemeMenuItem();
        UpdateSoundMenuItem();
        UpdateWindowStateMenuItems();

        // Update binding
        UpdateMenuFromOptions();

        LastShown = DateTime.Now;
        _dispatcherTimer.Start();
    }

    /// <summary>
    /// Invoked when the <see cref="_dispatcherTimer"/> interval has elapsed.
    /// </summary>
    /// <param name="sender">The <see cref="DispatcherTimer"/>.</param>
    /// <param name="e">The event data.</param>
    private void DispatcherTimerTick(object sender, EventArgs e)
    {
        LastShown = DateTime.Now;
        UpdateSavedTimersHeaders();
    }

    /// <summary>
    /// Invoked just before the context menu is closed.
    /// </summary>
    /// <param name="sender">The bound <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void WindowContextMenuClosing(object sender, ContextMenuEventArgs e)
    {
        UpdateOptionsFromMenu();

        LastShown = DateTime.Now;
        _dispatcherTimer.Stop();

        AppManager.Instance.Persist();
    }

    #endregion

    #region Private Methods (Binding)

    /// <summary>
    /// Reads the options from the <see cref="TimerOptions"/> and applies them to this menu.
    /// </summary>
    private void UpdateMenuFromOptions()
    {
        UpdatePauseResumeAll();

        // Always on top
        _alwaysOnTopMenuItem.IsChecked = _timerWindow.Options.AlwaysOnTop;

        // Full screen
        _fullScreenMenuItem.IsChecked = _timerWindow.IsFullScreen;

        // Prompt on exit
        _promptOnExitMenuItem.IsChecked = _timerWindow.Options.PromptOnExit;

        // Show progress in taskbar
        _showProgressInTaskbarMenuItem.IsChecked = _timerWindow.Options.ShowProgressInTaskbar;

        // Show in notification area
        _showInNotificationAreaMenuItem.IsChecked = Settings.Default.ShowInNotificationArea;

        // Loop timer
        // Pause before looping
        if (_timerWindow.Timer.SupportsLooping)
        {
            _loopTimerMenuItem.IsEnabled = true;
            _loopTimerMenuItem.IsChecked = _timerWindow.Options.LoopTimer;
            _pauseBeforeLoopTimerMenuItem.IsEnabled = _timerWindow.Options.LoopTimer;
            _pauseBeforeLoopTimerMenuItem.IsChecked = _pauseBeforeLoopTimerMenuItem.IsEnabled && _timerWindow.Options.PauseBeforeLoopTimer;
        }
        else
        {
            _loopTimerMenuItem.IsEnabled = false;
            _loopTimerMenuItem.IsChecked = false;
            _pauseBeforeLoopTimerMenuItem.IsEnabled = false;
            _pauseBeforeLoopTimerMenuItem.IsChecked = false;
        }

        // Pop up when expired
        _popUpWhenExpiredMenuItem.IsChecked = _timerWindow.Options.PopUpWhenExpired;

        // Close when expired
        if ((!_timerWindow.Options.LoopTimer || !_timerWindow.Timer.SupportsLooping) && !_timerWindow.Options.LoopSound)
        {
            _closeWhenExpiredMenuItem.IsChecked = _timerWindow.Options.CloseWhenExpired;
            _closeWhenExpiredMenuItem.IsEnabled = true;
        }
        else
        {
            _closeWhenExpiredMenuItem.IsChecked = false;
            _closeWhenExpiredMenuItem.IsEnabled = false;
        }

        UpdateThemesMenuItems();

        // Sound
        foreach (var menuItem in _selectableSoundMenuItems)
        {
            menuItem.IsChecked = ReferenceEquals(menuItem.Tag, _timerWindow.Options.Sound);
        }

        // Loop sound
        _loopSoundMenuItem!.IsChecked = _timerWindow.Options.LoopSound;

        // Do not keep computer awake
        _doNotKeepComputerAwakeMenuItem.IsChecked = _timerWindow.Options.DoNotKeepComputerAwake;

        // Open saved timers on startup
        _openSavedTimersOnStartupMenuItem.IsChecked = Settings.Default.OpenSavedTimersOnStartup;

        // Prefer 24-hour time when parsing
        _prefer24HourTimeMenuItem.IsChecked = Settings.Default.Prefer24HourTime;

        // Activate next window when minimized or closed
        _activateNextWindowMenuItem.IsChecked = Settings.Default.ActivateNextWindow;

        // Order by the title first.
        _orderByTitleFirstMenuItem.IsChecked = Settings.Default.OrderByTitleFirst;

        // Display time in the digital clock format
        _digitalClockTimeMenuItem.IsChecked = _timerWindow.Options.DigitalClockTime;

        // Reverse progress bar
        _reverseProgressBarMenuItem.IsChecked = _timerWindow.Options.ReverseProgressBar;

        // Show time elapsed
        _showTimeElapsedMenuItem.IsChecked = _timerWindow.Options.ShowTimeElapsed;

        // Show trigger time
        _showTriggerTimeMenuItem.IsChecked = _timerWindow.Options.ShowTriggerTime;

        // Shut down when expired
        if ((!_timerWindow.Options.LoopTimer || !_timerWindow.Timer.SupportsLooping) && !_timerWindow.Options.LoopSound)
        {
            _shutDownWhenExpiredMenuItem.IsChecked = _timerWindow.Options.ShutDownWhenExpired;
            _shutDownWhenExpiredMenuItem.IsEnabled = true;
        }
        else
        {
            _shutDownWhenExpiredMenuItem.IsChecked = false;
            _shutDownWhenExpiredMenuItem.IsEnabled = false;
        }

        // Window title
        foreach (var menuItem in _selectableWindowTitleMenuItems)
        {
            var windowTitleMode = (WindowTitleMode)menuItem.Tag;
            menuItem.IsChecked = windowTitleMode == _timerWindow.Options.WindowTitleMode;
        }

        void UpdatePauseResumeAll()
        {
            var canPauseAll = TimerManager.CanPauseAll();
            var canResumeAll = TimerManager.CanResumeAll();

            _pauseAllMenuItem.Visibility = canPauseAll.ToVisibility();
            _resumeAllMenuItem.Visibility = canResumeAll.ToVisibility();
            _pauseResumeAllSeparator.Visibility = (canPauseAll || canResumeAll).ToVisibility();
        }
    }

    /// <summary>
    /// Reads the options from this menu and applies them to the <see cref="TimerOptions"/>.
    /// </summary>
    private void UpdateOptionsFromMenu()
    {
        // Always on top
        _timerWindow.Options.AlwaysOnTop = _alwaysOnTopMenuItem.IsChecked;

        // Full screen
        _timerWindow.IsFullScreen = _fullScreenMenuItem.IsChecked;

        // Prompt on exit
        _timerWindow.Options.PromptOnExit = _promptOnExitMenuItem.IsChecked;

        // Show progress in taskbar
        _timerWindow.Options.ShowProgressInTaskbar = _showProgressInTaskbarMenuItem.IsChecked;

        // Show in notification area
        Settings.Default.ShowInNotificationArea = _showInNotificationAreaMenuItem.IsChecked;

        // Loop timer
        if (_loopTimerMenuItem.IsEnabled)
        {
            _timerWindow.Options.LoopTimer = _loopTimerMenuItem.IsChecked;
        }

        // Pause before looping
        if (_pauseBeforeLoopTimerMenuItem.IsEnabled)
        {
            _timerWindow.Options.PauseBeforeLoopTimer = _pauseBeforeLoopTimerMenuItem.IsChecked;
        }

        // Pop up when expired
        _timerWindow.Options.PopUpWhenExpired = _popUpWhenExpiredMenuItem.IsChecked;

        // Close when expired
        if (_closeWhenExpiredMenuItem.IsEnabled)
        {
            _timerWindow.Options.CloseWhenExpired = _closeWhenExpiredMenuItem.IsChecked;
        }

        // Sound
        var selectedSoundMenuItem = _selectableSoundMenuItems.FirstOrDefault(static mi => mi.IsChecked);
        _timerWindow.Options.Sound = selectedSoundMenuItem?.Tag as Sound;

        // Loop sound
        _timerWindow.Options.LoopSound = _loopSoundMenuItem?.IsChecked == true;

        // Do not keep computer awake
        _timerWindow.Options.DoNotKeepComputerAwake = _doNotKeepComputerAwakeMenuItem.IsChecked;

        // Open saved timers on startup
        Settings.Default.OpenSavedTimersOnStartup = _openSavedTimersOnStartupMenuItem.IsChecked;

        // Prefer 24-hour time when parsing
        Settings.Default.Prefer24HourTime = _prefer24HourTimeMenuItem.IsChecked;

        // Activate next window when minimized or closed
        Settings.Default.ActivateNextWindow = _activateNextWindowMenuItem.IsChecked;

        // Order by the title first.
        Settings.Default.OrderByTitleFirst = _orderByTitleFirstMenuItem.IsChecked;

        // Display time in the digital clock format
        Settings.Default.DigitalClockTime = _digitalClockTimeMenuItem.IsChecked;

        // Reverse progress bar
        _timerWindow.Options.ReverseProgressBar = _reverseProgressBarMenuItem.IsChecked;

        // Display time in the digital clock format
        _timerWindow.Options.DigitalClockTime = _digitalClockTimeMenuItem.IsChecked;

        // Show time elapsed
        _timerWindow.Options.ShowTimeElapsed = _showTimeElapsedMenuItem.IsChecked;

        // Show time elapsed
        _timerWindow.Options.ShowTriggerTime = _showTriggerTimeMenuItem.IsChecked;

        // Shut down when expired
        if (_shutDownWhenExpiredMenuItem.IsEnabled)
        {
            _timerWindow.Options.ShutDownWhenExpired = _shutDownWhenExpiredMenuItem.IsChecked;
        }

        // Window title
        var selectedWindowTitleMenuItem = _selectableWindowTitleMenuItems.FirstOrDefault(static mi => mi.IsChecked);
        _timerWindow.Options.WindowTitleMode = selectedWindowTitleMenuItem is not null
            ? (WindowTitleMode)selectedWindowTitleMenuItem.Tag
            : WindowTitleMode.ApplicationName;
    }

    /// <summary>
    /// Invoked when a checkable <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void CheckableMenuItemClick(object sender, RoutedEventArgs e)
    {
        UpdateOptionsFromMenu();
        UpdateMenuFromOptions();
    }

    #endregion

    /// <summary>
    /// Builds or rebuilds the context menu.
    /// </summary>
    private void BuildMenu()
    {
        Items.Clear();

        // New timer
        MenuItem newTimerMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuNewTimerMenuItem,
            InputGestureText = TimerWindow.NewTimerKeyGesture.ToInputGestureText()
        };
        newTimerMenuItem.Click += NewTimerMenuItemClick;
        Items.Add(newTimerMenuItem);

        Items.Add(new Separator());

        // Timer commands.
        foreach(MenuItem timerCommandMenuItem in _timerWindow.Commands)
        {
            Items.Add(timerCommandMenuItem);
        }

        Items.Add(new Separator());

        _pauseAllMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuPauseAllMenuItem
        };
        _pauseAllMenuItem.Click += static delegate { TimerManager.PauseAll(); };
        Items.Add(_pauseAllMenuItem);

        _resumeAllMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuResumeAllMenuItem
        };
        _resumeAllMenuItem.Click += static delegate { TimerManager.ResumeAll(); };
        Items.Add(_resumeAllMenuItem);

        _pauseResumeAllSeparator = new Separator();

        Items.Add(_pauseResumeAllSeparator);

        // Always on top
        _alwaysOnTopMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuAlwaysOnTopMenuItem
        };
        _alwaysOnTopMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_alwaysOnTopMenuItem);

        // Full screen
        _fullScreenMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuFullScreenMenuItem,
            InputGestureText = TimerWindow.FullScreenKeyGesture.ToInputGestureText()
        };
        _fullScreenMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_fullScreenMenuItem);

        // Window title
        MenuItem windowTitleMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuWindowTitleMenuItem
        };

        // No window title
        MenuItem noWindowTitleMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuNoWindowTitleMenuItem,
            Tag = WindowTitleMode.None
        };
        noWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        noWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(noWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(noWindowTitleMenuItem);

        windowTitleMenuItem.Items.Add(new Separator());

        // Application name (window title)
        var applicationNameWindowTitleMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuApplicationNameWindowTitleMenuItem,
            Tag = WindowTitleMode.ApplicationName
        };
        applicationNameWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        applicationNameWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(applicationNameWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(applicationNameWindowTitleMenuItem);

        windowTitleMenuItem.Items.Add(new Separator());

        // Time left (window title)
        var timeLeftWindowTitleMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuTimeLeftWindowTitleMenuItem,
            Tag = WindowTitleMode.TimeLeft
        };
        timeLeftWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timeLeftWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timeLeftWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timeLeftWindowTitleMenuItem);

        // Time elapsed (window title)
        var timeElapsedWindowTitleMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuElapsedTimeWindowTitleMenuItem,
            Tag = WindowTitleMode.TimeElapsed
        };
        timeElapsedWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timeElapsedWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timeElapsedWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timeElapsedWindowTitleMenuItem);

        // Timer title (window title)
        var timerTitleWindowTitleMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuTimerTitleWindowTitleMenuItem,
            Tag = WindowTitleMode.TimerTitle
        };
        timerTitleWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timerTitleWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timerTitleWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timerTitleWindowTitleMenuItem);

        windowTitleMenuItem.Items.Add(new Separator());

        // Time left + timer title (window title)
        var timeLeftPlusTimerTitleWindowTitleMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuTimeLeftPlusTimerTitleWindowTitleMenuItem,
            Tag = WindowTitleMode.TimeLeftPlusTimerTitle
        };
        timeLeftPlusTimerTitleWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timeLeftPlusTimerTitleWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timeLeftPlusTimerTitleWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timeLeftPlusTimerTitleWindowTitleMenuItem);

        // Time elapsed + timer title (window title)
        var timeElapsedPlusTimerTitleWindowTitleMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuElapsedTimePlusTimerTitleWindowTitleMenuItem,
            Tag = WindowTitleMode.TimeElapsedPlusTimerTitle
        };
        timeElapsedPlusTimerTitleWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timeElapsedPlusTimerTitleWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timeElapsedPlusTimerTitleWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timeElapsedPlusTimerTitleWindowTitleMenuItem);

        windowTitleMenuItem.Items.Add(new Separator());

        // Timer title + time left (window title)
        var timerTitlePlusTimeLeftWindowTitleMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuTimerTitlePlusTimeLeftWindowTitleMenuItem,
            Tag = WindowTitleMode.TimerTitlePlusTimeLeft
        };
        timerTitlePlusTimeLeftWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timerTitlePlusTimeLeftWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timerTitlePlusTimeLeftWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timerTitlePlusTimeLeftWindowTitleMenuItem);

        // Timer title + time elapsed (window title)
        var timerTitlePlusTimeElapsedWindowTitleMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuTimerTitlePlusElapsedTimeWindowTitleMenuItem,
            Tag = WindowTitleMode.TimerTitlePlusTimeElapsed
        };
        timerTitlePlusTimeElapsedWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timerTitlePlusTimeElapsedWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timerTitlePlusTimeElapsedWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timerTitlePlusTimeElapsedWindowTitleMenuItem);

        Items.Add(windowTitleMenuItem);

        Items.Add(new Separator());

        // Prompt on exit
        _promptOnExitMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuPromptOnCloseMenuItem
        };
        _promptOnExitMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_promptOnExitMenuItem);

        // Show progress in taskbar
        _showProgressInTaskbarMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuShowProgressInTaskbarMenuItem
        };
        _showProgressInTaskbarMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_showProgressInTaskbarMenuItem);

        // Show in notification area
        _showInNotificationAreaMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuShowInNotificationAreaMenuItem
        };
        _showInNotificationAreaMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_showInNotificationAreaMenuItem);

        Items.Add(new Separator());

        // Pop up when expired
        _popUpWhenExpiredMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuPopUpWhenExpiredMenuItem
        };
        _popUpWhenExpiredMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_popUpWhenExpiredMenuItem);

        // Close when expired
        _closeWhenExpiredMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuCloseWhenExpiredMenuItem
        };
        _closeWhenExpiredMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_closeWhenExpiredMenuItem);

        Items.Add(new Separator());

        // Loop timer
        _loopTimerMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuLoopTimerMenuItem
        };
        _loopTimerMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_loopTimerMenuItem);

        // Pause before looping
        _pauseBeforeLoopTimerMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuPauseAfterEachLoopTimerMenuItem
        };
        _pauseBeforeLoopTimerMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_pauseBeforeLoopTimerMenuItem);

        Items.Add(new Separator());

        // Recent inputs
        _recentInputsMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuRecentInputsMenuItem
        };
        Items.Add(_recentInputsMenuItem);

        // Saved timers
        _savedTimersMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuSavedTimersMenuItem
        };
        Items.Add(_savedTimersMenuItem);

        Items.Add(new Separator());

        // Theme
        _themeMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuThemeMenuItem
        };
        Items.Add(_themeMenuItem);

        // Sound
        _soundMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuSoundMenuItem
        };
        Items.Add(_soundMenuItem);

        Items.Add(new Separator());

        // Advanced options
        MenuItem advancedOptionsMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuAdvancedOptionsMenuItem
        };
        Items.Add(advancedOptionsMenuItem);

        // Display time in the digital clock format
        _digitalClockTimeMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuDigitalClockTimeMenuItem
        };
        _digitalClockTimeMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_digitalClockTimeMenuItem);

        // Show time elapsed
        _showTimeElapsedMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuShowElapsedTimeMenuItem
        };
        _showTimeElapsedMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_showTimeElapsedMenuItem);

        // Show trigger time
        _showTriggerTimeMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuShowTriggerTimeMenuItem
        };
        _showTriggerTimeMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_showTriggerTimeMenuItem);

        advancedOptionsMenuItem.Items.Add(new Separator());

        // Reverse progress bar
        _reverseProgressBarMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuReverseProgressBarMenuItem
        };
        _reverseProgressBarMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_reverseProgressBarMenuItem);

        _activateNextWindowMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuActivateNextWindowMenuItem
        };
        _activateNextWindowMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_activateNextWindowMenuItem);

        _orderByTitleFirstMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuOrderByTitleFirstMenuItem
        };
        _orderByTitleFirstMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_orderByTitleFirstMenuItem);

        advancedOptionsMenuItem.Items.Add(new Separator());

        // Open saved timers on startup
        _openSavedTimersOnStartupMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuOpenSavedTimersOnStartupMenuItem
        };
        _openSavedTimersOnStartupMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_openSavedTimersOnStartupMenuItem);

        // Prefer 24-hour time when parsing
        _prefer24HourTimeMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuPrefer24HourTimeMenuItem
        };
        _prefer24HourTimeMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_prefer24HourTimeMenuItem);

        advancedOptionsMenuItem.Items.Add(new Separator());

        // Do not keep computer awake
        _doNotKeepComputerAwakeMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuDoNotKeepComputerAwakeMenuItem
        };
        _doNotKeepComputerAwakeMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_doNotKeepComputerAwakeMenuItem);

        // Shut down when expired
        _shutDownWhenExpiredMenuItem = new CheckableMenuItem
        {
            Header = Properties.Resources.ContextMenuShutDownWhenExpiredMenuItem
        };
        _shutDownWhenExpiredMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_shutDownWhenExpiredMenuItem);

        Items.Add(new Separator());

        // About
        MenuItem aboutMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuAboutMenuItem
        };
        aboutMenuItem.Click += AboutMenuItemClick;
        Items.Add(aboutMenuItem);

        Items.Add(new Separator());

        // Reset
        MenuItem resetMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuResetPositionAndSizeMenuItem
        };
        resetMenuItem.Click += ResetMenuItemClick;
        Items.Add(resetMenuItem);

        // Restore
        _restoreMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuRestoreMenuItem
        };
        _restoreMenuItem.Click += RestoreMenuItemClick;
        Items.Add(_restoreMenuItem);

        // Minimize
        _minimizeMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuMinimizeMenuItem
        };
        _minimizeMenuItem.Click += MinimizeMenuItemClick;
        Items.Add(_minimizeMenuItem);

        // Maximize
        _maximizeMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuMaximizeMenuItem
        };
        _maximizeMenuItem.Click += MaximizeMenuItemClick;
        Items.Add(_maximizeMenuItem);

        Items.Add(new Separator());

        // Close
        MenuItem closeMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuCloseMenuItem
        };
        closeMenuItem.Click += CloseMenuItemClick;
        Items.Add(closeMenuItem);
    }

    /// <summary>
    /// Invoked when the "New timer" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void NewTimerMenuItemClick(object sender, RoutedEventArgs e)
    {
        _timerWindow.New();
    }

    #region Private Methods (Recent Inputs)

    /// <summary>
    /// Updates the <see cref="_recentInputsMenuItem"/>.
    /// </summary>
    private void UpdateRecentInputsMenuItem()
    {
        _recentInputsMenuItem.Items.Clear();

        IReadOnlyCollection<TimerStart> timerStarts = TimerStartManager.Instance.TimerStarts;

        _recentInputsMenuItem.IsEnabled = timerStarts.Any();
        if (!_recentInputsMenuItem.IsEnabled)
        {
            return;
        }

        foreach (var timerStart in timerStarts)
        {
            MenuItem timerMenuItem = new()
            {
                Header = MakeFirstCharHotkey(timerStart.OriginalInput),
                Tag = timerStart
            };
            timerMenuItem.Click += RecentInputMenuItemClick;

            _recentInputsMenuItem.Items.Add(timerMenuItem);
        }

        _recentInputsMenuItem.Items.Add(new Separator());

        if (_clearRecentInputsMenuItem is null)
        {
            _clearRecentInputsMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuClearRecentInputsMenuItem
            };
            _clearRecentInputsMenuItem.Click += ClearRecentInputsMenuItemClick;
        }

        _recentInputsMenuItem.Items.Add(_clearRecentInputsMenuItem);
    }

    /// <summary>
    /// Invoked when a recent <see cref="TimerStart"/> <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void RecentInputMenuItemClick(object sender, RoutedEventArgs e)
    {
        var menuItem = (MenuItem)sender;
        var timerStart = (TimerStart)menuItem.Tag;

        TimerWindow window;
        if (_timerWindow.Timer.State is TimerState.Stopped or TimerState.Expired)
        {
            window = _timerWindow;
        }
        else
        {
            window = new();
            window.Options.Set(_timerWindow.Options);
            window.RestoreFromWindow(_timerWindow);
        }

        window.Show(timerStart);
    }

    /// <summary>
    /// Invoked when the "Clear recent inputs" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private static void ClearRecentInputsMenuItemClick(object sender, RoutedEventArgs e)
    {
        TimerStartManager.Instance.Clear();
    }

    #endregion

    #region Private Methods (Saved Timers)

    /// <summary>
    /// Updates the <see cref="_savedTimersMenuItem"/>.
    /// </summary>
    private void UpdateSavedTimersMenuItem()
    {
        _savedTimersMenuItem.Items.Clear();

        IReadOnlyCollection<Timer> resumableTimers = TimerManager.Instance.ResumableTimers;

        _savedTimersMenuItem.IsEnabled = resumableTimers.Any();
        if (!_savedTimersMenuItem.IsEnabled)
        {
            return;
        }

        foreach (var timer in resumableTimers)
        {
            timer.Update();

            MenuItem timerMenuItem = new()
            {
                Header = GetHeaderForTimer(timer),
                Icon = GetIconForTimer(timer),
                Tag = timer
            };
            timerMenuItem.Click += SavedTimerMenuItemClick;

            _savedTimersMenuItem.Items.Add(timerMenuItem);
        }

        _savedTimersMenuItem.Items.Add(new Separator());

        if (_openAllSavedTimersMenuItem is null)
        {
            _openAllSavedTimersMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuOpenAllSavedTimersMenuItem
            };
            _openAllSavedTimersMenuItem.Click += OpenAllSavedTimersMenuItemClick;
        }

        _savedTimersMenuItem.Items.Add(_openAllSavedTimersMenuItem);

        if (_clearSavedTimersMenuItem is null)
        {
            _clearSavedTimersMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuClearSavedTimersMenuItem
            };
            _clearSavedTimersMenuItem.Click += ClearSavedTimersMenuItemClick;
        }

        _savedTimersMenuItem.Items.Add(_clearSavedTimersMenuItem);
    }

    /// <summary>
    /// Updates the <see cref="MenuItem.Header"/> of the items in the <see cref="_savedTimersMenuItem"/>.
    /// </summary>
    private void UpdateSavedTimersHeaders()
    {
        foreach (var menuItem in _savedTimersMenuItem.Items.OfType<MenuItem>())
        {
            if (menuItem.Tag is Timer timer)
            {
                menuItem.Header = GetHeaderForTimer(timer);
                menuItem.Icon = GetIconForTimer(timer);
            }
        }
    }

    /// <summary>
    /// Returns an object that can be set for the <see cref="MenuItem.Header"/> of a <see cref="MenuItem"/> that displays a <see cref="Timer"/>.
    /// </summary>
    /// <param name="timer">A <see cref="Timer"/>.</param>
    /// <returns>Timer string representation.</returns>
    private static string GetHeaderForTimer(Timer timer)
    {
        return MakeFirstCharHotkey(timer.ToString());
    }

    /// <summary>
    /// Returns an object that can be set for the <see cref="MenuItem.Icon"/> of a <see cref="MenuItem"/> that
    /// displays a <see cref="Timer"/>.
    /// </summary>
    /// <param name="timer">A <see cref="Timer"/>.</param>
    /// <returns>An object that can be set for the <see cref="MenuItem.Icon"/>.</returns>
    private static object GetIconForTimer(Timer timer)
    {
        Border outerBorder = new()
        {
            BorderBrush = new SolidColorBrush(Colors.LightGray),
            BorderThickness = new(1),
            CornerRadius = new(2),
            Width = 16,
            Height = 6
        };

        if (timer.State == TimerState.Expired)
        {
            Border progress = new()
            {
                Background = new SolidColorBrush(Color.FromRgb(199, 80, 80)),
                Width = 16,
                Height = 6
            };

            outerBorder.Child = progress;
        }
        else if (!timer.Options.ReverseProgressBar && timer.TimeLeftAsPercentage.HasValue)
        {
            Border progress = new()
            {
                Background = timer.Options.Theme?.ProgressBarBrush,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = MathExtensions.LimitToRange(timer.TimeLeftAsPercentage.Value / 100.0 * 16.0, 0.0, 16.0),
                Height = 6
            };

            outerBorder.Child = progress;
        }
        else if (timer.Options.ReverseProgressBar && timer.TimeElapsedAsPercentage.HasValue)
        {
            Border progress = new()
            {
                Background = timer.Options.Theme?.ProgressBarBrush,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = MathExtensions.LimitToRange(timer.TimeElapsedAsPercentage.Value / 100.0 * 16.0, 0.0, 16.0),
                Height = 6
            };

            outerBorder.Child = progress;
        }

        return outerBorder;
    }

    /// <summary>
    /// Invoked when a saved timer <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void SavedTimerMenuItemClick(object sender, RoutedEventArgs e)
    {
        var menuItem = (MenuItem)sender;
        var savedTimer = (Timer)menuItem.Tag;
        ShowSavedTimer(savedTimer);
    }

    /// <summary>
    /// Invoked when the "Open all saved timers" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void OpenAllSavedTimersMenuItemClick(object sender, RoutedEventArgs e)
    {
        foreach (var savedTimer in TimerManager.Instance.ResumableTimers)
        {
            ShowSavedTimer(savedTimer);
        }
    }

    /// <summary>
    /// Shows an existing <see cref="Timer"/>.
    /// </summary>
    /// <param name="savedTimer">An existing <see cref="Timer"/>.</param>
    private void ShowSavedTimer(Timer savedTimer)
    {
        if (_timerWindow.Timer.State is TimerState.Stopped or TimerState.Expired)
        {
            ShowSavedTimerInCurrentWindow(savedTimer);
        }
        else
        {
            ShowSavedTimerInNewWindow(savedTimer);
        }
    }

    /// <summary>
    /// Shows an existing <see cref="Timer"/> in the current <see cref="TimerWindow"/>.
    /// </summary>
    /// <param name="savedTimer">An existing <see cref="Timer"/>.</param>
    private void ShowSavedTimerInCurrentWindow(Timer savedTimer)
    {
        if (savedTimer.Options.WindowSize is not null)
        {
            _timerWindow.Restore(savedTimer.Options.WindowSize);
        }

        _timerWindow.Show(savedTimer);
        UpdateMenuFromOptions();
    }

    /// <summary>
    /// Shows an existing <see cref="Timer"/> in a new <see cref="TimerWindow"/>.
    /// </summary>
    /// <param name="savedTimer">An existing <see cref="Timer"/>.</param>
    private void ShowSavedTimerInNewWindow(Timer savedTimer)
    {
        TimerWindow newTimerWindow = new();

        if (savedTimer.Options.WindowSize is not null)
        {
            newTimerWindow.Restore(savedTimer.Options.WindowSize);
        }
        else
        {
            newTimerWindow.RestoreFromWindow(_timerWindow);
        }

        newTimerWindow.Show(savedTimer);
    }

    /// <summary>
    /// Invoked when the "Clear saved timers" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private static void ClearSavedTimersMenuItemClick(object sender, RoutedEventArgs e)
    {
        TimerManager.Instance.ClearResumableTimers();
    }

    #endregion

    #region Private Methods (Theme)

    /// <summary>
    /// Updates the <see cref="_themeMenuItem"/>.
    /// </summary>
    private void UpdateThemeMenuItem()
    {
        _themeMenuItem.Items.Clear();
        _selectableThemeMenuItems.Clear();

        // Switch between light and dark themes
        if (_lightThemeMenuItem is null)
        {
            _lightThemeMenuItem = new CheckableMenuItem
            {
                Header = Properties.Resources.ContextMenuLightThemeMenuItem,
                Tag = ThemeType.BuiltInLight
            };
            _lightThemeMenuItem.Click += ThemeTypeMenuItemClick;
        }

        _themeMenuItem.Items.Add(_lightThemeMenuItem);

        if (_darkThemeMenuItem is null)
        {
            _darkThemeMenuItem = new CheckableMenuItem
            {
                Header = Properties.Resources.ContextMenuDarkThemeMenuItem,
                Tag = ThemeType.BuiltInDark
            };
            _darkThemeMenuItem.Click += ThemeTypeMenuItemClick;
        }

        _themeMenuItem.Items.Add(_darkThemeMenuItem);

        // Built-in themes
        CreateThemeMenuItemsFromList(ThemeManager.Instance.BuiltInThemes);

        // User-provided themes
        if (ThemeManager.Instance.UserProvidedThemes.Count > 0)
        {
            CreateThemeMenuItemsFromList(ThemeManager.Instance.UserProvidedThemes);
        }

        // Manage themes
        _themeMenuItem.Items.Add(new Separator());

        if (_manageThemesMenuItem is null)
        {
            _manageThemesMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuManageThemesMenuItem
            };
            _manageThemesMenuItem.Click += ManageThemesMenuItemClick;
        }

        _themeMenuItem.Items.Add(_manageThemesMenuItem);
    }

    /// <summary>
    /// Creates a <see cref="MenuItem"/> for each <see cref="Theme"/> in the collection.
    /// </summary>
    /// <param name="themes">A collection of <see cref="Theme"/>s.</param>
    private void CreateThemeMenuItemsFromList(IReadOnlyCollection<Theme> themes)
    {
        _themeMenuItem.Items.Add(new Separator());

        foreach (var theme in themes)
        {
            CreateThemeMenuItem(theme);
        }
    }

    /// <summary>
    /// Creates a <see cref="MenuItem"/> for a <see cref="Theme"/>.
    /// </summary>
    /// <param name="theme">A <see cref="Theme"/>.</param>
    private void CreateThemeMenuItem(Theme theme)
    {
        var menuItem = new CheckableMenuItem
        {
            Header = GetHeaderForTheme(theme),
            Tag = theme
        };
        menuItem.Click += ThemeMenuItemClick;
        menuItem.Click += CheckableMenuItemClick;

        _themeMenuItem.Items.Add(menuItem);
        _selectableThemeMenuItems.Add(menuItem);
    }

    /// <summary>
    /// Returns an object that can be set for the <see cref="MenuItem.Header"/> of a <see cref="MenuItem"/> that
    /// displays a <see cref="Theme"/>.
    /// </summary>
    /// <param name="theme">A <see cref="Theme"/>.</param>
    /// <returns>An object that can be set for the <see cref="MenuItem.Header"/>.</returns>
    private static object GetHeaderForTheme(Theme theme)
    {
        Border border = new()
        {
            Background = theme.ProgressBarBrush,
            CornerRadius = new(2),
            Width = 8,
            Height = 8
        };

        Label label = new()
        {
            Content = MakeFirstCharHotkey($"{(string.IsNullOrWhiteSpace(theme.Name) ? Properties.Resources.ContextMenuUnnamedTheme : theme.Name)}"),
            Margin = new(0)
        };

        StackPanel stackPanel = new()
        {
            Orientation = Orientation.Horizontal
        };
        stackPanel.Children.Add(border);
        stackPanel.Children.Add(label);

        return stackPanel;
    }

    /// <summary>
    /// Update themes menu items.
    /// </summary>
    /// <param name="updateBuiltInThemes">Indicates whether built-in themes menu items should be updated.</param>
    private void UpdateThemesMenuItems(bool updateBuiltInThemes = true)
    {
        var themeType = _timerWindow.Options.Theme?.Type;

        if (updateBuiltInThemes)
        {
            foreach (var menuItem in _selectableThemeMenuItems)
            {
                var menuItemTheme = (Theme)menuItem.Tag;
                menuItem.IsChecked = menuItemTheme == _timerWindow.Options.Theme;
                menuItem.Visibility = (
                    themeType == ThemeType.UserProvided
                        ? _timerWindow.Options.Theme?.IsUserThemeDark == true
                            ? menuItemTheme.Type is ThemeType.BuiltInDark  or ThemeType.UserProvided
                            : menuItemTheme.Type is ThemeType.BuiltInLight or ThemeType.UserProvided
                        : menuItemTheme.Type == themeType || menuItemTheme.Type == ThemeType.UserProvided
                ).ToVisibility();
            }
        }

        _lightThemeMenuItem!.IsChecked = themeType == ThemeType.BuiltInLight;
        _darkThemeMenuItem!.IsChecked  = themeType == ThemeType.BuiltInDark;
    }

    /// <summary>
    /// Invoked when a theme type <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void ThemeTypeMenuItemClick(object sender, RoutedEventArgs e)
    {
        var clickedMenuItem = (MenuItem)sender;
        var type = (ThemeType)clickedMenuItem.Tag;

        _timerWindow.Options.Theme = type == ThemeType.BuiltInDark
            ? _timerWindow.Options.Theme?.DarkVariant
            : _timerWindow.Options.Theme?.LightVariant;

        UpdateThemesMenuItems();
    }

    /// <summary>
    /// Invoked when a theme <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void ThemeMenuItemClick(object sender, RoutedEventArgs e)
    {
        foreach (var menuItem in _selectableThemeMenuItems)
        {
            menuItem.IsChecked = ReferenceEquals(menuItem, sender);
        }

        var selectedMenuItem = (MenuItem)sender;
        _timerWindow.Options.Theme = (Theme)selectedMenuItem.Tag;

        UpdateThemesMenuItems(false);
    }

    /// <summary>
    /// Invoked when the "Manage themes" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void ManageThemesMenuItemClick(object sender, RoutedEventArgs e)
    {
        var themeManagerWindow = Application.Current.Windows.OfType<ThemeManagerWindow>().FirstOrDefault();
        if (themeManagerWindow is null)
        {
            themeManagerWindow = new(_timerWindow);
            themeManagerWindow.Show();
        }
        else
        {
            themeManagerWindow.SetTimerWindow(_timerWindow);
            themeManagerWindow.BringToFrontAndActivate();
        }
    }

    /// <summary>
    /// Makes first character a hotkey.
    /// </summary>
    /// <param name="text">Text.</param>
    /// <returns>Text where fist character is a hotkey.</returns>
    private static string MakeFirstCharHotkey(string text)
    {
        const char hotkeyChar = '_';

        return string.IsNullOrWhiteSpace(text) || text[0] == hotkeyChar
            ? text
            : $"{hotkeyChar}{text}";
    }

    #endregion

    #region Private Methods (Sound)

    /// <summary>
    /// Updates the <see cref="_soundMenuItem"/>.
    /// </summary>
    private void UpdateSoundMenuItem()
    {
        _soundMenuItem.Items.Clear();
        _selectableSoundMenuItems.Clear();

        // Sounds
        CreateSoundMenuItem(Sound.NoSound);
        CreateSoundMenuItemsFromList(SoundManager.Instance.BuiltInSounds);
        CreateSoundMenuItemsFromList(SoundManager.Instance.UserProvidedSounds);

        // Options
        _soundMenuItem.Items.Add(new Separator());

        if (_loopSoundMenuItem is null)
        {
            _loopSoundMenuItem = new CheckableMenuItem
            {
                Header = Properties.Resources.ContextMenuLoopSoundMenuItem
            };
            _loopSoundMenuItem.Click += CheckableMenuItemClick;
        }

        _soundMenuItem.Items.Add(_loopSoundMenuItem);
    }

    /// <summary>
    /// Creates a <see cref="MenuItem"/> for a <see cref="Sound"/>.
    /// </summary>
    /// <param name="sound">A <see cref="Sound"/>.</param>
    private void CreateSoundMenuItem(Sound? sound)
    {
        var menuItem = new CheckableMenuItem
        {
            Header = sound is not null ? sound.Name : Properties.Resources.ContextMenuNoSoundMenuItem,
            Tag = sound
        };
        menuItem.Click += SoundMenuItemClick;
        menuItem.Click += CheckableMenuItemClick;

        _soundMenuItem.Items.Add(menuItem);
        _selectableSoundMenuItems.Add(menuItem);
    }

    /// <summary>
    /// Creates a <see cref="MenuItem"/> for each <see cref="Sound"/> in the collection.
    /// </summary>
    /// <param name="sounds">A collection of <see cref="Sound"/>s.</param>
    private void CreateSoundMenuItemsFromList(IReadOnlyCollection<Sound> sounds)
    {
        if (sounds.Count <= 0)
        {
            return;
        }

        _soundMenuItem.Items.Add(new Separator());
        foreach (var sound in sounds)
        {
            CreateSoundMenuItem(sound);
        }
    }

    /// <summary>
    /// Invoked when a sound <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void SoundMenuItemClick(object sender, RoutedEventArgs e)
    {
        foreach (var menuItem in _selectableSoundMenuItems)
        {
            menuItem.IsChecked = ReferenceEquals(menuItem, sender);
        }
    }

    #endregion

    /// <summary>
    /// Invoked when a window title <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void WindowTitleMenuItemClick(object sender, RoutedEventArgs e)
    {
        foreach (var menuItem in _selectableWindowTitleMenuItems)
        {
            menuItem.IsChecked = ReferenceEquals(menuItem, sender);
        }
    }

    #region Private Methods (Window State)

    /// <summary>
    /// Updates the <see cref="_restoreMenuItem"/>, <see cref="_minimizeMenuItem"/>, and
    /// <see cref="_maximizeMenuItem"/>.
    /// </summary>
    private void UpdateWindowStateMenuItems()
    {
        _restoreMenuItem.IsEnabled = _timerWindow.WindowState != WindowState.Normal;
        _minimizeMenuItem.IsEnabled = _timerWindow.WindowState != WindowState.Minimized;
        _maximizeMenuItem.IsEnabled = _timerWindow.WindowState != WindowState.Maximized;
    }

    /// <summary>
    /// Invoked when the "About" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private static void AboutMenuItemClick(object sender, RoutedEventArgs e)
    {
        AboutDialog.ShowOrActivate();
    }

    /// <summary>
    /// Invoked when the "Restore" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void RestoreMenuItemClick(object sender, RoutedEventArgs e)
    {
        ExitFullScreen();
    }

    /// <summary>
    /// Invoked when the "Minimize" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void MinimizeMenuItemClick(object sender, RoutedEventArgs e)
    {
        _timerWindow.WindowState = WindowState.Minimized;
    }

    private void ResetMenuItemClick(object sender, RoutedEventArgs e)
    {
        ExitFullScreen();

        _timerWindow.Width = InterfaceScaler.BaseWindowWidth;
        _timerWindow.Height = InterfaceScaler.BaseWindowHeight;

        _timerWindow.CenterOnScreen();
    }

    /// <summary>
    /// Invoked when the "Maximize" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void MaximizeMenuItemClick(object sender, RoutedEventArgs e)
    {
        _timerWindow.WindowState = WindowState.Maximized;
    }

    #endregion

    /// <summary>
    /// Invoked when the "Close" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void CloseMenuItemClick(object sender, RoutedEventArgs e)
    {
        _timerWindow.Close();
    }

    private void ExitFullScreen()
    {
        if (!_timerWindow.IsFullScreen)
        {
            _timerWindow.WindowState = WindowState.Normal;
        }

        // Must set the menu item value here, since it will sync to the TimerWindow on menu close.
        _fullScreenMenuItem.IsChecked = false;
        _timerWindow.IsFullScreen = false;
    }

    private sealed class CheckableMenuItem : MenuItem
    {
        public CheckableMenuItem() =>
            (IsCheckable, StaysOpenOnClick) = (true, true);
    }
}

