// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerStartManager.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Managers;

using System.Collections.Generic;
using System.Linq;

using Properties;
using Timing;

/// <summary>
/// Manages recent <see cref="TimerStart"/> objects.
/// </summary>
public sealed class TimerStartManager : Manager
{
    /// <summary>
    /// The maximum number of <see cref="TimerStart"/>s.
    /// </summary>
    public const int Capacity = 10;

    /// <summary>
    /// Singleton instance of the <see cref="TimerStartManager"/> class.
    /// </summary>
    public static readonly TimerStartManager Instance = new();

    /// <summary>
    /// The most recent <see cref="TimerStart"/> objects in reverse chronological order.
    /// </summary>
    private readonly List<TimerStart> _timerStarts = new(Capacity);

    /// <summary>
    /// Prevents a default instance of the <see cref="TimerStartManager"/> class from being created.
    /// </summary>
    private TimerStartManager()
    {
    }

    /// <summary>
    /// Gets a list of the most recent <see cref="TimerStart"/> objects in reverse chronological order.
    /// </summary>
#pragma warning disable S2365
    public IReadOnlyCollection<TimerStart> TimerStarts => _timerStarts.Where(static e => e.IsCurrent).ToArray();
#pragma warning restore S2365

    /// <summary>
    /// Gets the most recent <see cref="TimerStart"/>, or the default <see cref="TimerStart"/> if there are no <see cref="TimerStart"/> objects in <see cref="TimerStarts"/>.
    /// </summary>
    public TimerStart LastTimerStart => _timerStarts.Find(static e => e.IsCurrent) ?? TimerStart.Default;

    /// <summary>
    /// Initializes the class.
    /// </summary>
    public override void Initialize()
    {
        _timerStarts.Clear();
        _timerStarts.AddRange(Settings.Default.TimerStarts);
    }

    /// <summary>
    /// Persists the state of the class.
    /// </summary>
    public override void Persist()
    {
        Settings.Default.TimerStarts = _timerStarts;
    }

    /// <summary>
    /// Adds a <see cref="TimerStart"/> to the list of recent <see cref="TimerStart"/> objects.
    /// </summary>
    /// <param name="timerStart">A <see cref="TimerStart"/>.</param>
    public void Add(TimerStart timerStart)
    {
        // Remove all equivalent objects
        _timerStarts.RemoveAll(e => e.ToString() == timerStart.ToString());

        // Add the object to the top of the list
        _timerStarts.Insert(0, timerStart);

        // Limit the number of objects in the list
        while (_timerStarts.Count > Capacity)
        {
            _timerStarts.RemoveAt(_timerStarts.Count - 1);
        }
    }

    /// <summary>
    /// Clears the list of recent <see cref="TimerStart"/> objects.
    /// </summary>
    public void Clear()
    {
        _timerStarts.Clear();
    }
}