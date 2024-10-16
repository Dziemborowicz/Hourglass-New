// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeepAwakeManager.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Managers;

using System.Collections.Generic;

/// <summary>
/// Manages the thread-state of the main user interface thread to keep the computer from sleeping while a timer is running in any window.
/// </summary>
public sealed class KeepAwakeManager : Manager
{
    /// <summary>
    /// Singleton instance of the <see cref="KeepAwakeManager"/> class.
    /// </summary>
    public static readonly KeepAwakeManager Instance = new();

    /// <summary>
    /// The set of ids that require that the system to be kept awake.
    /// </summary>
    private readonly HashSet<int> _idsToKeepAwakeFor = [];

    /// <summary>
    /// The <see cref="ExecutionState"/> before the manager started keeping the system awake.
    /// </summary>
    private ExecutionState _previousExecutionState = ExecutionState.EsNull;

    /// <summary>
    /// Prevents a default instance of the <see cref="KeepAwakeManager"/> class from being created.
    /// </summary>
    private KeepAwakeManager()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the manager is currently keeping the system awake.
    /// </summary>
    public bool IsKeepingSystemAwake { get; private set; }

    /// <summary>
    /// Adds the specified id to the set of ids that require that the system to be kept awake and starts
    /// keeping the system awake if it was not already being kept awake.
    /// </summary>
    /// <param name="id">An <see cref="int"/>.</param>
    public void StartKeepAwakeFor(int id)
    {
        _idsToKeepAwakeFor.Add(id);
        UpdateKeepAwake();
    }

    /// <summary>
    /// Removes the specified id from the set of ids that require that the system to be kept awake and stops
    /// keeping the system awake if it was being kept awake.
    /// </summary>
    /// <param name="id">An <see cref="int"/>.</param>
    public void StopKeepAwakeFor(int id)
    {
        _idsToKeepAwakeFor.Remove(id);
        UpdateKeepAwake();
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

        StopKeepAwake();
        base.Dispose(disposing);
    }

    /// <summary>
    /// Starts or stops keeping the system awake as required.
    /// </summary>
    private void UpdateKeepAwake()
    {
        if (_idsToKeepAwakeFor.Count > 0)
        {
            StartKeepAwake();
        }
        else
        {
            StopKeepAwake();
        }
    }

    /// <summary>
    /// Start keeping the system awake. If the system is already being kept awake, this method does nothing.
    /// </summary>
    private void StartKeepAwake()
    {
        if (IsKeepingSystemAwake)
        {
            return;
        }

        ExecutionState executionState = ExecutionState.EsContinuous | ExecutionState.EsDisplayRequired | ExecutionState.EsSystemRequired;
        _previousExecutionState = NativeMethods.SetThreadExecutionState(executionState);

        IsKeepingSystemAwake = true;
    }

    /// <summary>
    /// Stops keeping the system awake. If the system is not being kept awake, this method does nothing.
    /// </summary>
    private void StopKeepAwake()
    {
        if (!IsKeepingSystemAwake)
        {
            return;
        }

        if (_previousExecutionState != ExecutionState.EsNull)
        {
            NativeMethods.SetThreadExecutionState(_previousExecutionState);
        }

        IsKeepingSystemAwake = false;
    }
}