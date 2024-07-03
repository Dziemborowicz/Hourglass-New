// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Timer.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Timing;

using System;
using System.Globalization;

using Extensions;
using Properties;
using Serialization;

// ReSharper disable ExceptionNotDocumented

/// <summary>
/// A countdown timer.
/// </summary>
public sealed class Timer : TimerBase
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Timer"/> class.
    /// </summary>
    public Timer()
        : this(new TimerOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Timer"/> class.
    /// </summary>
    /// <param name="options">Configuration data for this timer.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/></exception>
    public Timer(TimerOptions? options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        TimerStart = null;
        Options = TimerOptions.FromTimerOptions(options)!;

        UpdateHourglassTimer();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Timer"/> class.
    /// </summary>
    /// <param name="timerInfo">A <see cref="TimerInfo"/> representing the state of the <see
    /// cref="Timer"/>.</param>
    public Timer(TimerInfo timerInfo)
        : base(timerInfo)
    {
        TimerStart = TimerStart.FromTimerStartInfo(timerInfo.TimerStart);
        Options = TimerOptions.FromTimerOptionsInfo(timerInfo.Options) ?? new();

        UpdateHourglassTimer();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the configuration data for this timer.
    /// </summary>
    public TimerOptions Options { get; }

    /// <summary>
    /// Gets the <see cref="TimerStart"/> used to start this timer, or <c>null</c> if the <see
    /// cref="TimerBase.State"/> is <see cref="TimerState.Stopped"/>.
    /// </summary>
    public TimerStart? TimerStart { get; private set; }

    /// <summary>
    /// Gets the percentage of time left until the timer expires.
    /// </summary>
    /// <remarks>
    /// This property is <c>null</c> if <see cref="SupportsProgress"/> is <c>false</c>.
    /// </remarks>
    public double? TimeLeftAsPercentage { get; private set; }

    /// <summary>
    /// Gets the percentage of time elapsed since the timer was started.
    /// </summary>
    /// <remarks>
    /// This property is <c>null</c> if <see cref="SupportsProgress"/> or <see cref="SupportsTimeElapsed"/> is
    /// <c>false</c>.
    /// </remarks>
    public double? TimeElapsedAsPercentage { get; private set; }

    /// <summary>
    /// Gets the string representation of the time left until the timer expires.
    /// </summary>
    public string? TimeLeftAsString { get; private set; }

    /// <summary>
    /// Gets the string representation of the time elapsed since the timer was started.
    /// </summary>
    /// <remarks>
    /// This property is <c>null</c> if <see cref="SupportsTimeElapsed"/> is <c>false</c>.
    /// </remarks>
    public string? TimeElapsedAsString { get; private set; }

    /// <summary>
    /// Gets the string representation of the time since the timer expired.
    /// </summary>
    public string? TimeExpiredAsString { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the timer supports pause.
    /// </summary>
    public bool SupportsPause => TimerStart is null || TimerStart.Type == TimerStartType.TimeSpan;

    /// <summary>
    /// Gets a value indicating whether the timer supports looping.
    /// </summary>
    public bool SupportsLooping => TimerStart is null || TimerStart.Type == TimerStartType.TimeSpan;

    /// <summary>
    /// Gets a value indicating whether the timer supports displaying a progress value.
    /// </summary>
    public bool SupportsProgress => true;

    /// <summary>
    /// Gets a value indicating whether the timer supports restarting.
    /// </summary>
    public bool SupportsRestart => TimerStart?.Type == TimerStartType.TimeSpan;

    /// <summary>
    /// Gets a value indicating whether the timer supports displaying the elapsed time since the timer was started.
    /// </summary>
    public bool SupportsTimeElapsed => true;

    #endregion

    /// <summary>
    /// Returns a <see cref="Timer"/> for a <see cref="TimerInfo"/>.
    /// </summary>
    /// <param name="timerInfo">A <see cref="TimerInfo"/>.</param>
    /// <returns>The <see cref="Timer"/> for the <see cref="TimerInfo"/>.</returns>
    public static Timer? FromTimerInfo(TimerInfo? timerInfo) => timerInfo is null ? null : new(timerInfo);

    #region Public Methods

    /// <summary>
    /// Starts the timer.
    /// </summary>
    /// <param name="newTimerStart">A <see cref="TimerStart"/>.</param>
    /// <returns>A value indicating whether the timer was started successfully.</returns>
    /// <exception cref="ObjectDisposedException">If the timer has been disposed.</exception>
    public bool Start(TimerStart newTimerStart)
    {
        ThrowIfDisposed();

        DateTime start = DateTime.Now;
        if (newTimerStart.TryGetEndTime(start, out var end))
        {
            TimerStart = newTimerStart;
            OnPropertyChanged(nameof(TimerStart));

            Start(start, end);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Restarts the timer.
    /// </summary>
    /// <returns>A value indicating whether the timer was restarted successfully.</returns>
    /// <exception cref="ObjectDisposedException">If the timer has been disposed.</exception>
    public bool Restart()
    {
        ThrowIfDisposed();

        TimerStart? actualTimerStart = TimerStart;
        if (actualTimerStart?.Type == TimerStartType.TimeSpan)
        {
            Stop();
            return Start(actualTimerStart);
        }

        return false;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        string resourceName = string.Format(
            CultureInfo.InvariantCulture,
            "Timer{0}{1}{2}FormatString",
            State,
            string.IsNullOrWhiteSpace(Options.Title) ? string.Empty : Resources.WithTitleText,
            Options.LoopTimer && SupportsLooping ? Resources.LoopedText : string.Empty);

        return string.Format(
            Resources.ResourceManager.GetEffectiveProvider(),
            Resources.ResourceManager.GetString(resourceName) ?? GetType().ToString(),
            Options.ShowTimeElapsed ? TimeElapsed.ToNaturalString(Options.DigitalClockTime) : TimeLeft.RoundUp().ToNaturalString(Options.DigitalClockTime),
            TimerStart,
            Options.Title);
    }

    /// <summary>
    /// Returns the representation of the <see cref="TimerInfo"/> used for XML serialization.
    /// </summary>
    /// <returns>The representation of the <see cref="TimerInfo"/> used for XML serialization.</returns>
    public override TimerInfo ToTimerInfo()
    {
        TimerInfo timerInfo = base.ToTimerInfo();
        timerInfo.TimerStart = TimerStartInfo.FromTimerStart(TimerStart);
        timerInfo.Options = TimerOptionsInfo.FromTimerOptions(Options);
        return timerInfo;
    }

    #endregion

    #region Protected Methods (Events)

    /// <summary>
    /// Invoked before the <see cref="TimerBase.Started"/> event is raised
    /// </summary>
    protected override void OnStarted()
    {
        ResetLastEndTime();

        UpdateHourglassTimer();
        base.OnStarted();
    }

    /// <summary>
    /// Invoked before the <see cref="TimerBase.Paused"/> event is raised
    /// </summary>
    protected override void OnPaused()
    {
        ResetLastEndTime();

        UpdateHourglassTimer();
        base.OnPaused();
    }

    /// <summary>
    /// Invoked before the <see cref="TimerBase.Resumed"/> event is raised
    /// </summary>
    protected override void OnResumed()
    {
        ResetLastEndTime();

        UpdateHourglassTimer();
        base.OnResumed();
    }

    /// <summary>
    /// Invoked before the <see cref="TimerBase.Stopped"/> event is raised
    /// </summary>
    protected override void OnStopped()
    {
        ResetLastEndTime();

        UpdateHourglassTimer();
        base.OnStopped();
    }

    /// <summary>
    /// Invoked before the <see cref="TimerBase.Expired"/> event is raised
    /// </summary>
    protected override void OnExpired()
    {
        ResetLastEndTime();

        UpdateHourglassTimer();
        base.OnExpired();

        if (Options.LoopTimer && SupportsLooping && State != TimerState.Stopped)
        {
            Loop();
        }
    }

    /// <summary>
    /// Invoked before the <see cref="TimerBase.Tick"/> event is raised
    /// </summary>
    protected override void OnTick()
    {
        UpdateHourglassTimer();
        base.OnTick();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Restarts the timer with the current <see cref="TimerStart"/>.
    /// </summary>
    private void Loop()
    {
        if (!EndTime.HasValue || EndTime > DateTime.Now)
        {
            throw new InvalidOperationException();
        }

        DateTime now = DateTime.Now;
        DateTime start = EndTime.Value;
        DateTime end = start;

        // Try to find the current loop iteration
        int iteration = 0;
        while (TimerStart?.TryGetEndTime(start, out end) == true && end <= now && end > start && iteration++ < 10000)
        {
            // Keep looping
            start = end;
        }

        // Loop if we found the current loop iteration
        if (end > now)
        {
            Start(start, end);
        }
    }

    /// <summary>
    /// Updates the <see cref="Timer"/> state.
    /// </summary>
    private void UpdateHourglassTimer()
    {
        TimerStart = State != TimerState.Stopped ? TimerStart : null;
        TimeLeftAsPercentage = GetTimeLeftAsPercentage();
        TimeElapsedAsPercentage = GetTimeElapsedAsPercentage();
        TimeLeftAsString = GetTimeLeftAsString(Options.DigitalClockTime);
        TimeElapsedAsString = GetTimeElapsedAsString(Options.DigitalClockTime);
        TimeExpiredAsString = GetTimeExpiredAsString(false); // Always human-readable.

        OnPropertyChanged(
            nameof(TimerStart),
            nameof(TimeLeftAsPercentage),
            nameof(TimeElapsedAsPercentage),
            nameof(TimeLeftAsString),
            nameof(TimeElapsedAsString),
            nameof(TimeExpiredAsString));
    }

    /// <summary>
    /// Returns the percentage of time left until the timer expires.
    /// </summary>
    /// <returns>The percentage of time left until the timer expires.</returns>
    private double? GetTimeLeftAsPercentage()
    {
        if (!SupportsProgress ||
            State == TimerState.Stopped ||
            !TimeElapsed.HasValue ||
            !TotalTime.HasValue)
        {
            return null;
        }

        if (State == TimerState.Expired)
        {
            return 100.0;
        }

        long timeElapsed = TimeElapsed.Value.Ticks;
        long totalTime = TotalTime.Value.Ticks;

        if (totalTime == 0)
        {
            return 0.0;
        }

        return 100.0 * timeElapsed / totalTime;
    }

    /// <summary>
    /// Returns the percentage of time elapsed since the timer was started.
    /// </summary>
    /// <returns>The percentage of time elapsed since the timer was started.</returns>
    private double? GetTimeElapsedAsPercentage()
    {
        if (!SupportsProgress ||
            !SupportsTimeElapsed ||
            State == TimerState.Stopped ||
            !TimeLeft.HasValue ||
            !TotalTime.HasValue)
        {
            return null;
        }

        if (State == TimerState.Expired)
        {
            return 0.0;
        }

        long timeLeft = TimeLeft.Value.Ticks;
        long totalTime = TotalTime.Value.Ticks;

        if (totalTime == 0)
        {
            return 100.0;
        }

        return 100.0 * timeLeft / totalTime;
    }

    /// <summary>
    /// Returns the string representation of the time left until the timer expires.
    /// </summary>
    /// <param name="compact">Use compact time format.</param>
    /// <returns>The string representation of the time left until the timer expires.</returns>
    private string GetTimeLeftAsString(bool compact) =>
        State switch
        {
            TimerState.Stopped => Resources.TimerTimerStopped,
            TimerState.Expired => Resources.TimerTimerExpired,
            _ => TimeLeft.RoundUp().ToNaturalString(compact)
        };

    /// <summary>
    /// Returns the string representation of the time elapsed since the timer was started.
    /// </summary>
    /// <param name="compact">Use compact time format.</param>
    /// <returns>The string representation of the time elapsed since the timer was started.</returns>
    private string? GetTimeElapsedAsString(bool compact) =>
        State switch
        {
            _ when !SupportsTimeElapsed => null,
            TimerState.Stopped => Resources.TimerTimerStopped,
            TimerState.Expired => Resources.TimerTimerExpired,
            _ => TimeElapsed.ToNaturalString(compact)
        };

    /// <summary>
    /// Returns the string representation of the time since the timer expired.
    /// </summary>
    /// <param name="compact">Use compact time format.</param>
    /// <returns>The string representation of the time since the timer expired.</returns>
    private string GetTimeExpiredAsString(bool compact) =>
        State != TimerState.Expired
            ? FormatEndTime()
            : string.Format(
                Resources.ResourceManager.GetEffectiveProvider(),
                Resources.TimerTimeExpiredFormatString,
                TimeExpired.ToNaturalString(compact));

    private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

    private EndTimeFormatState _endTimeFormatState;
    private string _endTimeFormatted = string.Empty;

    private void ResetLastEndTime()
    {
        _endTimeFormatState = EndTimeFormatState.Undefined;
        _endTimeFormatted = string.Empty;
    }

    private enum EndTimeFormatState
    {
        Undefined,
        Today,
        Tomorrow,
        Other
    }

    private string FormatEndTime()
    {
        if (EndTime is null)
        {
            return string.Empty;
        }

        DateTime endTime = EndTime.Value;
        DateTime now = DateTime.Now;

        var endTimeFormatState = true switch
        {
            _ when now.Date == endTime.Date =>
                EndTimeFormatState.Today,
            _ when (endTime - now).Duration() < OneDay =>
                EndTimeFormatState.Tomorrow,
            _ =>
                EndTimeFormatState.Other
        };

        if (_endTimeFormatState == endTimeFormatState)
        {
            return _endTimeFormatted;
        }

        _endTimeFormatState = endTimeFormatState;

        return _endTimeFormatted = endTimeFormatState switch
        {
            EndTimeFormatState.Today =>
                endTime.ToLongTimeString(),
            EndTimeFormatState.Tomorrow =>
                string.Format(Resources.TimerTimeTomorrowFormatString, endTime.ToLongTimeString()),
            _ =>
                string.Format(Resources.TimerTimeDateTimeFormatString, endTime.ToLongDateString(), endTime.ToLongTimeString()),
        };
    }

    #endregion
}