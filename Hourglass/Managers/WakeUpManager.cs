// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WakeUpManager.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Managers;

using System;
using System.Linq;

using Extensions;

using Microsoft.Win32;

/// <summary>
/// Schedules the computer to wake up when the next timer expires if the computer is not being kept awake while one
/// or more timers is running.
/// </summary>
public sealed class WakeUpManager : Manager
{
    /// <summary>
    /// Singleton instance of the <see cref="WakeUpManager"/> class.
    /// </summary>
    public static readonly WakeUpManager Instance = new();

    /// <summary>
    /// A handle to timer that will wake the computer, or <see cref="IntPtr.Zero"/> if no timer is set.
    /// </summary>
    private IntPtr _waitableTimer;

    /// <summary>
    /// Prevents a default instance of the <see cref="WakeUpManager"/> class from being created.
    /// </summary>
    private WakeUpManager()
    {
    }

    /// <summary>
    /// Initializes the class.
    /// </summary>
    public override void Initialize()
    {
        SystemEvents.PowerModeChanged += PowerModeChanged;
    }

    /// <summary>
    /// Disposes the manager.
    /// </summary>
    /// <param name="disposing">A value indicating whether this method was invoked by an explicit call to <see
    /// cref="Dispose"/>.</param>
    protected override void Dispose(bool disposing)
    {
        if (Disposed)
        {
            return;
        }

        if (disposing)
        {
            SystemEvents.PowerModeChanged -= PowerModeChanged;
        }

        CancelWaitableTimer();
    }

    /// <summary>
    /// Invoked when the user suspends or resumes the system.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event data.</param>
    private void PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Suspend)
        {
            CancelWaitableTimer();

            DateTime? nextExpiry = GetNextTimerExpiry();
            if (nextExpiry.HasValue)
            {
                DateTime wakeTime = nextExpiry.Value.AddSeconds(-15);
                wakeTime = MathExtensions.Max(wakeTime, DateTime.Now.AddSeconds(15));
                SetWaitableTimer(wakeTime);
            }
        }
        else if (e.Mode == PowerModes.Resume)
        {
            CancelWaitableTimer();
        }
    }

    /// <summary>
    /// Returns the next expiry time for any running timer, or <c>null</c> if there are no running timers.
    /// </summary>
    /// <returns>The next expiry time for any running timer, or <c>null</c> if there are no running timers.
    /// </returns>
    private static DateTime? GetNextTimerExpiry()
    {
        return TimerManager.Instance.RunningTimers
            .Where(static t => t.EndTime.HasValue)
            .Select(static t => t.EndTime!.Value)
            .OrderBy(static t => t)
            .FirstOrDefault();
    }

    /// <summary>
    /// Sets a timer to wake the computer at the specified date and time.
    /// </summary>
    /// <param name="dateTime">The date and time at which to wake the computer.</param>
    private void SetWaitableTimer(DateTime dateTime)
    {
        long dueTime = dateTime.ToFileTimeUtc();
        _waitableTimer = NativeMethods.CreateWaitableTimer(
            IntPtr.Zero /* lpTimerAttributes */,
            true /* bManualReset */,
            null /* lpTimerName */);
        NativeMethods.SetWaitableTimer(
            _waitableTimer,
            ref dueTime,
            0 /* lPeriod */,
            IntPtr.Zero /* pfnCompletionRoutine */,
            IntPtr.Zero /* lpArgToCompletionRoutine */,
            true /* fResume */);
    }

    /// <summary>
    /// Cancels the timer. If the timer has not been set or has already been cancelled, this method does nothing.
    /// </summary>
    private void CancelWaitableTimer()
    {
        if (_waitableTimer != IntPtr.Zero)
        {
            NativeMethods.CancelWaitableTimer(_waitableTimer);
            NativeMethods.CloseHandle(_waitableTimer);
            _waitableTimer = IntPtr.Zero;
        }
    }
}