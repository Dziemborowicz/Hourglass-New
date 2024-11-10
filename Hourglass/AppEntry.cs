// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppEntry.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

using Extensions;
using Managers;
using Properties;
using Timing;
using Windows;

using Microsoft.VisualBasic.ApplicationServices;

using StartupEventArgs = Microsoft.VisualBasic.ApplicationServices.StartupEventArgs;

/// <summary>
/// Handles application start up, command-line arguments, and ensures that only one instance of the application is
/// running at any time.
/// </summary>
public sealed class AppEntry : WindowsFormsApplicationBase
{
    static AppEntry()
    {
        Timeline.DesiredFrameRateProperty.OverrideMetadata(
            typeof(Timeline),
            new FrameworkPropertyMetadata(20));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppEntry"/> class.
    /// </summary>
    private AppEntry()
    {
        IsSingleInstance = true;
    }

    /// <summary>
    /// The entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        AppEntry appEntry = new();
        appEntry.Run(args);
    }

    /// <summary>
    /// Invoked when the application starts.
    /// </summary>
    /// <param name="eventArgs">Contains the command-line arguments of the application and indicates whether the
    /// application startup should be canceled.</param>
    /// <returns>A value indicating whether the application should continue starting up.</returns>
    protected override bool OnStartup(StartupEventArgs eventArgs)
    {
        AppManager.Instance.Initialize();

        CommandLineArguments arguments = CommandLineArguments.Parse(eventArgs.CommandLine);
        if (arguments.ShouldShowUsage || arguments.HasParseError)
        {
            CommandLineArguments.ShowUsage(arguments.ParseErrorMessage);
            AppManager.Instance.Dispose();
            return false;
        }

        SetGlobalSettingsFromArguments(arguments);

        Application app = new();
        app.Startup += delegate { ShowTimerWindowsForArguments(arguments); };
        app.SessionEnding += ExitEventHandler;
        app.Exit += ExitEventHandler;

        app.Run();

        return false;

        static void ExitEventHandler(object sender, EventArgs e)
        {
            if (!AppManager.Instance.Disposed)
            {
                AppManager.Instance.Persist();
                AppManager.Instance.Dispose();
            }
        }
    }

    /// <summary>
    /// Invoked when a subsequent instance of this application starts.
    /// </summary>
    /// <param name="eventArgs">Contains the command-line arguments of the subsequent application instance and indicates
    /// whether the first application instance should be brought to the foreground.</param>
    protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
    {
        CommandLineArguments arguments = CommandLineArguments.Parse(eventArgs.CommandLine);
        if (arguments.ShouldShowUsage || arguments.HasParseError)
        {
            CommandLineArguments.ShowUsage(arguments.ParseErrorMessage);
            return;
        }

        SetGlobalSettingsFromArguments(arguments);

        ShowTimerWindowsForArguments(arguments);
    }

    private static int _openSavedTimersExecuted;

    /// <summary>
    /// Shows a new timer window or windows for all saved timers, depending on whether the <see
    /// cref="CommandLineArguments"/> specify to open saved timers.
    /// </summary>
    /// <param name="arguments">Parsed command-line arguments.</param>
    private static void ShowTimerWindowsForArguments(CommandLineArguments arguments)
    {
        const int executed = 1;

        if (System.Threading.Interlocked.Exchange(ref _openSavedTimersExecuted, executed) != executed &&
            arguments.OpenSavedTimers && TimerManager.Instance.ResumableTimers.Any())
        {
            ShowSavedTimerWindows(arguments);
            ShowNewTimerWindows();
        }
        else
        {
            ShowNewTimerWindows(arguments is { PauseAll: false, ResumeAll: false });
        }

        if (arguments.PauseAll)
        {
            TimerManager.PauseAll();
        }

        if (arguments.ResumeAll)
        {
            TimerManager.ResumeAll();
        }

        void ShowNewTimerWindows(bool forceNew = false)
        {
            IEnumerable<TimerStart?> timerStarts = arguments.TimerStart;

            if (forceNew)
            {
                timerStarts = timerStarts.DefaultIfEmpty(null);
            }

            foreach (TimerStart? timerStart in timerStarts)
            {
                ShowNewTimerWindow(arguments, timerStart);
            }
        }
    }

    /// <summary>
    /// Shows a new timer window. The window will run the <see cref="TimerStart"/> specified in the <see
    /// cref="CommandLineArguments"/>, or it will display in input mode if there is no <see cref="TimerStart"/>.
    /// </summary>
    /// <param name="arguments">Parsed command-line arguments.</param>
    /// <param name="timerStart">Timer start.</param>
    private static void ShowNewTimerWindow(CommandLineArguments arguments, TimerStart? timerStart)
    {
        TimerWindow window = new(timerStart);
        window.Options.Set(arguments.GetTimerOptions());
        window.Restore(arguments.GetWindowSize(), RestoreOptions.AllowMinimized);
        window.Show();

        if (window.WindowState != WindowState.Minimized)
        {
            window.BringToFrontAndActivate();
        }
    }

    /// <summary>
    /// Shows windows for all saved timers.
    /// </summary>
    /// <param name="arguments">Parsed command-line arguments.</param>
    private static void ShowSavedTimerWindows(CommandLineArguments arguments)
    {
        foreach (Timer savedTimer in TimerManager.Instance.ResumableTimers)
        {
            TimerWindow window = new();

            window.Restore(savedTimer.Options.WindowSize ?? arguments.GetWindowSize(), RestoreOptions.AllowMinimized);

            window.Show(savedTimer);
        }
    }

    /// <summary>
    /// Sets global options from parsed command-line arguments.
    /// </summary>
    /// <param name="arguments">Parsed command-line arguments.</param>
    private static void SetGlobalSettingsFromArguments(CommandLineArguments arguments)
    {
        Settings.Default.ShowInNotificationArea = arguments.ShowInNotificationArea;
        Settings.Default.OpenSavedTimersOnStartup = arguments.OpenSavedTimers;
        Settings.Default.Prefer24HourTime = arguments.Prefer24HourTime;
        Settings.Default.ActivateNextWindow = arguments.ActivateNextWindow;
        Settings.Default.OrderByTitleFirst = arguments.OrderByTitleFirst;
        Settings.Default.DigitalClockTime = arguments.DigitalClockTime;
    }
}