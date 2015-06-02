﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandLineArguments.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows;

    using Hourglass.Properties;

    /// <summary>
    /// Parsed command-line arguments.
    /// </summary>
    public class CommandLineArguments
    {
        #region Properties

        /// <summary>
        /// Gets the command-line usage for this application.
        /// </summary>
        public static string Usage
        {
            get
            {
                string assemblyLocation = Assembly.GetEntryAssembly().CodeBase;
                string assemblyFileName = Path.GetFileName(assemblyLocation);
                return string.Format(Resources.Usage, assemblyFileName);
            }
        }

        /// <summary>
        /// Gets a <see cref="TimerInput"/>, or <c>null</c> if no <see cref="TimerInput"/> was specified on the command
        /// line.
        /// </summary>
        public TimerInput Input { get; private set; }

        /// <summary>
        /// Gets a user-specified title for the timer.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the timer window should always be displayed on top of other windows.
        /// </summary>
        public bool AlwaysOnTop { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the timer window should be in full-screen mode.
        /// </summary>
        public bool IsFullScreen { get; private set; }

        /// <summary>
        /// Gets a value indicating whether an icon for the app should be visible in the notification area of the
        /// taskbar.
        /// </summary>
        public bool ShowInNotificationArea { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to loop the timer continuously.
        /// </summary>
        public bool LoopTimer { get; private set; }

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
        /// Gets the color of the timer progress bar.
        /// </summary>
        public TimerColor Color { get; private set; }

        /// <summary>
        /// Gets the sound to play when the timer expires, or <c>null</c> if no sound is to be played.
        /// </summary>
        public Sound Sound { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the sound that plays when the timer expires should be looped until stopped
        /// by the user.
        /// </summary>
        public bool LoopSound { get; private set; }

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

        #endregion

        #region Public Methods

        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A <see cref="CommandLineParseResult"/>.</returns>
        public static CommandLineParseResult Parse(IList<string> args)
        {
            if (ContainsHelpSwitch(args))
            {
                return CommandLineParseResult.ForShowUsage();
            }

            try
            {
                CommandLineArguments arguments = GetCommandLineArguments(args);
                return CommandLineParseResult.ForCommandLineArguments(arguments);
            }
            catch (ParseException e)
            {
                return CommandLineParseResult.ForException(e);
            }
        }

        /// <summary>
        /// Shows the command-line usage of this application in a window.
        /// </summary>
        /// <param name="errorMessage">An error message to display. (Optional.)</param>
        public static void ShowUsage(string errorMessage = null)
        {
            UsageWindow window = new UsageWindow();
            window.ErrorMessage = errorMessage;

            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                window.Show();
            }
            else
            {
                window.ShowDialog();
            }
        }

        /// <summary>
        /// Returns a <see cref="TimerOptions"/> instance based on the parsed command-line arguments.
        /// </summary>
        /// <returns>A <see cref="TimerOptions"/> instance based on the parsed command-line arguments.</returns>
        public TimerOptions ToTimerOptions()
        {
            return new TimerOptions
            {
                Title = this.Title,
                AlwaysOnTop = this.AlwaysOnTop,
                LoopTimer = this.LoopTimer,
                PopUpWhenExpired = this.PopUpWhenExpired,
                CloseWhenExpired = this.CloseWhenExpired,
                Color = this.Color,
                Sound = this.Sound,
                LoopSound = this.LoopSound,
                WindowSize = this.ToWindowSize()
            };
        }

        /// <summary>
        /// Returns a <see cref="WindowSize"/> instance based on the parsed command-line arguments.
        /// </summary>
        /// <returns>A <see cref="WindowSize"/> instance based on the parsed command-line arguments.</returns>
        public WindowSize ToWindowSize()
        {
            return new WindowSize(
                this.WindowBounds,
                this.WindowState,
                this.RestoreWindowState,
                this.IsFullScreen);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns an <see cref="CommandLineArguments"/> instance based on the most recent options.
        /// </summary>
        /// <returns>An <see cref="CommandLineArguments"/> instance based on the most recent options.</returns>
        private static CommandLineArguments GetArgumentsFromMostRecentOptions()
        {
            TimerOptions options = TimerOptionsManager.Instance.MostRecentOptions;
            WindowSize windowSize = GetMostRecentWindowSize();

            return new CommandLineArguments
            {
                Title = null,
                AlwaysOnTop = options.AlwaysOnTop,
                IsFullScreen = windowSize.IsFullScreen,
                ShowInNotificationArea = Settings.Default.ShowInNotificationArea,
                LoopTimer = options.LoopTimer,
                PopUpWhenExpired = options.PopUpWhenExpired,
                CloseWhenExpired = options.CloseWhenExpired,
                Color = options.Color,
                Sound = options.Sound,
                LoopSound = options.LoopSound,
                WindowState = windowSize.WindowState,
                RestoreWindowState = windowSize.RestoreWindowState,
                WindowBounds = windowSize.RestoreBounds
            };
        }

        /// <summary>
        /// Returns an <see cref="CommandLineArguments"/> instance based on the factory default settings.
        /// </summary>
        /// <returns>An <see cref="CommandLineArguments"/> instance based on the factory default settings.</returns>
        private static CommandLineArguments GetArgumentsFromFactoryDefaults()
        {
            WindowSize mostRecentWindowSize = GetMostRecentWindowSize();
            Rect defaultWindowBounds = new Rect(double.PositiveInfinity, double.PositiveInfinity, 350, 150);
            Rect defaultWindowBoundsWithLocation = mostRecentWindowSize.RestoreBounds.Merge(defaultWindowBounds);

            return new CommandLineArguments
            {
                Title = null,
                AlwaysOnTop = false,
                IsFullScreen = false,
                ShowInNotificationArea = false,
                LoopTimer = false,
                PopUpWhenExpired = true,
                CloseWhenExpired = false,
                Color = TimerColor.DefaultColor,
                Sound = Sound.DefaultSound,
                LoopSound = false,
                WindowState = WindowState.Normal,
                RestoreWindowState = WindowState.Normal,
                WindowBounds = defaultWindowBoundsWithLocation
            };
        }

        /// <summary>
        /// Returns the most recent <see cref="WindowSize"/>.
        /// </summary>
        /// <returns>The most recent <see cref="WindowSize"/>.</returns>
        private static WindowSize GetMostRecentWindowSize()
        {
            WindowSize windowSizeFromSettings = WindowSize.FromWindowSizeInfo(Settings.Default.WindowSize);
            WindowSize windowSizeFromSibling = WindowSize.FromWindowOfType<TimerWindow>().Offset();
            return windowSizeFromSibling ?? windowSizeFromSettings ?? new WindowSize();
        }

        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>The parsed command-line arguments.</returns>
        /// <exception cref="Exception">If the command-line arguments could not be parsed.</exception>
        private static CommandLineArguments GetCommandLineArguments(IEnumerable<string> args)
        {
            CommandLineArguments argumentsBasedOnMostRecentOptions = GetArgumentsFromMostRecentOptions();
            CommandLineArguments argumentsBasedOnFactoryDefaults = GetArgumentsFromFactoryDefaults();
            bool useFactoryDefaults = false;

            HashSet<string> specifiedSwitches = new HashSet<string>();
            Queue<string> remainingArgs = new Queue<string>(args);
            while (remainingArgs.Count > 0)
            {
                string arg = remainingArgs.Dequeue();

                switch (arg)
                {
                    case "--title":
                    case "-t":
                        ThrowIfDuplicateSwitch(specifiedSwitches, "--title");

                        string title = GetRequiredValue(arg, remainingArgs);

                        argumentsBasedOnMostRecentOptions.Title = title;
                        argumentsBasedOnFactoryDefaults.Title = title;
                        break;

                    case "--always-on-top":
                    case "-a":
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
                        ThrowIfDuplicateSwitch(specifiedSwitches, "--full-screen");

                        bool isFullScreen = GetBoolValue(
                            arg,
                            remainingArgs,
                            argumentsBasedOnMostRecentOptions.IsFullScreen);

                        argumentsBasedOnMostRecentOptions.IsFullScreen = isFullScreen;
                        argumentsBasedOnFactoryDefaults.IsFullScreen = isFullScreen;
                        break;

                    case "--show-in-notification-area":
                    case "-n":
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
                        ThrowIfDuplicateSwitch(specifiedSwitches, "--loop-timer");

                        bool loopTimer = GetBoolValue(
                            arg,
                            remainingArgs,
                            argumentsBasedOnMostRecentOptions.LoopTimer);

                        argumentsBasedOnMostRecentOptions.LoopTimer = loopTimer;
                        argumentsBasedOnFactoryDefaults.LoopTimer = loopTimer;
                        break;

                    case "--pop-up-when-expired":
                    case "-p":
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
                        ThrowIfDuplicateSwitch(specifiedSwitches, "--close-when-expired");

                        bool closeWhenExpired = GetBoolValue(
                            arg,
                            remainingArgs,
                            argumentsBasedOnMostRecentOptions.CloseWhenExpired);

                        argumentsBasedOnMostRecentOptions.CloseWhenExpired = closeWhenExpired;
                        argumentsBasedOnFactoryDefaults.CloseWhenExpired = closeWhenExpired;
                        break;

                    case "--color":
                    case "-c":
                        ThrowIfDuplicateSwitch(specifiedSwitches, "--color");

                        TimerColor color = GetTimerColorValue(
                            arg,
                            remainingArgs,
                            argumentsBasedOnMostRecentOptions.Color);

                        argumentsBasedOnMostRecentOptions.Color = color;
                        argumentsBasedOnFactoryDefaults.Color = color;
                        break;

                    case "--sound":
                    case "-s":
                        ThrowIfDuplicateSwitch(specifiedSwitches, "--sound");

                        Sound sound = GetSoundValue(
                            arg,
                            remainingArgs,
                            argumentsBasedOnMostRecentOptions.Sound);

                        argumentsBasedOnMostRecentOptions.Sound = sound;
                        argumentsBasedOnFactoryDefaults.Sound = sound;
                        break;

                    case "--loop-sound":
                    case "-r":
                        ThrowIfDuplicateSwitch(specifiedSwitches, "--loop-sound");

                        bool loopSound = GetBoolValue(
                            arg,
                            remainingArgs,
                            argumentsBasedOnMostRecentOptions.LoopSound);

                        argumentsBasedOnMostRecentOptions.LoopSound = loopSound;
                        argumentsBasedOnFactoryDefaults.LoopSound = loopSound;
                        break;

                    case "--window-bounds":
                    case "-b":
                        ThrowIfDuplicateSwitch(specifiedSwitches, "--window-bounds");

                        Rect windowBounds = GetRectValue(
                            arg,
                            remainingArgs,
                            argumentsBasedOnMostRecentOptions.WindowBounds);

                        argumentsBasedOnMostRecentOptions.WindowBounds = argumentsBasedOnMostRecentOptions.WindowBounds.Merge(windowBounds);
                        argumentsBasedOnFactoryDefaults.WindowBounds = argumentsBasedOnFactoryDefaults.WindowBounds.Merge(windowBounds);
                        break;

                    case "--window-state":
                    case "-w":
                        ThrowIfDuplicateSwitch(specifiedSwitches, "--window-state");

                        WindowState windowState = GetWindowStateValue(
                            arg,
                            remainingArgs,
                            argumentsBasedOnMostRecentOptions.WindowState);

                        argumentsBasedOnMostRecentOptions.WindowState = windowState;
                        argumentsBasedOnFactoryDefaults.WindowState = windowState;
                        break;

                    case "--use-factory-defaults":
                    case "-d":
                        ThrowIfDuplicateSwitch(specifiedSwitches, "--use-factory-defaults");

                        useFactoryDefaults = true;
                        break;

                    default:
                        if (IsSwitch(arg))
                        {
                            string message = string.Format("Unrecognized switch \"{0}\".", arg);
                            throw new ParseException(message);
                        }

                        List<string> inputArgs = remainingArgs.ToList();
                        inputArgs.Insert(0, arg);
                        remainingArgs.Clear();

                        TimerInput input = GetTimerInputValue(inputArgs);

                        argumentsBasedOnMostRecentOptions.Input = input;
                        argumentsBasedOnFactoryDefaults.Input = input;
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
        private static string GetValue(Queue<string> remainingArgs)
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
        /// <exception cref="Exception">If <paramref name="remainingArgs"/> is empty or the next argument in <paramref
        /// name="remainingArgs"/> is a switch.</exception>
        private static string GetRequiredValue(string arg, Queue<string> remainingArgs)
        {
            string value = GetValue(remainingArgs);

            if (value == null)
            {
                string message = string.Format("Missing value for switch \"{0}\".", arg);
                throw new ParseException(message);
            }

            return value;
        }

        /// <summary>
        /// Returns the next <see cref="bool"/> value in <paramref name="remainingArgs"/>, or throws an exception if
        /// <paramref name="remainingArgs"/> is empty or the next argument is not "on", "off", or "last".
        /// </summary>
        /// <param name="arg">The name of the argument for which the value is to be returned.</param>
        /// <param name="remainingArgs">The unparsed arguments.</param>
        /// <param name="last">The value of the argument returned when the user specifies "last".</param>
        /// <returns>The next <see cref="bool"/> value in <paramref name="remainingArgs"/>.</returns>
        /// <exception cref="Exception">If <paramref name="remainingArgs"/> is empty or the next argument is not "on",
        /// "off", or "last".</exception>
        private static bool GetBoolValue(string arg, Queue<string> remainingArgs, bool last)
        {
            string value = GetRequiredValue(arg, remainingArgs);

            switch (value)
            {
                case "on":
                    return true;

                case "off":
                    return false;

                case "last":
                    return last;

                default:
                    string message = string.Format("Invalid value \"{1}\" for switch \"{0}\".", arg, value);
                    throw new ParseException(message);
            }
        }

        /// <summary>
        /// Returns the next <see cref="TimerColor"/> value in <paramref name="remainingArgs"/>, or throws an exception
        /// if <paramref name="remainingArgs"/> is empty or the next argument is not "last" or a valid representation
        /// of a <see cref="TimerColor"/>.
        /// </summary>
        /// <param name="arg">The name of the argument for which the value is to be returned.</param>
        /// <param name="remainingArgs">The unparsed arguments.</param>
        /// <param name="last">The value of the argument returned when the user specifies "last".</param>
        /// <returns>The next <see cref="TimerColor"/> value in <paramref name="remainingArgs"/></returns>
        /// <exception cref="Exception">If <paramref name="remainingArgs"/> is empty or the next argument is not "last"
        /// or a valid representation of a <see cref="TimerColor"/>.</exception>
        private static TimerColor GetTimerColorValue(string arg, Queue<string> remainingArgs, TimerColor last)
        {
            string value = GetRequiredValue(arg, remainingArgs);

            switch (value)
            {
                case "last":
                    return last;

                default:
                    TimerColor color = TimerColorManager.Instance.TryGetColorByName(value, true /* isBuiltIn */);
                    if (color != null)
                    {
                        return color;
                    }

                    try
                    {
                        return new TimerColor(value);
                    }
                    catch
                    {
                        string message = string.Format("Invalid value \"{1}\" for switch \"{0}\".", arg, value);
                        throw new ParseException(message);
                    }
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
        /// <exception cref="Exception">if <paramref name="remainingArgs"/> is empty or the next argument is not
        /// "none", "last", or a valid representation of a <see cref="Sound"/>.</exception>
        private static Sound GetSoundValue(string arg, Queue<string> remainingArgs, Sound last)
        {
            string value = GetRequiredValue(arg, remainingArgs);

            switch (value)
            {
                case "none":
                    return null;

                case "last":
                    return last;

                default:
                    Sound sound = SoundManager.Instance.GetSoundByName(value);

                    if (sound == null)
                    {
                        string message = string.Format("Invalid value \"{1}\" for switch \"{0}\".", arg, value);
                        throw new ParseException(message);
                    }

                    return sound;
            }
        }

        /// <summary>
        /// Returns the next <see cref="WindowState"/> value in <paramref name="remainingArgs"/>, or throws an
        /// exception if <paramref name="remainingArgs"/> is empty or the next argument is not "normal", "maximized",
        /// "minimized", or "last".
        /// </summary>
        /// <param name="arg">The name of the argument for which the value is to be returned.</param>
        /// <param name="remainingArgs">The unparsed arguments.</param>
        /// <param name="last">The value of the argument returned when the user specifies "last".</param>
        /// <returns>The next <see cref="WindowState"/> value in <paramref name="remainingArgs"/>.</returns>
        /// <exception cref="Exception">If <paramref name="remainingArgs"/> is empty or the next argument is not
        /// "normal", "maximized", "minimized", or "last".</exception>
        private static WindowState GetWindowStateValue(string arg, Queue<string> remainingArgs, WindowState last)
        {
            string value = GetRequiredValue(arg, remainingArgs);

            switch (value)
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
                    string message = string.Format("Invalid value \"{1}\" for switch \"{0}\".", arg, value);
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
        /// <exception cref="Exception">If <paramref name="remainingArgs"/> is empty or the next argument is not a
        /// valid representation of a <see cref="Rect"/>.</exception>
        private static Rect GetRectValue(string arg, Queue<string> remainingArgs, Rect last)
        {
            string value = GetRequiredValue(arg, remainingArgs);

            try
            {
                if (value == "last")
                {
                    return last;
                }

                string adjustedValue = Regex.Replace(value, @"\bauto\b", @"Infinity");
                return Rect.Parse(adjustedValue);
            }
            catch
            {
                string message = string.Format("Invalid value \"{1}\" for switch \"{0}\".", arg, value);
                throw new ParseException(message);
            }
        }

        /// <summary>
        /// Returns the <see cref="TimerInput"/> value corresponding to the concatenation of all <paramref
        /// name="remainingArgs"/>, or throws an exception if the concatenation of all <paramref name="remainingArgs"/>
        /// is not a valid representation of a <see cref="TimerInput"/>.
        /// </summary>
        /// <param name="remainingArgs">The unparsed arguments.</param>
        /// <returns>The <see cref="TimerInput"/> value corresponding to the concatenation of all <paramref
        /// name="remainingArgs"/></returns>
        /// <exception cref="Exception">If the concatenation of all <paramref name="remainingArgs"/> is not a valid
        /// representation of a <see cref="TimerInput"/>.</exception>
        private static TimerInput GetTimerInputValue(IEnumerable<string> remainingArgs)
        {
            string value = string.Join(" ", remainingArgs);
            TimerInput input = TimerInput.FromString(value);

            if (input == null)
            {
                string message = string.Format("Invalid timer input \"{0}\".", value);
                throw new ParseException(message);
            }

            return input;
        }

        /// <summary>
        /// Returns a value indicating whether the command-line arguments include the help switch.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A value indicating whether the command-line arguments include the help switch.</returns>
        private static bool ContainsHelpSwitch(IList<string> args)
        {
            return args.Contains("--help") || args.Contains("-h") || args.Contains("-?");
        }

        /// <summary>
        /// Returns a value indicating whether a string is a command-line switch.
        /// </summary>
        /// <param name="arg">A string.</param>
        /// <returns>A value indicating whether <paramref name="arg"/> is a command-line switch.</returns>
        private static bool IsSwitch(string arg)
        {
            return arg.StartsWith("-");
        }

        /// <summary>
        /// Unescapes a command-line value.
        /// </summary>
        /// <remarks>
        /// A value is any command-line argument not beginning with '-'. If the user must specify a command-line value
        /// that begins with '-', the user must escape the '-' with '''.
        /// </remarks>
        /// <param name="value">An escaped value string.</param>
        /// <returns>The unescaped value.</returns>
        private static string UnescapeValue(string value)
        {
            return !value.StartsWith("'") ? value : value.Substring(1);
        }

        /// <summary>
        /// Throws an exception if <paramref name="canonicalSwitch"/> is already in <paramref name="specifiedSwitches"/>,
        /// or otherwise adds <paramref name="canonicalSwitch"/> to <paramref name="specifiedSwitches"/>.
        /// </summary>
        /// <param name="specifiedSwitches">The switch arguments that are already specified.</param>
        /// <param name="canonicalSwitch">The canonical representation of a switch argument.</param>
        private static void ThrowIfDuplicateSwitch(HashSet<string> specifiedSwitches, string canonicalSwitch)
        {
            if (!specifiedSwitches.Add(canonicalSwitch))
            {
                string message = string.Format("Duplicate argument \"{0}\".", canonicalSwitch);
                throw new ParseException(message);
            }
        }

        #endregion

        #region Classes

        /// <summary>
        /// Represents an error during <see cref="CommandLineArguments.GetCommandLineArguments"/>.
        /// </summary>
        private class ParseException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ParseException"/> class.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            public ParseException(string message)
                : base(message)
            {
            }
        }

        #endregion
    }
}