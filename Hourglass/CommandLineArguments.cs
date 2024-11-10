// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandLineArguments.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

using Extensions;
using Managers;
using Properties;
using Timing;
using Windows;

/// <summary>
/// Parsed command-line arguments.
/// </summary>
public sealed class CommandLineArguments
{
    #region Properties

    /// <summary>
    /// Gets the command-line usage for this application.
    /// </summary>
    public static string Usage
    {
        get
        {
            string assemblyLocation = Assembly.GetEntryAssembly()!.CodeBase;
            string assemblyFileName = Path.GetFileName(assemblyLocation);
            return $"{Environment.NewLine}{Resources.Usage.Replace("hourglass.exe", assemblyFileName.ToLowerInvariant())}{Environment.NewLine}";
        }
    }

    /// <summary>
    /// Gets a value indicating whether the command-line arguments were not successfully parsed.
    /// </summary>
    public bool HasParseError { get; private set; }

    /// <summary>
    /// Gets an error message if the command-line arguments were not successfully parsed, or <c>null</c> otherwise.
    /// </summary>
    public string? ParseErrorMessage { get; private set; }

    /// <summary>
    /// Gets a value indicating whether command-line usage information should be shown to the user.
    /// </summary>
    public bool ShouldShowUsage { get; private set; }

    /// <summary>
    /// Gets a <see cref="TimerStart"/> values, or empty if no time was specified on the command line.
    /// </summary>
    public IEnumerable<TimerStart> TimerStart { get; private set; } = [];

    /// <summary>
    /// Gets a user-specified title for the timer.
    /// </summary>
    public string? Title { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the timer window should always be displayed on top of other windows.
    /// </summary>
    public bool AlwaysOnTop { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the timer window should be in full-screen mode.
    /// </summary>
    public bool IsFullScreen { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to prompt the user before closing the timer window if the timer is running.
    /// </summary>
    public bool PromptOnExit { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to show progress in the taskbar.
    /// </summary>
    public bool ShowProgressInTaskbar { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to keep the computer awake while the timer is running.
    /// </summary>
    public bool DoNotKeepComputerAwake { get; private set; }

    /// <summary>
    /// Gets a value indicating whether an icon for the app should be visible in the notification area of the
    /// taskbar.
    /// </summary>
    public bool ShowInNotificationArea { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to reverse the progress bar (count backwards).
    /// </summary>
    public bool ReverseProgressBar { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to display time in the digital clock format.
    /// </summary>
    public bool DigitalClockTime { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to show the time elapsed rather than the time left.
    /// </summary>
    public bool ShowTimeElapsed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to show the trigger time.
    /// </summary>
    public bool ShowTriggerTime { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to loop the timer continuously.
    /// </summary>
    public bool LoopTimer { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to pause before loop the timer continuously.
    /// </summary>
    public bool PauseBeforeLoopTimer { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the timer window should be brought to the top of other windows when the
    /// timer expires.
    /// </summary>
    public bool PopUpWhenExpired { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the timer window should be closed when the timer expires.
    /// </summary>
    public bool CloseWhenExpired { get; private set; }

    /// <summary>
    /// Gets a value indicating whether Windows should be shut down when the timer expires.
    /// </summary>
    public bool ShutDownWhenExpired { get; private set; }

    /// <summary>
    /// Gets the theme of the timer window.
    /// </summary>
    public Theme? Theme { get; private set; }

    /// <summary>
    /// Gets the sound to play when the timer expires, or <c>null</c> if no sound is to be played.
    /// </summary>
    public Sound? Sound { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the sound that plays when the timer expires should be looped until stopped
    /// by the user.
    /// </summary>
    public bool LoopSound { get; private set; }

    /// <summary>
    /// Gets a value indicating whether all saved timers should be opened when the application starts.
    /// </summary>
    public bool OpenSavedTimers { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to prefer interpreting time of day values as 24-hour time.
    /// </summary>
    public bool Prefer24HourTime { get; private set; }

    /// <summary>
    /// Order timers by the title first.
    /// </summary>
    public bool OrderByTitleFirst { get; private set; }

    /// <summary>
    /// Gets a value indicating what information to display in the timer window title.
    /// </summary>
    public WindowTitleMode WindowTitleMode { get; private set; }

    /// <summary>
    /// Gets a value that indicates whether the timer window is restored, minimized, or maximized.
    /// </summary>
    public WindowState WindowState { get; private set; }

    /// <summary>
    /// Gets the window's <see cref="Window.WindowState"/> before the window was minimized.
    /// </summary>
    public WindowState RestoreWindowState { get; private set; }

    /// <summary>
    /// Gets the size and location of the window.
    /// </summary>
    public Rect WindowBounds { get; private set; }

    /// <summary>
    /// Gets a value indicating whether space separated timer command-line arguments should be processed individually.
    /// </summary>
    public bool MultiTimers { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user interface should be locked, preventing the user from taking any
    /// action until the timer expires.
    /// </summary>
    public bool LockInterface { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to activate next window when minimized or closed.
    /// </summary>
    public bool ActivateNextWindow { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to execute pause all command.
    /// </summary>
    public bool PauseAll { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to execute resume all command.
    /// </summary>
    public bool ResumeAll { get; private set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Parses command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A <see cref="CommandLineArguments"/> object.</returns>
    public static CommandLineArguments Parse(IList<string> args)
    {
        try
        {
            return GetCommandLineArguments(args);
        }
        catch (ParseException e)
        {
            return new()
            {
                HasParseError = true,
                ParseErrorMessage = e.Message
            };
        }
    }

    /// <summary>
    /// Shows the command-line usage of this application in a window.
    /// </summary>
    /// <param name="errorMessage">An error message to display (optional).</param>
    public static void ShowUsage(string? errorMessage = null)
    {
        UsageDialog.ShowOrActivate(errorMessage);
    }

    /// <summary>
    /// Returns the <see cref="TimerOptions"/> specified in the parsed command-line arguments.
    /// </summary>
    /// <returns>The <see cref="TimerOptions"/> specified in the parsed command-line arguments.</returns>
    public TimerOptions GetTimerOptions()
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
            Theme = Theme,
            Sound = Sound,
            LoopSound = LoopSound,
            WindowTitleMode = WindowTitleMode,
            WindowSize = GetWindowSize(),
            LockInterface = LockInterface
        };
    }

    /// <summary>
    /// Returns the <see cref="WindowSize"/> specified in the parsed command-line arguments.
    /// </summary>
    /// <returns>The <see cref="WindowSize"/> specified in the parsed command-line arguments.</returns>
    public WindowSize GetWindowSize()
    {
        return new(
            WindowBounds,
            WindowState,
            RestoreWindowState,
            IsFullScreen);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Returns a <see cref="CommandLineArguments"/> instance based on the most recent options.
    /// </summary>
    /// <returns>A <see cref="CommandLineArguments"/> instance based on the most recent options.</returns>
    private static CommandLineArguments GetArgumentsFromMostRecentOptions()
    {
        TimerOptions options = TimerOptionsManager.Instance.MostRecentOptions;
        WindowSize windowSize = GetMostRecentWindowSize();

        return new()
        {
            Title = null,
            AlwaysOnTop = options.AlwaysOnTop,
            IsFullScreen = windowSize.IsFullScreen,
            PromptOnExit = options.PromptOnExit,
            ShowProgressInTaskbar = options.ShowProgressInTaskbar,
            DoNotKeepComputerAwake = options.DoNotKeepComputerAwake,
            ReverseProgressBar = options.ReverseProgressBar,
            DigitalClockTime = options.DigitalClockTime,
            ShowTimeElapsed = options.ShowTimeElapsed,
            ShowTriggerTime = options.ShowTriggerTime,
            ShowInNotificationArea = Settings.Default.ShowInNotificationArea,
            LoopTimer = options.LoopTimer,
            PauseBeforeLoopTimer = options.PauseBeforeLoopTimer,
            PopUpWhenExpired = options.PopUpWhenExpired,
            CloseWhenExpired = options.CloseWhenExpired,
            ShutDownWhenExpired = options.ShutDownWhenExpired,
            Theme = options.Theme,
            Sound = options.Sound,
            LoopSound = options.LoopSound,
            OpenSavedTimers = Settings.Default.OpenSavedTimersOnStartup,
            Prefer24HourTime = Settings.Default.Prefer24HourTime,
            ActivateNextWindow = Settings.Default.ActivateNextWindow,
            OrderByTitleFirst = Settings.Default.OrderByTitleFirst,
            WindowTitleMode = options.WindowTitleMode,
            WindowState = windowSize.WindowState != WindowState.Minimized ? windowSize.WindowState : windowSize.RestoreWindowState,
            RestoreWindowState = windowSize.RestoreWindowState,
            WindowBounds = windowSize.RestoreBounds,
            LockInterface = options.LockInterface
        };
    }

    /// <summary>
    /// Returns a <see cref="CommandLineArguments"/> instance based on the factory default settings.
    /// </summary>
    /// <returns>A <see cref="CommandLineArguments"/> instance based on the factory default settings.</returns>
    private static CommandLineArguments GetArgumentsFromFactoryDefaults()
    {
        TimerOptions defaultOptions = new();

        WindowSize mostRecentWindowSize = GetMostRecentWindowSize();
        Rect defaultWindowBounds = defaultOptions.WindowSize?.RestoreBounds ?? new(new(InterfaceScaler.BaseWindowWidth, InterfaceScaler.BaseWindowHeight));
        Rect defaultWindowBoundsWithLocation = mostRecentWindowSize.RestoreBounds.Merge(defaultWindowBounds);

        return new()
        {
            Title = defaultOptions.Title,
            AlwaysOnTop = defaultOptions.AlwaysOnTop,
            IsFullScreen = defaultOptions.WindowSize?.IsFullScreen == true,
            PromptOnExit = defaultOptions.PromptOnExit,
            ShowProgressInTaskbar = defaultOptions.ShowProgressInTaskbar,
            DoNotKeepComputerAwake = defaultOptions.DoNotKeepComputerAwake,
            ReverseProgressBar = defaultOptions.ReverseProgressBar,
            DigitalClockTime = defaultOptions.DigitalClockTime,
            ShowTimeElapsed = defaultOptions.ShowTimeElapsed,
            ShowTriggerTime = defaultOptions.ShowTriggerTime,
            ShowInNotificationArea = false,
            LoopTimer = defaultOptions.LoopTimer,
            PauseBeforeLoopTimer = defaultOptions.PauseBeforeLoopTimer,
            PopUpWhenExpired = defaultOptions.PopUpWhenExpired,
            CloseWhenExpired = defaultOptions.CloseWhenExpired,
            ShutDownWhenExpired = defaultOptions.ShutDownWhenExpired,
            Theme = defaultOptions.Theme,
            Sound = defaultOptions.Sound,
            LoopSound = defaultOptions.LoopSound,
            OpenSavedTimers = false,
            Prefer24HourTime = false,
            ActivateNextWindow = true,
            OrderByTitleFirst = false,
            WindowTitleMode = WindowTitleMode.ApplicationName,
            WindowState = defaultOptions.WindowSize?.WindowState ?? WindowState.Normal,
            RestoreWindowState = defaultOptions.WindowSize?.RestoreWindowState ?? WindowState.Normal,
            WindowBounds = defaultWindowBoundsWithLocation,
            LockInterface = defaultOptions.LockInterface
        };
    }

    /// <summary>
    /// Returns the most recent <see cref="WindowSize"/>.
    /// </summary>
    /// <returns>The most recent <see cref="WindowSize"/>.</returns>
    private static WindowSize GetMostRecentWindowSize()
    {
        WindowSize? windowSizeFromSettings = Settings.Default.WindowSize;
        WindowSize? windowSizeFromSibling = WindowSize.FromWindowOfType<TimerWindow>().Offset();
        return windowSizeFromSibling ?? windowSizeFromSettings ?? new WindowSize();
    }

    /// <summary>
    /// Parses command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The parsed command-line arguments.</returns>
    /// <exception cref="ParseException">If the command-line arguments could not be parsed.</exception>
    private static CommandLineArguments GetCommandLineArguments(IEnumerable<string> args)
    {
        CommandLineArguments argumentsBasedOnMostRecentOptions = GetArgumentsFromMostRecentOptions();
        CommandLineArguments argumentsBasedOnFactoryDefaults = GetArgumentsFromFactoryDefaults();
        bool useFactoryDefaults = false;

        HashSet<string> specifiedSwitches = [];
        Queue<string> remainingArgs = new(args);
        while (remainingArgs.Count > 0)
        {
            string arg = remainingArgs.Dequeue();

            switch (arg)
            {
                // Commands.

                case "pause":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "pause");
                    ThrowIfDuplicateSwitch(specifiedSwitches, "resume");

                    argumentsBasedOnMostRecentOptions.PauseAll = true;
                    argumentsBasedOnFactoryDefaults.PauseAll = true;
                    break;

                case "resume":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "resume");
                    ThrowIfDuplicateSwitch(specifiedSwitches, "pause");

                    argumentsBasedOnMostRecentOptions.ResumeAll = true;
                    argumentsBasedOnFactoryDefaults.ResumeAll = true;
                    break;

                // Options.

                case "--title":
                case "-t":
                case "/t":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--title");

                    string title = GetRequiredValue(arg, remainingArgs);

                    argumentsBasedOnMostRecentOptions.Title = title;
                    argumentsBasedOnFactoryDefaults.Title = title;
                    break;

                case "--always-on-top":
                case "-a":
                case "/a":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--always-on-top");

                    bool alwaysOnTop = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.AlwaysOnTop);

                    argumentsBasedOnMostRecentOptions.AlwaysOnTop = alwaysOnTop;
                    argumentsBasedOnFactoryDefaults.AlwaysOnTop = alwaysOnTop;
                    break;

                case "--full-screen":
                case "-f":
                case "/f":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--full-screen");

                    bool isFullScreen = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.IsFullScreen);

                    argumentsBasedOnMostRecentOptions.IsFullScreen = isFullScreen;
                    argumentsBasedOnFactoryDefaults.IsFullScreen = isFullScreen;
                    break;

                case "--prompt-on-exit": // Backward compatibility.
                case "--prompt-on-close":
                case "-o":
                case "/o":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--prompt-on-exit"); // Backward compatibility.
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--prompt-on-close");

                    bool promptOnExit = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.PromptOnExit);

                    argumentsBasedOnMostRecentOptions.PromptOnExit = promptOnExit;
                    argumentsBasedOnFactoryDefaults.PromptOnExit = promptOnExit;
                    break;

                case "--show-progress-in-taskbar":
                case "-y":
                case "/y":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--show-progress-in-taskbar");

                    bool showProgressInTaskbar = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.ShowProgressInTaskbar);

                    argumentsBasedOnMostRecentOptions.ShowProgressInTaskbar = showProgressInTaskbar;
                    argumentsBasedOnFactoryDefaults.ShowProgressInTaskbar = showProgressInTaskbar;
                    break;

                case "--do-not-keep-awake":
                case "-k":
                case "/k":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--do-not-keep-awake");

                    bool doNotKeepComputerAwake = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.DoNotKeepComputerAwake);

                    argumentsBasedOnMostRecentOptions.DoNotKeepComputerAwake = doNotKeepComputerAwake;
                    argumentsBasedOnFactoryDefaults.DoNotKeepComputerAwake = doNotKeepComputerAwake;
                    break;

                case "--reverse-progress-bar":
                case "-g":
                case "/g":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--reverse-progress-bar");

                    bool reverseProgressBar = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.ReverseProgressBar);

                    argumentsBasedOnMostRecentOptions.ReverseProgressBar = reverseProgressBar;
                    argumentsBasedOnFactoryDefaults.ReverseProgressBar = reverseProgressBar;
                    break;

                case "--digital-clock-time":
                case "-c":
                case "/c":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--digital-clock-time");

                    bool digitalClockTime = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.DigitalClockTime);

                    argumentsBasedOnMostRecentOptions.DigitalClockTime = digitalClockTime;
                    argumentsBasedOnFactoryDefaults.DigitalClockTime = digitalClockTime;
                    break;

                case "--show-time-elapsed":
                case "-u":
                case "/u":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--show-time-elapsed");

                    bool showTimeElapsed = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.ShowTimeElapsed);

                    argumentsBasedOnMostRecentOptions.ShowTimeElapsed = showTimeElapsed;
                    argumentsBasedOnFactoryDefaults.ShowTimeElapsed = showTimeElapsed;
                    break;

                case "--show-trigger-time":
                case "-st":
                case "/st":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--show-trigger-time");

                    bool showTriggerTime = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.ShowTriggerTime);

                    argumentsBasedOnMostRecentOptions.ShowTriggerTime = showTriggerTime;
                    argumentsBasedOnFactoryDefaults.ShowTriggerTime = showTriggerTime;
                    break;

                case "--show-in-notification-area":
                case "-n":
                case "/n":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--show-in-notification-area");

                    bool showInNotificationArea = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.ShowInNotificationArea);

                    argumentsBasedOnMostRecentOptions.ShowInNotificationArea = showInNotificationArea;
                    argumentsBasedOnFactoryDefaults.ShowInNotificationArea = showInNotificationArea;
                    break;

                case "--loop-timer":
                case "-l":
                case "/l":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--loop-timer");

                    bool loopTimer = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.LoopTimer);

                    argumentsBasedOnMostRecentOptions.LoopTimer = loopTimer;
                    argumentsBasedOnFactoryDefaults.LoopTimer = loopTimer;
                    break;

                case "--pause-after-loop-timer":
                case "-pl":
                case "/pl":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--pause-after-loop-timer");

                    bool pauseBeforeLoopTimer = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.PauseBeforeLoopTimer);

                    argumentsBasedOnMostRecentOptions.PauseBeforeLoopTimer = pauseBeforeLoopTimer;
                    argumentsBasedOnFactoryDefaults.PauseBeforeLoopTimer = pauseBeforeLoopTimer;
                    break;

                case "--pop-up-when-expired":
                case "-p":
                case "/p":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--pop-up-when-expired");

                    bool popUpWhenExpired = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.PopUpWhenExpired);

                    argumentsBasedOnMostRecentOptions.PopUpWhenExpired = popUpWhenExpired;
                    argumentsBasedOnFactoryDefaults.PopUpWhenExpired = popUpWhenExpired;
                    break;

                case "--close-when-expired":
                case "-e":
                case "/e":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--close-when-expired");

                    bool closeWhenExpired = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.CloseWhenExpired);

                    argumentsBasedOnMostRecentOptions.CloseWhenExpired = closeWhenExpired;
                    argumentsBasedOnFactoryDefaults.CloseWhenExpired = closeWhenExpired;
                    break;

                case "--shut-down-when-expired":
                case "-x":
                case "/x":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--shut-down-when-expired");

                    bool shutDownWhenExpired = GetBoolValue(
                        arg,
                        remainingArgs);

                    argumentsBasedOnMostRecentOptions.ShutDownWhenExpired = shutDownWhenExpired;
                    argumentsBasedOnFactoryDefaults.ShutDownWhenExpired = shutDownWhenExpired;
                    break;

                case "--theme":
                case "-m":
                case "/m":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--theme");

                    Theme? theme = GetThemeValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.Theme);

                    argumentsBasedOnMostRecentOptions.Theme = theme;
                    argumentsBasedOnFactoryDefaults.Theme = theme;
                    break;

                case "--sound":
                case "-s":
                case "/s":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--sound");

                    Sound? sound = GetSoundValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.Sound);

                    argumentsBasedOnMostRecentOptions.Sound = sound;
                    argumentsBasedOnFactoryDefaults.Sound = sound;
                    break;

                case "--loop-sound":
                case "-r":
                case "/r":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--loop-sound");

                    bool loopSound = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.LoopSound);

                    argumentsBasedOnMostRecentOptions.LoopSound = loopSound;
                    argumentsBasedOnFactoryDefaults.LoopSound = loopSound;
                    break;

                case "--open-saved-timers":
                case "-v":
                case "/v":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--open-saved-timers");

                    bool openSavedTimers = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.OpenSavedTimers);

                    argumentsBasedOnMostRecentOptions.OpenSavedTimers = openSavedTimers;
                    argumentsBasedOnFactoryDefaults.OpenSavedTimers = openSavedTimers;
                    break;

                case "--prefer-24h-time":
                case "-j":
                case "/j":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--prefer-24h-time");

                    bool prefer24HourTime = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.Prefer24HourTime);

                    argumentsBasedOnMostRecentOptions.Prefer24HourTime = prefer24HourTime;
                    argumentsBasedOnFactoryDefaults.Prefer24HourTime = prefer24HourTime;
                    break;

                case "--activate-next":
                case "-an":
                case "/an":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--activate-next");

                    bool activateNextWindow = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.ActivateNextWindow);

                    argumentsBasedOnMostRecentOptions.ActivateNextWindow = activateNextWindow;
                    argumentsBasedOnFactoryDefaults.ActivateNextWindow = activateNextWindow;
                    break;

                case "--order-by-title":
                case "-ot":
                case "/ot":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--order-by-title");

                    bool orderByTitleFirst = GetBoolValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.OrderByTitleFirst);

                    argumentsBasedOnMostRecentOptions.OrderByTitleFirst = orderByTitleFirst;
                    argumentsBasedOnFactoryDefaults.OrderByTitleFirst = orderByTitleFirst;
                    break;

                case "--window-title":
                case "-i":
                case "/i":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--window-title");

                    WindowTitleMode windowTitleMode = GetWindowTitleModeValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.WindowTitleMode);

                    argumentsBasedOnMostRecentOptions.WindowTitleMode = windowTitleMode;
                    argumentsBasedOnFactoryDefaults.WindowTitleMode = windowTitleMode;
                    break;

                case "--window-state":
                case "-w":
                case "/w":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--window-state");

                    WindowState windowState = GetWindowStateValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.WindowState);

                    argumentsBasedOnMostRecentOptions.WindowState = windowState;
                    argumentsBasedOnFactoryDefaults.WindowState = windowState;
                    break;

                case "--window-bounds":
                case "-b":
                case "/b":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--window-bounds");

                    Rect windowBounds = GetRectValue(
                        arg,
                        remainingArgs,
                        argumentsBasedOnMostRecentOptions.WindowBounds);

                    argumentsBasedOnMostRecentOptions.WindowBounds = argumentsBasedOnMostRecentOptions.WindowBounds.Merge(windowBounds);
                    argumentsBasedOnFactoryDefaults.WindowBounds = argumentsBasedOnFactoryDefaults.WindowBounds.Merge(windowBounds);
                    break;

                case "--multi-timers":
                case "-mt":
                case "/mt":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--multi-timers");

                    bool multiTimers = GetBoolValue(
                        arg,
                        remainingArgs);

                    argumentsBasedOnMostRecentOptions.MultiTimers = multiTimers;
                    argumentsBasedOnFactoryDefaults.MultiTimers = multiTimers;
                    break;

                case "--lock-interface":
                case "-z":
                case "/z":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--lock-interface");

                    bool lockInterface = GetBoolValue(
                        arg,
                        remainingArgs);

                    argumentsBasedOnMostRecentOptions.LockInterface = lockInterface;
                    argumentsBasedOnFactoryDefaults.LockInterface = lockInterface;
                    break;

                case "--use-factory-defaults":
                case "-d":
                case "/d":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--use-factory-defaults");

                    useFactoryDefaults = true;
                    break;

                case "--help":
                case "-h":
                case "-?":
                case "/h":
                case "/?":
                    ThrowIfDuplicateSwitch(specifiedSwitches, "--help");

                    argumentsBasedOnMostRecentOptions.ShouldShowUsage = true;
                    argumentsBasedOnFactoryDefaults.ShouldShowUsage = true;
                    break;

                default:
                    if (IsSwitch(arg))
                    {
                        string message = string.Format(
                            Resources.ResourceManager.GetEffectiveProvider(),
                            Resources.CommandLineArgumentsParseExceptionUnrecognizedSwitchFormatString,
                            arg);

                        throw new ParseException(message);
                    }

                    List<string> inputArgs = [arg, ..remainingArgs];
                    remainingArgs.Clear();

                    IEnumerable<TimerStart> timerStart = GetTimerStartValue(inputArgs, argumentsBasedOnMostRecentOptions.MultiTimers).ToArray();

                    argumentsBasedOnMostRecentOptions.TimerStart = timerStart;
                    argumentsBasedOnFactoryDefaults.TimerStart = timerStart;
                    break;
            }
        }

        return useFactoryDefaults ? argumentsBasedOnFactoryDefaults : argumentsBasedOnMostRecentOptions;
    }

    /// <summary>
    /// Returns the next value in <paramref name="remainingArgs"/>.
    /// </summary>
    /// <param name="remainingArgs">The unparsed arguments.</param>
    /// <returns>The next value in <paramref name="remainingArgs"/>.</returns>
    private static string? GetValue(Queue<string> remainingArgs)
    {
        if (remainingArgs.Count > 0 && !IsSwitch(remainingArgs.Peek()))
        {
            string value = remainingArgs.Dequeue();
            return UnescapeValue(value);
        }

        return null;
    }

    /// <summary>
    /// Returns the next value in <paramref name="remainingArgs"/>, or throws an exception if <paramref
    /// name="remainingArgs"/> is empty or the next argument in <paramref name="remainingArgs"/> is a switch.
    /// </summary>
    /// <param name="arg">The name of the argument for which the value is to be returned.</param>
    /// <param name="remainingArgs">The unparsed arguments.</param>
    /// <returns>The next value in <paramref name="remainingArgs"/>.</returns>
    /// <exception cref="ParseException">If <paramref name="remainingArgs"/> is empty or the next argument in <paramref name="remainingArgs"/> is a switch.</exception>
    private static string GetRequiredValue(string arg, Queue<string> remainingArgs)
    {
        string? value = GetValue(remainingArgs);

        if (value is null)
        {
            string message = string.Format(
                Resources.ResourceManager.GetEffectiveProvider(),
                Resources.CommandLineArgumentsParseExceptionMissingValueForSwitchFormatString,
                arg);

            throw new ParseException(message);
        }

        return value;
    }

    /// <summary>
    /// Returns the next <see cref="bool"/> value in <paramref name="remainingArgs"/>, or throws an exception if
    /// <paramref name="remainingArgs"/> is empty or the next argument is not "on" or "off".
    /// </summary>
    /// <param name="arg">The name of the argument for which the value is to be returned.</param>
    /// <param name="remainingArgs">The unparsed arguments.</param>
    /// <returns>The next <see cref="bool"/> value in <paramref name="remainingArgs"/>.</returns>
    /// <exception cref="ParseException">If <paramref name="remainingArgs"/> is empty or the next argument is not "on" or "off".</exception>
    private static bool GetBoolValue(string arg, Queue<string> remainingArgs)
    {
        string value = GetRequiredValue(arg, remainingArgs);

        switch (value.ToLowerInvariant())
        {
            case "on":
                return true;

            case "off":
                return false;

            default:
                string message = string.Format(
                    Resources.ResourceManager.GetEffectiveProvider(),
                    Resources.CommandLineArgumentsParseExceptionInvalidValueForSwitchFormatString,
                    arg,
                    value);

                throw new ParseException(message);
        }
    }

    /// <summary>
    /// Returns the next <see cref="bool"/> value in <paramref name="remainingArgs"/>, or throws an exception if
    /// <paramref name="remainingArgs"/> is empty or the next argument is not "on", "off", or "last".
    /// </summary>
    /// <param name="arg">The name of the argument for which the value is to be returned.</param>
    /// <param name="remainingArgs">The unparsed arguments.</param>
    /// <param name="last">The value of the argument returned when the user specifies "last".</param>
    /// <returns>The next <see cref="bool"/> value in <paramref name="remainingArgs"/>.</returns>
    /// <exception cref="ParseException">If <paramref name="remainingArgs"/> is empty or the next argument is not "on", "off", or "last".</exception>
    private static bool GetBoolValue(string arg, Queue<string> remainingArgs, bool last)
    {
        string value = GetRequiredValue(arg, remainingArgs);

        switch (value.ToLowerInvariant())
        {
            case "on":
                return true;

            case "off":
                return false;

            case "last":
                return last;

            default:
                string message = string.Format(
                    Resources.ResourceManager.GetEffectiveProvider(),
                    Resources.CommandLineArgumentsParseExceptionInvalidValueForSwitchFormatString,
                    arg,
                    value);

                throw new ParseException(message);
        }
    }

    /// <summary>
    /// Returns the next <see cref="Theme"/> value in <paramref name="remainingArgs"/>, or throws an exception if
    /// <paramref name="remainingArgs"/> is empty or the next argument is not "last" or a valid representation of a
    /// <see cref="Theme"/>.
    /// </summary>
    /// <param name="arg">The name of the argument for which the value is to be returned.</param>
    /// <param name="remainingArgs">The unparsed arguments.</param>
    /// <param name="last">The value of the argument returned when the user specifies "last".</param>
    /// <returns>The next <see cref="Theme"/> value in <paramref name="remainingArgs"/></returns>
    /// <exception cref="ParseException">If <paramref name="remainingArgs"/> is empty or the next argument is not "last" or a valid representation of a <see cref="Theme"/>.</exception>
    private static Theme? GetThemeValue(string arg, Queue<string> remainingArgs, Theme? last)
    {
        string value = GetRequiredValue(arg, remainingArgs);

        switch (value.ToLowerInvariant())
        {
            case "last":
                return last;

            default:
                Theme? theme = ThemeManager.Instance.GetThemeByIdentifier(value.ToLowerInvariant()) ??
                               ThemeManager.Instance.GetThemeByName(value, StringComparison.CurrentCultureIgnoreCase);

                if (theme is null)
                {
                    string message = string.Format(
                        Resources.ResourceManager.GetEffectiveProvider(),
                        Resources.CommandLineArgumentsParseExceptionInvalidValueForSwitchFormatString,
                        arg,
                        value);

                    throw new ParseException(message);
                }

                return theme;
        }
    }

    /// <summary>
    /// Returns the next <see cref="Sound"/> value in <paramref name="remainingArgs"/>, or throws an exception if
    /// <paramref name="remainingArgs"/> is empty or the next argument is not "none", "last", or a valid
    /// representation of a <see cref="Sound"/>.
    /// </summary>
    /// <param name="arg">The name of the argument for which the value is to be returned.</param>
    /// <param name="remainingArgs">The unparsed arguments.</param>
    /// <param name="last">The value of the argument returned when the user specifies "last".</param>
    /// <returns>The next <see cref="Sound"/> value in <paramref name="remainingArgs"/>.</returns>
    /// <exception cref="ParseException">if <paramref name="remainingArgs"/> is empty or the next argument is not "none", "last", or a valid representation of a <see cref="Sound"/>.</exception>
    private static Sound? GetSoundValue(string arg, Queue<string> remainingArgs, Sound? last)
    {
        string value = GetRequiredValue(arg, remainingArgs);

        switch (value.ToLowerInvariant())
        {
            case "none":
                return null;

            case "last":
                return last;

            default:
                Sound? sound = SoundManager.Instance.GetSoundByName(value, StringComparison.CurrentCultureIgnoreCase);

                if (sound is null)
                {
                    string message = string.Format(
                        Resources.ResourceManager.GetEffectiveProvider(),
                        Resources.CommandLineArgumentsParseExceptionInvalidValueForSwitchFormatString,
                        arg,
                        value);

                    throw new ParseException(message);
                }

                return sound;
        }
    }

    /// <summary>
    /// Returns the next <see cref="WindowTitleMode"/> value in <paramref name="remainingArgs"/>, or throws an exception if <paramref name="remainingArgs"/> is empty or the next argument is not "app", "left", "elapsed", "title", or "last".
    /// </summary>
    /// <param name="arg">The name of the argument for which the value is to be returned.</param>
    /// <param name="remainingArgs">The unparsed arguments.</param>
    /// <param name="last">The value of the argument returned when the user specifies "last".</param>
    /// <returns>The next <see cref="WindowTitleMode"/> value in <paramref name="remainingArgs"/>.</returns>
    /// <exception cref="ParseException">If <paramref name="remainingArgs"/> is empty or the next argument is not "app", "left", "elapsed", "title", or "last".</exception>
    private static WindowTitleMode GetWindowTitleModeValue(string arg, Queue<string> remainingArgs, WindowTitleMode last)
    {
        string value = GetRequiredValue(arg, remainingArgs);

        switch (value.ToLowerInvariant())
        {
            case "none":
                return WindowTitleMode.None;

            case "app":
                return WindowTitleMode.ApplicationName;

            case "left":
                return WindowTitleMode.TimeLeft;

            case "elapsed":
                return WindowTitleMode.TimeElapsed;

            case "title":
                return WindowTitleMode.TimerTitle;

            case "left+title":
                return WindowTitleMode.TimeLeftPlusTimerTitle;

            case "elapsed+title":
                return WindowTitleMode.TimeElapsedPlusTimerTitle;

            case "title+left":
                return WindowTitleMode.TimerTitlePlusTimeLeft;

            case "title+elapsed":
                return WindowTitleMode.TimerTitlePlusTimeElapsed;

            case "last":
                return last;

            default:
                string message = string.Format(
                    Resources.ResourceManager.GetEffectiveProvider(),
                    Resources.CommandLineArgumentsParseExceptionInvalidValueForSwitchFormatString,
                    arg,
                    value);

                throw new ParseException(message);
        }
    }

    /// <summary>
    /// Returns the next <see cref="WindowState"/> value in <paramref name="remainingArgs"/>, or throws an exception if <paramref name="remainingArgs"/> is empty or the next argument is not "normal", "maximized", "minimized", or "last".
    /// </summary>
    /// <param name="arg">The name of the argument for which the value is to be returned.</param>
    /// <param name="remainingArgs">The unparsed arguments.</param>
    /// <param name="last">The value of the argument returned when the user specifies "last".</param>
    /// <returns>The next <see cref="WindowState"/> value in <paramref name="remainingArgs"/>.</returns>
    /// <exception cref="ParseException">If <paramref name="remainingArgs"/> is empty or the next argument is not "normal", "maximized", "minimized", or "last".</exception>
    private static WindowState GetWindowStateValue(string arg, Queue<string> remainingArgs, WindowState last)
    {
        string value = GetRequiredValue(arg, remainingArgs);

        switch (value.ToLowerInvariant())
        {
            case "normal":
                return WindowState.Normal;

            case "maximized":
                return WindowState.Maximized;

            case "minimized":
                return WindowState.Minimized;

            case "last":
                return last;

            default:
                string message = string.Format(
                    Resources.ResourceManager.GetEffectiveProvider(),
                    Resources.CommandLineArgumentsParseExceptionInvalidValueForSwitchFormatString,
                    arg,
                    value);

                throw new ParseException(message);
        }
    }

    /// <summary>
    /// Returns the next <see cref="Rect"/> value in <paramref name="remainingArgs"/>, or throws an exception if
    /// <paramref name="remainingArgs"/> is empty or the next argument is not a valid representation of a <see
    /// cref="Rect"/>.
    /// </summary>
    /// <param name="arg">The name of the argument for which the value is to be returned.</param>
    /// <param name="remainingArgs">The unparsed arguments.</param>
    /// <param name="last">The value of the argument returned when the user specifies "last".</param>
    /// <returns>The next <see cref="Rect"/> value in <paramref name="remainingArgs"/>.</returns>
    /// <exception cref="ParseException">If <paramref name="remainingArgs"/> is empty or the next argument is not a valid representation of a <see cref="Rect"/>.</exception>
    private static Rect GetRectValue(string arg, Queue<string> remainingArgs, Rect last)
    {
        string value = GetRequiredValue(arg, remainingArgs);

        try
        {
            if (value == "last")
            {
                return last;
            }

            string adjustedValue = Regex.Replace(value, @"\bauto\b", "Infinity");
            return Rect.Parse(adjustedValue);
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            string message = string.Format(
                Resources.ResourceManager.GetEffectiveProvider(),
                Resources.CommandLineArgumentsParseExceptionInvalidValueForSwitchFormatString,
                arg,
                value);

            throw new ParseException(message, ex);
        }
    }

    /// <summary>
    /// Returns the <see cref="TimerStart"/> value corresponding to the concatenation of all <paramref
    /// name="remainingArgs"/>, or throws an exception if the concatenation of all <paramref name="remainingArgs"/>
    /// is not a valid representation of a <see cref="TimerStart"/>.
    /// </summary>
    /// <param name="remainingArgs">The unparsed arguments.</param>
    /// <param name="multiTimers">Treat each timer argument as a separate timer.</param>
    /// <returns>The <see cref="TimerStart"/> value corresponding to the concatenation of all <paramref name="remainingArgs"/> or individual timer values if <paramref name="multiTimers"/> if <c>true</c>.</returns>
    /// <exception cref="ParseException">If the concatenation of all <paramref name="remainingArgs"/> is not a valid representation of a <see cref="TimerStart"/>.</exception>
    private static IEnumerable<TimerStart> GetTimerStartValue(IEnumerable<string> remainingArgs, bool multiTimers)
    {
        if (!multiTimers)
        {
            string value = string.Join(" ", remainingArgs);
            remainingArgs = [ value ];
        }

        foreach (string arg in remainingArgs)
        {
            TimerStart? timerStart = Timing.TimerStart.FromString(arg);

            if (timerStart is null)
            {
                string message = string.Format(
                    Resources.ResourceManager.GetEffectiveProvider(),
                    Resources.CommandLineArgumentsParseExceptionInvalidTimerInputFormatString,
                    arg);

                throw new ParseException(message);
            }

            yield return timerStart;
        }
    }

    /// <summary>
    /// Returns a value indicating whether a string is a command-line switch.
    /// </summary>
    /// <param name="arg">A string.</param>
    /// <returns>A value indicating whether <paramref name="arg"/> is a command-line switch.</returns>
    private static bool IsSwitch(string arg)
    {
        return arg.StartsWith("-") || arg.StartsWith("/");
    }

    /// <summary>
    /// Unescapes a command-line value.
    /// </summary>
    /// <remarks>
    /// A value is any command-line argument not beginning with `-`. If the user must specify a command-line value
    /// that begins with `-`, the user must escape the `-` with `'`.
    /// </remarks>
    /// <param name="value">An escaped value string.</param>
    /// <returns>The unescaped value.</returns>
    private static string UnescapeValue(string value)
    {
        return value.StartsWith("'") ? value.Substring(1) : value;
    }

    /// <summary>
    /// Throws an exception if <paramref name="canonicalSwitch"/> is already in <paramref name="specifiedSwitches"/>,
    /// or otherwise adds <paramref name="canonicalSwitch"/> to <paramref name="specifiedSwitches"/>.
    /// </summary>
    /// <param name="specifiedSwitches">The switch arguments that are already specified.</param>
    /// <param name="canonicalSwitch">The canonical representation of a switch argument.</param>
    private static void ThrowIfDuplicateSwitch(HashSet<string> specifiedSwitches, string canonicalSwitch)
    {
        if (specifiedSwitches.Add(canonicalSwitch))
        {
            return;
        }

        string message = string.Format(
            Resources.ResourceManager.GetEffectiveProvider(),
            Resources.CommandLineArgumentsParseExceptionDuplicateSwitchFormatString,
            canonicalSwitch);

        throw new ParseException(message);
    }

    #endregion

    /// <summary>
    /// Represents an error during <see cref="GetCommandLineArguments"/>.
    /// </summary>
    [Serializable]
#pragma warning disable S3925
    public sealed class ParseException : Exception
#pragma warning restore S3925
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ParseException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a <c>null</c> reference if no inner exception is specified.</param>
        public ParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}