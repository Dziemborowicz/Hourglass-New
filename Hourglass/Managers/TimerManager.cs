// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerManager.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Properties;
using Timing;
using Windows;

/// <summary>
/// Manages timers.
/// </summary>
public sealed class TimerManager : Manager
{
    /// <summary>
    /// The maximum number of timers to persist in settings.
    /// </summary>
    private const int MaxSavedTimers = 30;

    /// <summary>
    /// Singleton instance of the <see cref="TimerManager"/> class.
    /// </summary>
    public static readonly TimerManager Instance = new();

    /// <summary>
    /// The currently loaded timers in reverse chronological order.
    /// </summary>
    private readonly List<Timer> _timers = [];

    /// <summary>
    /// Prevents a default instance of the <see cref="TimerManager"/> class from being created.
    /// </summary>
    private TimerManager()
    {
    }

    /// <summary>
    /// Gets the silent mode when all the timers notifications are postponed.
    /// </summary>
    public bool SilentMode
    {
        get => field && Settings.Default.ShowInNotificationArea;
        private set;
    }

    /// <summary>
    /// Gets a list of the currently loaded timers.
    /// </summary>
    public IList<Timer> Timers => _timers.AsReadOnly();

    /// <summary>
    /// Gets a list of the currently loaded timers that are not bound to any <see cref="TimerWindow"/> and are not
    /// <see cref="TimerState.Stopped"/>.
    /// </summary>
#pragma warning disable S2365
    public IReadOnlyCollection<Timer> ResumableTimers => _timers.Where(static t => t is { ShouldBeSaved: true, State: not TimerState.Stopped } && !IsBoundToWindow(t)).ToArray();
#pragma warning restore S2365

    /// <summary>
    /// Gets a list of the currently loaded timers that are bound to any <see cref="TimerWindow"/> and are <see
    /// cref="TimerState.Running"/>.
    /// </summary>
#pragma warning disable S2365
    public IReadOnlyCollection<Timer> RunningTimers => _timers.Where(static t => t.State == TimerState.Running && IsBoundToWindow(t)).ToArray();
#pragma warning restore S2365

    /// <summary>
    /// Initializes the class.
    /// </summary>
    public override void Initialize()
    {
        _timers.Clear();
        _timers.AddRange(Settings.Default.Timers);
    }

    /// <summary>
    /// Persists the state of the class.
    /// </summary>
    public override void Persist()
    {
        Settings.Default.Timers = _timers
            .Where(static t => t is { ShouldBeSaved: true, State: not TimerState.Stopped and not TimerState.Expired })
            .Where(static t => !t.Options.LockInterface)
            .Take(MaxSavedTimers)
            .ToList();
    }

    /// <summary>
    /// Add a new timer.
    /// </summary>
    /// <param name="timer">A <see cref="Timer"/>.</param>
    /// <exception cref="InvalidOperationException">If the <see cref="Timer"/> has already been added.</exception>
    public void Add(Timer timer)
    {
        if (_timers.Contains(timer))
        {
            throw new InvalidOperationException();
        }

        _timers.Insert(0, timer);
    }

    /// <summary>
    /// Removes a timer.
    /// </summary>
    /// <param name="timer">A <see cref="Timer"/>.</param>
    public void Remove(Timer timer)
    {
        _timers.Remove(timer);
    }

    /// <summary>
    /// Stops and removes an existing timer.
    /// </summary>
    /// <param name="timer">A <see cref="Timer"/>.</param>
    /// <exception cref="InvalidOperationException">If the timer had not been added previously or has already been removed.</exception>
    public void StopAndRemove(Timer timer)
    {
        timer.Stop();

        if (!_timers.Remove(timer))
        {
            throw new InvalidOperationException();
        }

        timer.Dispose();
    }

    /// <summary>
    /// Stops and removes existing timers.
    /// </summary>
    /// <param name="collection">A collection of timers to remove.</param>
    /// <exception cref="InvalidOperationException">If the timer had not been added previously or has already been removed.</exception>
    public void StopAndRemove(IEnumerable<Timer> collection)
    {
        foreach (Timer timer in collection)
        {
            StopAndRemove(timer);
        }
    }

    /// <summary>
    /// Stops and removes the <see cref="ResumableTimers"/>.
    /// </summary>
    public void StopAndRemoveResumableTimers()
    {
        StopAndRemove(ResumableTimers);
    }

    /// <summary>
    /// Checks whether at least one timer can be paused.
    /// </summary>
    /// <returns><c>true</c> if at least one timer can be paused.</returns>
    public static bool CanPauseAll() =>
        GetPausableTimers(TimerState.Running).Any();

    /// <summary>
    /// Checks whether at least one timer can be resumed.
    /// </summary>
    /// <returns><c>true</c> if at least one timer can be resumed.</returns>
    public static bool CanResumeAll() =>
        GetPausableTimers(TimerState.Paused).Any();

    /// <summary>
    /// Toggles the silent mode.
    /// </summary>
    public void ToggleSilentMode()
    {
        SilentMode = !SilentMode;

        if (SilentMode)
        {
            return;
        }

        foreach (TimerWindow timerWindow in GetTimersByState(TimerState.Expired))
        {
            timerWindow.BeginExpirationAnimationAndSound();
        }
    }

    /// <summary>
    /// Pauses all the running timers.
    /// </summary>
    public static void PauseAll() =>
        ForEachPausableTimer(TimerState.Running, static timer => timer.Pause());

    /// <summary>
    /// Resumes all the running timers.
    /// </summary>
    public static void ResumeAll() =>
        ForEachPausableTimer(TimerState.Paused, static timer => timer.Resume());

    /// <summary>
    /// Gets all pausable timers.
    /// </summary>
    /// <param name="state">Timer state.</param>
    /// <returns>Pausable timers in <paramref name="state"/> specified.</returns>
    public static IEnumerable<TimerWindow> GetPausableTimers(TimerState state) =>
        GetTimersByState(state)
            .Where(timerWindow => timerWindow.Timer.SupportsPause && !timerWindow.Options.LockInterface);

    /// <summary>
    /// Gets timers by state.
    /// </summary>
    /// <param name="state">Timer state.</param>
    /// <returns>Timers in <paramref name="state"/> specified.</returns>
    public static IEnumerable<TimerWindow> GetTimersByState(TimerState state) =>
        Application.Current?.Windows.OfType<TimerWindow>()
            .Where(timerWindow => timerWindow.Timer.State == state)
        ?? [];

    private static void ForEachPausableTimer(TimerState state, Action<Timer> execute)
    {
        foreach (TimerWindow timerWindow in GetPausableTimers(state))
        {
            execute(timerWindow.Timer);
        }
    }

    /// <summary>
    /// Returns a value indicating whether a timer is bound to any <see cref="TimerWindow"/>.</summary>
    /// <param name="timer">A <see cref="Timer"/>.</param>
    /// <returns>A value indicating whether the timer is bound to any <see cref="TimerWindow"/>.</returns>
    private static bool IsBoundToWindow(Timer timer)
    {
        return Application.Current?.Windows.OfType<TimerWindow>().Any(w => ReferenceEquals(w.Timer, timer)) == true;
    }
}