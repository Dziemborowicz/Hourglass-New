// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass;

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

/// <summary>
/// Flags used to specify window attributes for Desktop Window Manager (DWM) non-client rendering.
/// </summary>
internal enum DwmWindowAttribute : uint
{
    /// <summary>
    /// Enabled immersive dark mode for a Window. Has no effect before Windows 10 Build 17763. After Windows 10
    /// Build 18985, use <see cref="UseImmersiveDarkMode"/>.
    /// </summary>
    UseImmersiveDarkModeBefore20H1 = 19,

    /// <summary>
    /// Enabled immersive dark mode for a Window. Has no effect before Windows 10 Build 18985.
    /// </summary>
    UseImmersiveDarkMode = 20
}

/// <summary>
/// The thread's execution requirements.
/// </summary>
[Flags]
internal enum ExecutionState : uint
{
    /// <summary>
    /// Informs the system that the state being set should remain in effect until the next call that uses
    /// <see cref="EsContinuous"/> and one of the other state flags is cleared.
    /// </summary>
    EsContinuous = 0x80000000,

    /// <summary>
    /// <para>
    /// Forces the display to be on by resetting the display idle timer.
    /// </para><para>
    /// Windows 8: This flag can only keep a display turned on, it can't turn on a display that's currently
    /// off.
    /// </para>
    /// </summary>
    EsDisplayRequired = 0x00000002,

    /// <summary>
    /// Forces the system to be in the working state by resetting the system idle timer.
    /// </summary>
    EsSystemRequired = 0x00000001,

    /// <summary>
    /// Indicates that a call to <see cref="NativeMethods.SetThreadExecutionState"/> failed.
    /// </summary>
#pragma warning disable S2346
    EsNull = 0x00000000
#pragma warning restore S2346
}

/// <summary>
/// Wrapper class for calling Win32 and other unmanaged APIs.
/// </summary>
internal static class NativeMethods
{
    /// <summary>
    /// Gets a value of Desktop Window Manager (DWM) non-client rendering attributes for a window.
    /// </summary>
    /// <param name="hwnd">The handle to the window for which the attribute value is to be retrieved.</param>
    /// <param name="dwAttribute">A flag describing which value to retrieve.</param>
    /// <param name="pvAttribute">A pointer to a value which, when this function returns successfully, receives the
    /// current value of the attribute. The type of the retrieved value depends on the value of the <paramref
    /// name="dwAttribute"/> parameter.</param>
    /// <param name="cbAttribute">The size, in bytes, of the attribute value being received via the <paramref
    /// name="pvAttribute"/> parameter. The type of the retrieved value, and therefore its size in bytes, depends
    /// on the value of the <paramref name="dwAttribute"/> parameter.</param>
    /// <returns>0 if the successful, or nonzero otherwise.</returns>
    [DllImport("dwmapi.dll")]
    public static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, out bool pvAttribute, int cbAttribute);

    /// <summary>
    /// Sets the value of Desktop Window Manager (DWM) non-client rendering attributes for a window.
    /// </summary>
    /// <param name="hwnd">The handle to the window for which the attribute value is to be set.</param>
    /// <param name="dwAttribute">A flag describing which value to set.</param>
    /// <param name="pvAttribute">A pointer to an object containing the attribute value to set. The type of the
    /// value set depends on the value of the <paramref name="dwAttribute"/> parameter.</param>
    /// <param name="cbAttribute">The size, in bytes, of the attribute value being set via the <paramref
    /// name="pvAttribute"/> parameter. The type of the value set, and therefore its size in bytes, depends on the
    /// value of the <paramref name="dwAttribute"/> parameter.</param>
    /// <returns>0 if the successful, or nonzero otherwise.</returns>
    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref bool pvAttribute, int cbAttribute);

    /// <summary>
    /// Sets the specified timer to the inactive state.
    /// </summary>
    /// <param name="hTimer">A handle to the timer object.</param>
    /// <returns><c>true</c> if the call succeeds, or <c>false</c> otherwise.</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CancelWaitableTimer(IntPtr hTimer);

    /// <summary>
    /// Closes an open object handle.
    /// </summary>
    /// <param name="hObject">A valid handle to an open object.</param>
    /// <returns><c>true</c> if the call succeeds, or <c>false</c> otherwise.</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    /// <summary>
    /// Creates or opens a timer object.
    /// </summary>
    /// <param name="lpTimerAttributes">A pointer to a structure that specifies a security descriptor for the new
    /// timer object and determines whether child processes can inherit the returned handle.</param>
    /// <param name="bManualReset">If this parameter is <c>true</c>, the timer is a manual-reset notification timer.
    /// Otherwise, the timer is a synchronization timer.</param>
    /// <param name="lpTimerName">The name of the timer object.</param>
    /// <returns>A handle to the timer object if the call succeeds, or <see cref="IntPtr.Zero"/> otherwise.</returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string? lpTimerName);

    /// <summary>
    /// Enables an application to inform the system that it is in use, thereby preventing the system from entering
    /// sleep or turning off the display while the application is running.
    /// </summary>
    /// <param name="esFlags">The thread's execution requirements.</param>
    /// <returns>If the function succeeds, the return value is the previous thread execution state. If the function
    /// fails, the return value is <see cref="ExecutionState.EsNull"/>.</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

    /// <summary>
    /// Activates the specified timer. When the due time arrives, the timer is signaled and the thread that set the
    /// timer calls the optional completion routine.
    /// </summary>
    /// <param name="hTimer">A handle to the timer object.</param>
    /// <param name="pDueTime">The time after which the state of the timer is to be set to signaled.</param>
    /// <param name="lPeriod">The period of the timer, in milliseconds. If <paramref name="lPeriod"/> is zero, the
    /// timer is signaled once. If <paramref name="lPeriod"/> is greater than zero, the timer is periodic.</param>
    /// <param name="pfnCompletionRoutine">A pointer to an optional completion routine.</param>
    /// <param name="lpArgToCompletionRoutine">A pointer to a structure that is passed to the completion routine.
    /// </param>
    /// <param name="fResume">If this parameter is <c>true</c>, restores a system in suspended power conservation
    /// mode when the timer state is set to signaled. Otherwise, the system is not restored.</param>
    /// <returns><c>true</c> if the call succeeds, or <c>false</c> otherwise.</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetWaitableTimer(IntPtr hTimer, [In] ref long pDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);
}