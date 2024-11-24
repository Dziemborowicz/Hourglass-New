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
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerOptions"/> class.
    /// </summary>
    public TimerOptions()
    {
        Title = string.Empty;
        AlwaysOnTop = false;
        PromptOnExit = true;
        ShowProgressInTaskbar = true;
        DoNotKeepComputerAwake = false;
        ReverseProgressBar = false;
        DigitalClockTime = false;
        ShowTimeElapsed = false;
        ShowTriggerTime = false;
        LoopTimer = false;
        PauseBeforeLoopTimer = false;
        PopUpWhenExpired = true;
        CloseWhenExpired = false;
        ShutDownWhenExpired = false;
        Theme = Theme.DefaultTheme;
        Sound = Sound.DefaultSound;
        LoopSound = false;
        WindowTitleMode = WindowTitleMode.ApplicationName;
        WindowSize = new(
            new(double.PositiveInfinity, double.PositiveInfinity, InterfaceScaler.BaseWindowWidth, InterfaceScaler.BaseWindowHeight),
            WindowState.Normal,
            WindowState.Normal,
            false /* isFullScreen */);
        LockInterface = false;
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
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the timer window should always be displayed on top of other windows.
    /// </summary>
    public bool AlwaysOnTop
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to prompt the user before closing the timer window if the timer is
    /// running.
    /// </summary>
    public bool PromptOnExit
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show progress in the taskbar.
    /// </summary>
    public bool ShowProgressInTaskbar
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to keep the computer awake while the timer is running.
    /// </summary>
    public bool DoNotKeepComputerAwake
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to reverse the progress bar (count backwards).
    /// </summary>
    public bool ReverseProgressBar
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to display time in the digital clock format.
    /// </summary>
    public bool DigitalClockTime
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the time elapsed rather than the time left.
    /// </summary>
    public bool ShowTimeElapsed
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the trigger time.
    /// </summary>
    public bool ShowTriggerTime
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to loop the timer continuously.
    /// </summary>
    public bool LoopTimer
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to pause before loop the timer continuously.
    /// </summary>
    public bool PauseBeforeLoopTimer
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the timer window should be brought to the top of other windows when
    /// the timer expires.
    /// </summary>
    public bool PopUpWhenExpired
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the timer window should be closed when the timer expires.
    /// </summary>
    public bool CloseWhenExpired
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether Windows should be shut down when the timer expires.
    /// </summary>
    public bool ShutDownWhenExpired
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets the theme of the timer window.
    /// </summary>
    public Theme? Theme
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets the sound to play when the timer expires, or <c>null</c> if no sound is to be played.
    /// </summary>
    public Sound? Sound
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the sound that plays when the timer expires should be looped until
    /// stopped by the user.
    /// </summary>
    public bool LoopSound
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating what information to display in the timer window title.
    /// </summary>
    public WindowTitleMode WindowTitleMode
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets the size, position, and state of the timer window.
    /// </summary>
    public WindowSize? WindowSize
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user interface should be locked, preventing the user from taking
    /// any action until the timer expires.
    /// </summary>
    public bool LockInterface
    {
        get;

        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
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
    /// <returns>A <see cref="TimerOptions"/> for the specified <see cref="TimerOptions"/>, or <c>null</c> if the specified <see cref="TimerOptions"/> is <c>null</c>.</returns>
    public static TimerOptions? FromTimerOptions(TimerOptions? options)
    {
        return options is not null ? new TimerOptions(options) : null;
    }

    /// <summary>
    /// Returns a <see cref="TimerOptions"/> for the specified <see cref="TimerOptionsInfo"/>, or <c>null</c> if
    /// the specified <see cref="TimerOptionsInfo"/> is <c>null</c>.
    /// </summary>
    /// <param name="info">A <see cref="TimerOptionsInfo"/>.</param>
    /// <returns>A <see cref="TimerOptions"/> for the specified <see cref="TimerOptionsInfo"/>, or <c>null</c> if the specified <see cref="TimerOptionsInfo"/> is <c>null</c>.</returns>
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

        Title = options.Title;
        AlwaysOnTop = options.AlwaysOnTop;
        PromptOnExit = options.PromptOnExit;
        ShowProgressInTaskbar = options.ShowProgressInTaskbar;
        DoNotKeepComputerAwake = options.DoNotKeepComputerAwake;
        ReverseProgressBar = options.ReverseProgressBar;
        DigitalClockTime = options.DigitalClockTime;
        ShowTimeElapsed = options.ShowTimeElapsed;
        ShowTriggerTime = options.ShowTriggerTime;
        LoopTimer = options.LoopTimer;
        PauseBeforeLoopTimer = options.PauseBeforeLoopTimer;
        PopUpWhenExpired = options.PopUpWhenExpired;
        CloseWhenExpired = options.CloseWhenExpired;
        ShutDownWhenExpired = options.ShutDownWhenExpired;
        Theme = options.Theme;
        Sound = options.Sound;
        LoopSound = options.LoopSound;
        WindowTitleMode = options.WindowTitleMode;
        WindowSize = WindowSize.FromWindowSize(options.WindowSize);
        LockInterface = options.LockInterface;

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

        Title = info.Title;
        AlwaysOnTop = info.AlwaysOnTop;
        PromptOnExit = info.PromptOnExit;
        ShowProgressInTaskbar = info.ShowProgressInTaskbar;
        DoNotKeepComputerAwake = info.DoNotKeepComputerAwake;
        ReverseProgressBar = info.ReverseProgressBar;
        DigitalClockTime = info.DigitalClockTime;
        ShowTimeElapsed = info.ShowTimeElapsed;
        ShowTriggerTime = info.ShowTriggerTime;
        LoopTimer = info.LoopTimer;
        PauseBeforeLoopTimer = info.PauseBeforeLoopTimer;
        PopUpWhenExpired = info.PopUpWhenExpired;
        CloseWhenExpired = info.CloseWhenExpired;
        ShutDownWhenExpired = info.ShutDownWhenExpired;
        Theme = Theme.FromIdentifier(info.ThemeIdentifier);
        Sound = Sound.FromIdentifier(info.SoundIdentifier);
        LoopSound = info.LoopSound;
        WindowTitleMode = info.WindowTitleMode;
        WindowSize = WindowSize.FromWindowSizeInfo(info.WindowSize);
        LockInterface = info.LockInterface;

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
            Title = Title,
            AlwaysOnTop = AlwaysOnTop,
            PromptOnExit = PromptOnExit,
            ShowProgressInTaskbar = ShowProgressInTaskbar,
            DoNotKeepComputerAwake = DoNotKeepComputerAwake,
            ReverseProgressBar = ReverseProgressBar,
            DigitalClockTime = DigitalClockTime,
            ShowTimeElapsed = ShowTimeElapsed,
            ShowTriggerTime = ShowTriggerTime,
            LoopTimer = LoopTimer,
            PauseBeforeLoopTimer = PauseBeforeLoopTimer,
            PopUpWhenExpired = PopUpWhenExpired,
            CloseWhenExpired = CloseWhenExpired,
            ShutDownWhenExpired = ShutDownWhenExpired,
            ThemeIdentifier = Theme?.Identifier,
            SoundIdentifier = Sound?.Identifier,
            LoopSound = LoopSound,
            WindowTitleMode = WindowTitleMode,
            WindowSize = WindowSizeInfo.FromWindowSize(WindowSize)!,
            LockInterface = LockInterface
        };
    }

    #endregion
}