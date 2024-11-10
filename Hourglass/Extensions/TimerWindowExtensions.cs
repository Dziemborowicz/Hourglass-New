using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

using Hourglass.Properties;
using Hourglass.Timing;
using Hourglass.Windows;

namespace Hourglass.Extensions;

// ReSharper disable LocalSuppression
// ReSharper disable ExceptionNotDocumented

public static class TimerWindowExtensions
{
    private static readonly Comparer<TimerWindow> RankComparer = Comparer<TimerWindow>.Create(CompareRank);
    private static readonly Comparer<TimerWindow> TimeComparer = Comparer<TimerWindow>.Create(CompareTime);
    private static readonly StringComparer TitleComparer = StringComparer.CurrentCultureIgnoreCase;

    public static IEnumerable<TimerWindow> Arrange(this IEnumerable<TimerWindow> windows)
    {
        var orderByRank = windows.OrderBy(static window => window, RankComparer);

        return Settings.Default.OrderByTitleFirst
            ? orderByRank.ThenBy(Title, TitleComparer).ThenBy(static window => window, TimeComparer)
            : orderByRank.ThenBy(static window => window, TimeComparer).ThenBy(Title, TitleComparer);
    }

    public static IEnumerable<TimerWindow> ArrangeDescending(this IEnumerable<TimerWindow> windows) =>
        windows.Arrange().Reverse();

    public static void BringNextToFrontAndActivate(this TimerWindow thisWindow, bool activate = true)
    {
        if (thisWindow.DoNotActivateNextWindow)
        {
            thisWindow.DoNotActivateNextWindow = false;
            return;
        }

        if (!Settings.Default.ActivateNextWindow)
        {
            return;
        }

        var nextWindow = GetNextWindow();
        nextWindow?.Dispatcher.BeginInvoke(() => nextWindow.BringToFrontAndActivate(activate));

        TimerWindow? GetNextWindow()
        {
            if (Application.Current is null)
            {
                return null;
            }

            var allWindows = Application.Current.Windows.OfType<TimerWindow>().Arrange().ToArray();

            return GetNextApplicableWindow(allWindows.SkipWhile(NotThisWindow).Skip(1)) ??
                   GetNextApplicableWindow(allWindows.TakeWhile(NotThisWindow));

            bool NotThisWindow(TimerWindow window) =>
                !ReferenceEquals(thisWindow, window);

            static TimerWindow? GetNextApplicableWindow(IEnumerable<TimerWindow> windows) =>
                windows.FirstOrDefault(static window => window.IsVisible && window.WindowState != WindowState.Minimized);
        }
    }

    private static string? Title(TimerWindow window) =>
        window.Timer.Options.Title;

    private static int CompareRank(TimerWindow x, TimerWindow y)
    {
        return ToRank(x.Timer.State).CompareTo(ToRank(y.Timer.State));

        static int ToRank(TimerState timerState) =>
            timerState switch
            {
                TimerState.Stopped => 0,
                TimerState.Expired => 1,
                TimerState.Paused  => 2,
                TimerState.Running => 3,
                _ => int.MaxValue
            };
    }

    private static int CompareTime(TimerWindow x, TimerWindow y)
    {
        return IsNotRunning(x.Timer.TimeLeft, y.Timer.TimeLeft)
            ? CompareTimeSpan(x.Timer.TotalTime, y.Timer.TotalTime)
            : CompareTimeSpan(x.Timer.TimeLeft,  y.Timer.TimeLeft);

        int CompareTimeSpan(TimeSpan? xTimeSpan, TimeSpan? yTimeSpan) =>
            CompareTimeSpanValue(ToTimeSpanValue(xTimeSpan), ToTimeSpanValue(yTimeSpan));

        int CompareTimeSpanValue(TimeSpan xTimeSpan, TimeSpan yTimeSpan)
        {
            var timeSpanCompare = xTimeSpan.CompareTo(yTimeSpan);

            return timeSpanCompare == 0
                ? y.ID.CompareTo(x.ID)
                : timeSpanCompare;
        }

        static bool IsNotRunning(TimeSpan? x, TimeSpan? y) =>
            x is null          ||
            y is null          ||
            x == TimeSpan.Zero ||
            y == TimeSpan.Zero;

        static TimeSpan ToTimeSpanValue(TimeSpan? timeSpan) =>
            TimeSpan.FromSeconds(Math.Round((timeSpan ?? TimeSpan.Zero).TotalSeconds));
    }

    public static void AddWindowProcHook(this TimerWindow timerWindow)
    {
        var handle = new WindowInteropHelper(timerWindow).Handle;

        var hwndSource = HwndSource.FromHwnd(handle);
        hwndSource?.AddHook(WindowProc);

        timerWindow.Closed += TimerWindowClosed;

        void TimerWindowClosed(object sender, EventArgs e)
        {
            timerWindow.Closed -= TimerWindowClosed;

            // ReSharper disable AccessToDisposedClosure
            hwndSource?.RemoveHook(WindowProc);
            hwndSource?.Dispose();
            // ReSharper restore AccessToDisposedClosure
        }

        IntPtr WindowProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            if (msg == 0x0024 /* WM_GETMINMAXINFO */)
            {
                handled = WmGetMinMaxInfo(hwnd, lParam);
            }

            return IntPtr.Zero;
        }

        bool WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            if (timerWindow.WindowState == WindowState.Normal ||
                timerWindow.IsFullScreen)
            {
                return false;
            }

            var hMonitor = MonitorFromWindow(hwnd, 0x00000002 /* MONITOR_DEFAULTTONEAREST */);
            if (hMonitor == IntPtr.Zero)
            {
                return false;
            }

            var monitorInfo = new MONITORINFO
            {
                cbSize = Marshal.SizeOf(typeof(MONITORINFO))
            };
            if (!GetMonitorInfo(hMonitor, ref monitorInfo))
            {
                return false;
            }

            var rcWork = monitorInfo.rcWork;
            var rcMonitor = monitorInfo.rcMonitor;

            var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            mmi.ptMaxPosition.x = Math.Abs(rcWork.left - rcMonitor.left);
            mmi.ptMaxPosition.y = Math.Abs(rcWork.top - rcMonitor.top);
            mmi.ptMaxSize.x = Math.Abs(rcWork.right - rcWork.left);
            mmi.ptMaxSize.y = Math.Abs(rcWork.bottom - rcWork.top);

            Marshal.StructureToPtr(mmi, lParam, true);

            return true;

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

            [DllImport("user32.dll")]
            static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
        }
    }

#pragma warning disable S101
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }
#pragma warning restore S101
}
