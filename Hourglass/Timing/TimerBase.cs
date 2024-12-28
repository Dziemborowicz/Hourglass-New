// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerBase.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Timing;

using System;
using System.ComponentModel;
using System.Windows.Threading;

using Extensions;
using Serialization;

/// <summary>
/// The state of a timer.
/// </summary>
public enum TimerState
{
    /// <summary>
    /// Indicates that the timer is stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// Indicates that the timer is running.
    /// </summary>
    Running,

    /// <summary>
    /// Indicates that the timer is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// Indicates that the timer is expired.
    /// </summary>
    Expired
}

/// <summary>
/// A countdown timer.
/// </summary>
public abstract class TimerBase : IDisposable, INotifyPropertyChanged
{
    /// <summary>
    /// The default period of time between timer ticks.
    /// </summary>
    public static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// A <see cref="DispatcherTimer"/>.
    /// </summary>
    private readonly DispatcherTimer _dispatcherTimer;

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerBase"/> class.
    /// </summary>
    protected TimerBase()
    {
        _dispatcherTimer = new()
        {
            Interval = DefaultInterval
        };
        _dispatcherTimer.Tick += delegate { Update(); };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerBase"/> class.
    /// </summary>
    /// <param name="timerInfo">A <see cref="TimerInfo"/> representing the state of the <see cref="TimerBase"/>.</param>
    protected TimerBase(TimerInfo timerInfo)
        : this()
    {
        State = timerInfo.State;
        StartTime = timerInfo.StartTime;
        EndTime = timerInfo.EndTime;
        TimeElapsed = timerInfo.TimeElapsed;
        TimeLeft = timerInfo.TimeLeft;
        TimeExpired = timerInfo.TimeExpired;
        TotalTime = timerInfo.TotalTime;

        if (State == TimerState.Running)
        {
            _dispatcherTimer.Start();
        }
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when the timer is started.
    /// </summary>
    public event EventHandler? Started;

    /// <summary>
    /// Raised when the timer is paused.
    /// </summary>
    public event EventHandler? Paused;

    /// <summary>
    /// Raised when the timer is resumed from a paused state.
    /// </summary>
    public event EventHandler? Resumed;

    /// <summary>
    /// Raised when the timer is stopped.
    /// </summary>
    public event EventHandler? Stopped;

    /// <summary>
    /// Raised when the timer expires.
    /// </summary>
    public event EventHandler? Expired;

    /// <summary>
    /// Raised when the timer ticks.
    /// </summary>
    public event EventHandler? Tick;

    /// <summary>
    /// Raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the <see cref="TimerState"/> of this timer.
    /// </summary>
    public TimerState State { get; private set; } = TimerState.Stopped;

    /// <summary>
    /// Gets or sets value indicating whether this timer should be saved on closing.
    /// </summary>
    public bool ShouldBeSaved { get; set; } = true;

    /// <summary>
    /// Gets the <see cref="DateTime"/> that this timer was started if the <see cref="State"/> is <see
    /// cref="TimerState.Running"/> or <see cref="TimerState.Expired"/>, or <c>null</c> otherwise.
    /// </summary>
    public DateTime? StartTime { get; private set; }

    /// <summary>
    /// Gets the <see cref="DateTime"/> that this timer will expire or has expired if the <see cref="State"/> is
    /// <see cref="TimerState.Running"/> or <see cref="TimerState.Expired"/>, or <c>null</c> otherwise.
    /// </summary>
    public DateTime? EndTime { get; private set; }

    /// <summary>
    /// Gets a <see cref="TimeSpan"/> representing the time elapsed since this timer started if the <see
    /// cref="State"/> is <see cref="TimerState.Running"/>, <see cref="TimerState.Paused"/>, or <see
    /// cref="TimerState.Expired"/>, or <c>null</c> otherwise.
    /// </summary>
    public TimeSpan? TimeElapsed { get; private set; }

    /// <summary>
    /// Gets a <see cref="TimeSpan"/> representing the time left until this timer expires if the <see
    /// cref="State"/> is <see cref="TimerState.Running"/> or <see cref="TimerState.Paused"/>, <see
    /// cref="TimeSpan.Zero"/> if the <see cref="State"/> is <see cref="TimerState.Expired"/>, or <c>null</c>
    /// otherwise.
    /// </summary>
    public TimeSpan? TimeLeft { get; private set; }

    /// <summary>
    /// Gets a <see cref="TimeSpan"/> representing the time since this timer has expired if the <see cref="State"/>
    /// is <see cref="TimerState.Expired"/>, <see cref="TimeSpan.Zero"/> if the <see cref="State"/> is <see
    /// cref="TimerState.Running"/> or <see cref="TimerState.Paused"/>, or <c>null</c> otherwise.
    /// </summary>
    public TimeSpan? TimeExpired { get; private set; }

    /// <summary>
    /// Gets a <see cref="TimeSpan"/> representing the total time that this timer will run for or has run for if
    /// the <see cref="State"/> is <see cref="TimerState.Running"/> or <see cref="TimerState.Expired"/>, or
    /// <c>null</c> otherwise.
    /// </summary>
    public TimeSpan? TotalTime { get; private set; }

    /// <summary>
    /// Gets or sets the period of time between timer ticks.
    /// </summary>
    /// <seealso cref="Tick"/>
    public TimeSpan Interval
    {
        get => _dispatcherTimer.Interval;

        set
        {
            if (_dispatcherTimer.Interval == value)
            {
                return;
            }

            _dispatcherTimer.Interval = value;
            PropertyChanged.Notify(this);
        }
    }

    /// <summary>
    /// Gets a value indicating whether this object has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts the timer.
    /// </summary>
    /// <param name="start">The <see cref="DateTime"/> the timer was started.</param>
    /// <param name="end">The <see cref="DateTime"/> the timer expires.</param>
    protected void Start(DateTime start, DateTime end)
    {
        ThrowIfDisposed();

        State = TimerState.Running;
        StartTime = MathExtensions.Min(start, end);
        EndTime = end;
        TimeElapsed = TimeSpan.Zero;
        TimeLeft = EndTime - StartTime;
        TimeExpired = TimeSpan.Zero;
        TotalTime = TimeLeft;

        PropertyChanged.Notify(this,
            nameof(State),
            nameof(StartTime),
            nameof(EndTime),
            nameof(TimeElapsed),
            nameof(TimeLeft),
            nameof(TimeExpired),
            nameof(TotalTime));
        OnStarted();

        Update();
        _dispatcherTimer.Start();
    }

    /// <summary>
    /// Pauses the timer.
    /// </summary>
    /// <remarks>
    /// If the <see cref="State"/> is not <see cref="TimerState.Running"/>, this method does nothing.
    /// </remarks>
    public void Pause()
    {
        ThrowIfDisposed();

        if (State != TimerState.Running)
        {
            return;
        }

        DateTime now = DateTime.Now;
        State = TimerState.Paused;
        TimeElapsed = MathExtensions.Min(now - (StartTime ?? now), TotalTime ?? TimeSpan.Zero);
        TimeLeft = MathExtensions.Max((EndTime ?? now) - now, TimeSpan.Zero);
        TimeExpired = TimeSpan.Zero;
        StartTime = null;
        EndTime = null;

        _dispatcherTimer.Stop();

        PropertyChanged.Notify(this,
            nameof(State),
            nameof(StartTime),
            nameof(EndTime),
            nameof(TimeElapsed),
            nameof(TimeExpired),
            nameof(TimeLeft));
        OnPaused();
    }

    /// <summary>
    /// Resumes the timer if the timer is paused.
    /// </summary>
    /// <remarks>
    /// If the <see cref="State"/> is not <see cref="TimerState.Paused"/>, this method does nothing.
    /// </remarks>
    public void Resume()
    {
        ThrowIfDisposed();

        if (State != TimerState.Paused)
        {
            return;
        }

        State = TimerState.Running;
        EndTime = DateTime.Now + TimeLeft;
        StartTime = EndTime - TotalTime;

        PropertyChanged.Notify(this,
            nameof(State),
            nameof(StartTime),
            nameof(EndTime));
        OnResumed();

        Update();
        _dispatcherTimer.Start();
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    /// <remarks>
    /// If the <see cref="State"/> is <see cref="TimerState.Stopped"/>, this method does nothing.
    /// </remarks>
    public void Stop()
    {
        ThrowIfDisposed();

        if (State == TimerState.Stopped)
        {
            return;
        }

        State = TimerState.Stopped;
        StartTime = null;
        EndTime = null;
        TimeElapsed = null;
        TimeLeft = null;
        TimeExpired = null;
        TotalTime = null;

        _dispatcherTimer.Stop();

        PropertyChanged.Notify(this,
            nameof(State),
            nameof(StartTime),
            nameof(EndTime),
            nameof(TimeLeft),
            nameof(TimeExpired),
            nameof(TotalTime));
        OnStopped();
    }

    /// <summary>
    /// Updates the state of the timer.
    /// </summary>
    /// <remarks>
    /// When the timer is running, this method is periodically invoked to update the state of the timer.
    /// </remarks>
    public void Update()
    {
        ThrowIfDisposed();

        if (State != TimerState.Running && State != TimerState.Expired)
        {
            return;
        }

        // Update timer state
        DateTime now = DateTime.Now;
        TimeElapsed = MathExtensions.Min(now - (StartTime ?? now), TotalTime ?? TimeSpan.Zero);
        TimeLeft = MathExtensions.Max((EndTime ?? now) - now, TimeSpan.Zero);
        TimeExpired = MathExtensions.Max(now - (EndTime ?? now), TimeSpan.Zero);

        // Raise an event when the timer expires
        if (TimeLeft <= TimeSpan.Zero && State == TimerState.Running)
        {
            State = TimerState.Expired;

            PropertyChanged.Notify(this, nameof(State));
            OnExpired();
        }

        // Raise other events
        PropertyChanged.Notify(this,
            nameof(TimeElapsed),
            nameof(TimeLeft),
            nameof(TimeExpired));
        OnTick();
    }

    /// <summary>
    /// Returns the representation of the <see cref="TimerInfo"/> used for XML serialization.
    /// </summary>
    /// <returns>The representation of the <see cref="TimerInfo"/> used for XML serialization.</returns>
    public virtual TimerInfo ToTimerInfo()
    {
        return new()
        {
            State = State,
            StartTime = StartTime,
            EndTime = EndTime,
            TimeElapsed = TimeElapsed,
            TimeLeft = TimeLeft,
            TimeExpired = TimeExpired,
            TotalTime = TotalTime
        };
    }

    /// <summary>
    /// Disposes the timer.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Raises the <see cref="Started"/> event.
    /// </summary>
    protected virtual void OnStarted()
    {
        Started?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="Paused"/> event.
    /// </summary>
    protected virtual void OnPaused()
    {
        Paused?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="Resumed"/> event.
    /// </summary>
    protected virtual void OnResumed()
    {
        Resumed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="Stopped"/> event.
    /// </summary>
    protected virtual void OnStopped()
    {
        Stopped?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="Expired"/> event.
    /// </summary>
    protected virtual void OnExpired()
    {
        Expired?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="Tick"/> event.
    /// </summary>
    protected virtual void OnTick()
    {
        Tick?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Disposes the timer.
    /// </summary>
    /// <param name="disposing">A value indicating whether this method was invoked by an explicit call to <see
    /// cref="Dispose"/>.</param>
    protected void Dispose(bool disposing)
    {
        if (Disposed)
        {
            return;
        }

        Disposed = true;

        if (disposing)
        {
            _dispatcherTimer.Stop();
        }
    }

    /// <summary>
    /// Throws a <see cref="ObjectDisposedException"/> if the object has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">If the object has been disposed.</exception>
    protected void ThrowIfDisposed()
    {
        if (Disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }

    protected void OnPropertyChanged(string firstPropertyName, params string[] propertyNames)
    {
        PropertyChanged.Notify(this, firstPropertyName, propertyNames);
    }

    #endregion
}