// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerOptions.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Timing;

using System;
using System.ComponentModel;
using System.Windows;

using Extensions;
using Serialization;
using Windows;

/// <summary>
/// Modes indicating what information to display in the timer window title.
/// </summary>
public enum WindowTitleMode
{
    /// <summary>
    /// Hides the timer window title bar.
    /// </summary>
    None,

    /// <summary>
    /// The timer window title is set to show the application name.
    /// </summary>
    ApplicationName,

    /// <summary>
    /// The timer window title is set to show the time left.
    /// </summary>
    TimeLeft,

    /// <summary>
    /// The timer window title is set to show the time elapsed.
    /// </summary>
    TimeElapsed,

    /// <summary>
    /// The timer window title is set to show the timer title.
    /// </summary>
    TimerTitle,

    /// <summary>
    /// The timer window title is set to show the time left then the timer title.
    /// </summary>
    TimeLeftPlusTimerTitle,

    /// <summary>
    /// The timer window title is set to show the time elapsed then the timer title.
    /// </summary>
    TimeElapsedPlusTimerTitle,

    /// <summary>
    /// The timer window title is set to show the timer title then the time left.
    /// </summary>
    TimerTitlePlusTimeLeft,

    /// <summary>
    /// The timer window title is set to show the timer title then the time elapsed.
    /// </summary>
    TimerTitlePlusTimeElapsed
}

/// <summary>
/// Configuration data for a timer.
/// </summary>
public sealed class TimerOptions : INotifyPropertyChanged
{
    #region Private Members

    /// <summary>
    /// A user-specified title for the timer.
    /// </summary>
    private string? _title;

    /// <summary>
    /// A value indicating whether the timer window should always be displayed on top of other windows.
    /// </summary>
    private bool _alwaysOnTop;

    /// <summary>
    /// A value indicating whether to prompt the user before closing the timer window if the timer is running.
    /// </summary>
    private bool _promptOnExit;

    /// <summary>
    /// A value indicating whether to show progress in the taskbar.
    /// </summary>
    private bool _showProgressInTaskbar;

    /// <summary>
    /// A value indicating whether to keep the computer awake while the timer is running.
    /// </summary>
    private bool _doNotKeepComputerAwake;

    /// <summary>
    /// A value indicating whether to reverse the progress bar (count backwards).
    /// </summary>
    private bool _reverseProgressBar;

    /// <summary>
    /// A value indicating whether to show the time elapsed rather than the time left.
    /// </summary>
    private bool _showTimeElapsed;

    /// <summary>
    /// A value indicating whether to show the trigger time.
    /// </summary>
    private bool _showTriggerTime;

    /// <summary>
    /// A value indicating whether to loop the timer continuously.
    /// </summary>
    private bool _loopTimer;

    /// <summary>
    /// A value indicating whether to pause before loop the timer continuously.
    /// </summary>
    private bool _pauseBeforeLoopTimer;

    /// <summary>
    /// A value indicating whether the timer window should be brought to the top of other windows when the timer
    /// expires.
    /// </summary>
    private bool _popUpWhenExpired;

    /// <summary>
    /// A value indicating whether the timer window should be closed when the timer expires.
    /// </summary>
    private bool _closeWhenExpired;

    /// <summary>
    /// A value indicating whether Windows should be shut down when the timer expires.
    /// </summary>
    private bool _shutDownWhenExpired;

    /// <summary>
    /// The sound to play when the timer expires, or <c>null</c> if no sound is to be played.
    /// </summary>
    private Sound? _sound;

    /// <summary>
    /// A value indicating whether the sound that plays when the timer expires should be looped until stopped by
    /// the user.
    /// </summary>
    private bool _loopSound;

    /// <summary>
    /// The theme of the timer window.
    /// </summary>
    private Theme? _theme;

    /// <summary>
    /// A value indicating what information to display in the timer window title.
    /// </summary>
    private WindowTitleMode _windowTitleMode;

    /// <summary>
    /// The size, position, and state of the timer window.
    /// </summary>
    private WindowSize? _windowSize;

    /// <summary>
    /// A value indicating whether the user interface should be locked, preventing the user from taking any action
    /// until the timer expires.
    /// </summary>
    private bool _lockInterface;

    /// <summary>
    /// A value indicating whether to display time in the digital clock format.
    /// </summary>
    private bool _digitalClockTime;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerOptions"/> class.
    /// </summary>
    public TimerOptions()
    {
        _title = string.Empty;
        _alwaysOnTop = false;
        _promptOnExit = true;
        _showProgressInTaskbar = true;
        _doNotKeepComputerAwake = false;
        _reverseProgressBar = false;
        _digitalClockTime = false;
        _showTimeElapsed = false;
        _showTriggerTime = false;
        _loopTimer = false;
        _pauseBeforeLoopTimer = false;
        _popUpWhenExpired = true;
        _closeWhenExpired = false;
        _shutDownWhenExpired = false;
        _theme = Theme.DefaultTheme;
        _sound = Sound.DefaultSound;
        _loopSound = false;
        _windowTitleMode = WindowTitleMode.ApplicationName;
        _windowSize = new(
            new(double.PositiveInfinity, double.PositiveInfinity, InterfaceScaler.BaseWindowWidth, InterfaceScaler.BaseWindowHeight),
            WindowState.Normal,
            WindowState.Normal,
            false /* isFullScreen */);
        _lockInterface = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerOptions"/> class from another instance of the <see
    /// cref="TimerOptions"/> class.
    /// </summary>
    /// <param name="options">A <see cref="TimerOptions"/>.</param>
    public TimerOptions(TimerOptions options)
    {
        Set(options);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerOptions"/> class from a <see cref="TimerOptionsInfo"/>.
    /// </summary>
    /// <param name="info">A <see cref="TimerOptionsInfo"/>.</param>
    public TimerOptions(TimerOptionsInfo info)
    {
        Set(info);
    }

    #endregion

    /// <summary>
    /// Raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #region Properties

    /// <summary>
    /// Gets or sets a user-specified title for the timer.
    /// </summary>
    public string? Title
    {
        get => _title;

        set
        {
            if (_title == value)
            {
                return;
            }

            _title = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the timer window should always be displayed on top of other windows.
    /// </summary>
    public bool AlwaysOnTop
    {
        get => _alwaysOnTop;

        set
        {
            if (_alwaysOnTop == value)
            {
                return;
            }

            _alwaysOnTop = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to prompt the user before closing the timer window if the timer is
    /// running.
    /// </summary>
    public bool PromptOnExit
    {
        get => _promptOnExit;

        set
        {
            if (_promptOnExit == value)
            {
                return;
            }

            _promptOnExit = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show progress in the taskbar.
    /// </summary>
    public bool ShowProgressInTaskbar
    {
        get => _showProgressInTaskbar;

        set
        {
            if (_showProgressInTaskbar == value)
            {
                return;
            }

            _showProgressInTaskbar = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to keep the computer awake while the timer is running.
    /// </summary>
    public bool DoNotKeepComputerAwake
    {
        get => _doNotKeepComputerAwake;

        set
        {
            if (_doNotKeepComputerAwake == value)
            {
                return;
            }

            _doNotKeepComputerAwake = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to reverse the progress bar (count backwards).
    /// </summary>
    public bool ReverseProgressBar
    {
        get => _reverseProgressBar;

        set
        {
            if (_reverseProgressBar == value)
            {
                return;
            }

            _reverseProgressBar = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to display time in the digital clock format.
    /// </summary>
    public bool DigitalClockTime
    {
        get => _digitalClockTime;

        set
        {
            if (_digitalClockTime == value)
            {
                return;
            }

            _digitalClockTime = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the time elapsed rather than the time left.
    /// </summary>
    public bool ShowTimeElapsed
    {
        get => _showTimeElapsed;

        set
        {
            if (_showTimeElapsed == value)
            {
                return;
            }

            _showTimeElapsed = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the trigger time.
    /// </summary>
    public bool ShowTriggerTime
    {
        get => _showTriggerTime;

        set
        {
            if (_showTriggerTime == value)
            {
                return;
            }

            _showTriggerTime = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to loop the timer continuously.
    /// </summary>
    public bool LoopTimer
    {
        get => _loopTimer;

        set
        {
            if (_loopTimer == value)
            {
                return;
            }

            _loopTimer = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to pause before loop the timer continuously.
    /// </summary>
    public bool PauseBeforeLoopTimer
    {
        get => _pauseBeforeLoopTimer;

        set
        {
            if (_pauseBeforeLoopTimer == value)
            {
                return;
            }

            _pauseBeforeLoopTimer = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the timer window should be brought to the top of other windows when
    /// the timer expires.
    /// </summary>
    public bool PopUpWhenExpired
    {
        get => _popUpWhenExpired;

        set
        {
            if (_popUpWhenExpired == value)
            {
                return;
            }

            _popUpWhenExpired = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the timer window should be closed when the timer expires.
    /// </summary>
    public bool CloseWhenExpired
    {
        get => _closeWhenExpired;

        set
        {
            if (_closeWhenExpired == value)
            {
                return;
            }

            _closeWhenExpired = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether Windows should be shut down when the timer expires.
    /// </summary>
    public bool ShutDownWhenExpired
    {
        get => _shutDownWhenExpired;

        set
        {
            if (_shutDownWhenExpired == value)
            {
                return;
            }

            _shutDownWhenExpired = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets the theme of the timer window.
    /// </summary>
    public Theme? Theme
    {
        get => _theme;

        set
        {
            if (ReferenceEquals(_theme, value))
            {
                return;
            }

            _theme = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets the sound to play when the timer expires, or <c>null</c> if no sound is to be played.
    /// </summary>
    public Sound? Sound
    {
        get => _sound;

        set
        {
            if (_sound == value)
            {
                return;
            }

            _sound = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the sound that plays when the timer expires should be looped until
    /// stopped by the user.
    /// </summary>
    public bool LoopSound
    {
        get => _loopSound;

        set
        {
            if (_loopSound == value)
            {
                return;
            }

            _loopSound = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating what information to display in the timer window title.
    /// </summary>
    public WindowTitleMode WindowTitleMode
    {
        get => _windowTitleMode;

        set
        {
            if (_windowTitleMode == value)
            {
                return;
            }

            _windowTitleMode = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets the size, position, and state of the timer window.
    /// </summary>
    public WindowSize? WindowSize
    {
        get => _windowSize;

        set
        {
            if (_windowSize == value)
            {
                return;
            }

            _windowSize = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user interface should be locked, preventing the user from taking
    /// any action until the timer expires.
    /// </summary>
    public bool LockInterface
    {
        get => _lockInterface;

        set
        {
            if (_lockInterface == value)
            {
                return;
            }

            _lockInterface = value;
            PropertyChanged.Notify(this);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns a <see cref="TimerOptions"/> for the specified <see cref="TimerOptions"/>, or <c>null</c> if the
    /// specified <see cref="TimerOptions"/> is <c>null</c>.
    /// </summary>
    /// <param name="options">A <see cref="TimerOptions"/>.</param>
    /// <returns>A <see cref="TimerOptions"/> for the specified <see cref="TimerOptions"/>, or <c>null</c> if the
    /// specified <see cref="TimerOptions"/> is <c>null</c>.</returns>
    public static TimerOptions? FromTimerOptions(TimerOptions? options)
    {
        return options is not null ? new TimerOptions(options) : null;
    }

    /// <summary>
    /// Returns a <see cref="TimerOptions"/> for the specified <see cref="TimerOptionsInfo"/>, or <c>null</c> if
    /// the specified <see cref="TimerOptionsInfo"/> is <c>null</c>.
    /// </summary>
    /// <param name="info">A <see cref="TimerOptionsInfo"/>.</param>
    /// <returns>A <see cref="TimerOptions"/> for the specified <see cref="TimerOptionsInfo"/>, or <c>null</c> if
    /// the specified <see cref="TimerOptionsInfo"/> is <c>null</c>.</returns>
    public static TimerOptions? FromTimerOptionsInfo(TimerOptionsInfo? info)
    {
        return info is not null ? new TimerOptions(info) : null;
    }

    /// <summary>
    /// Sets all the options from another instance of the <see cref="TimerOptions"/> class.
    /// </summary>
    /// <param name="options">A <see cref="TimerOptions"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/></exception>
    public void Set(TimerOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _title = options._title;
        _alwaysOnTop = options._alwaysOnTop;
        _promptOnExit = options._promptOnExit;
        _showProgressInTaskbar = options._showProgressInTaskbar;
        _doNotKeepComputerAwake = options._doNotKeepComputerAwake;
        _reverseProgressBar = options._reverseProgressBar;
        _digitalClockTime = options._digitalClockTime;
        _showTimeElapsed = options._showTimeElapsed;
        _showTriggerTime = options._showTriggerTime;
        _loopTimer = options._loopTimer;
        _pauseBeforeLoopTimer = options._pauseBeforeLoopTimer;
        _popUpWhenExpired = options._popUpWhenExpired;
        _closeWhenExpired = options._closeWhenExpired;
        _shutDownWhenExpired = options._shutDownWhenExpired;
        _theme = options._theme;
        _sound = options._sound;
        _loopSound = options._loopSound;
        _windowTitleMode = options._windowTitleMode;
        _windowSize = WindowSize.FromWindowSize(options.WindowSize);
        _lockInterface = options._lockInterface;

        PropertyChanged.Notify(this,
            nameof(WindowTitleMode),
            nameof(WindowSize),
            nameof(Title),
            nameof(AlwaysOnTop),
            nameof(PromptOnExit),
            nameof(ShowProgressInTaskbar),
            nameof(DoNotKeepComputerAwake),
            nameof(ReverseProgressBar),
            nameof(DigitalClockTime),
            nameof(ShowTimeElapsed),
            nameof(LoopTimer),
            nameof(PauseBeforeLoopTimer),
            nameof(PopUpWhenExpired),
            nameof(CloseWhenExpired),
            nameof(ShutDownWhenExpired),
            nameof(Theme),
            nameof(Sound),
            nameof(LoopSound),
            nameof(LockInterface));
    }

    /// <summary>
    /// Sets all the options from an instance of the <see cref="TimerOptionsInfo"/> class.
    /// </summary>
    /// <param name="info">A <see cref="TimerOptionsInfo"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/></exception>
    private void Set(TimerOptionsInfo info)
    {
        if (info is null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        _title = info.Title;
        _alwaysOnTop = info.AlwaysOnTop;
        _promptOnExit = info.PromptOnExit;
        _showProgressInTaskbar = info.ShowProgressInTaskbar;
        _doNotKeepComputerAwake = info.DoNotKeepComputerAwake;
        _reverseProgressBar = info.ReverseProgressBar;
        _digitalClockTime = info.DigitalClockTime;
        _showTimeElapsed = info.ShowTimeElapsed;
        _showTriggerTime = info.ShowTriggerTime;
        _loopTimer = info.LoopTimer;
        _pauseBeforeLoopTimer = info.PauseBeforeLoopTimer;
        _popUpWhenExpired = info.PopUpWhenExpired;
        _closeWhenExpired = info.CloseWhenExpired;
        _shutDownWhenExpired = info.ShutDownWhenExpired;
        _theme = Theme.FromIdentifier(info.ThemeIdentifier);
        _sound = Sound.FromIdentifier(info.SoundIdentifier);
        _loopSound = info.LoopSound;
        _windowTitleMode = info.WindowTitleMode;
        _windowSize = WindowSize.FromWindowSizeInfo(info.WindowSize);
        _lockInterface = info.LockInterface;

        PropertyChanged.Notify(this,
            nameof(WindowTitleMode),
            nameof(WindowSize),
            nameof(Title),
            nameof(AlwaysOnTop),
            nameof(PromptOnExit),
            nameof(ShowProgressInTaskbar),
            nameof(DoNotKeepComputerAwake),
            nameof(ReverseProgressBar),
            nameof(DigitalClockTime),
            nameof(ShowTimeElapsed),
            nameof(LoopTimer),
            nameof(PauseBeforeLoopTimer),
            nameof(PopUpWhenExpired),
            nameof(CloseWhenExpired),
            nameof(ShutDownWhenExpired),
            nameof(Theme),
            nameof(Sound),
            nameof(LoopSound),
            nameof(LockInterface));
    }

    /// <summary>
    /// Returns the representation of the <see cref="TimerOptions"/> used for XML serialization.
    /// </summary>
    /// <returns>The representation of the <see cref="TimerOptions"/> used for XML serialization.</returns>
    public TimerOptionsInfo ToTimerOptionsInfo()
    {
        return new()
        {
            Title = _title,
            AlwaysOnTop = _alwaysOnTop,
            PromptOnExit = _promptOnExit,
            ShowProgressInTaskbar = _showProgressInTaskbar,
            DoNotKeepComputerAwake = _doNotKeepComputerAwake,
            ReverseProgressBar = _reverseProgressBar,
            DigitalClockTime = _digitalClockTime,
            ShowTimeElapsed = _showTimeElapsed,
            ShowTriggerTime = _showTriggerTime,
            LoopTimer = _loopTimer,
            PauseBeforeLoopTimer = _pauseBeforeLoopTimer,
            PopUpWhenExpired = _popUpWhenExpired,
            CloseWhenExpired = _closeWhenExpired,
            ShutDownWhenExpired = _shutDownWhenExpired,
            ThemeIdentifier = _theme?.Identifier,
            SoundIdentifier = _sound?.Identifier,
            LoopSound = _loopSound,
            WindowTitleMode = _windowTitleMode,
            WindowSize = WindowSizeInfo.FromWindowSize(_windowSize)!,
            LockInterface = _lockInterface
        };
    }

    #endregion
}