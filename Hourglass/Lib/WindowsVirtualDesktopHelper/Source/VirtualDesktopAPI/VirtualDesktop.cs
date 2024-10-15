using System;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace

namespace WindowsVirtualDesktopHelper.VirtualDesktopAPI.Implementation;

internal abstract class VirtualDesktop<TVirtualDesktopManagerInternal> : ICurrentVirtualDesktop
    where TVirtualDesktopManagerInternal : class
{
    private IVirtualDesktopManager? _virtualDesktopManager;

    protected TVirtualDesktopManagerInternal? VirtualDesktopManagerInternal;

    protected VirtualDesktop(ImmersiveShellProvider immersiveShellProvider)
    {
        try
        {
            VirtualDesktopManagerInternal = immersiveShellProvider.QueryService<TVirtualDesktopManagerInternal>(new("C5E0CDCA-7B6E-41B2-9FC4-D93975CC467B" /* CLSID_VirtualDesktopManagerInternal */));

            if (VirtualDesktopManagerInternal is not null)
            {
                _virtualDesktopManager = (IVirtualDesktopManager)Activator.CreateInstance(Type.GetTypeFromCLSID(new("AA509086-5CA9-4C25-8F95-589D3C07B48A" /* CLSID_VirtualDesktopManager */)));
            }
        }
        catch
        {
            // Ignore.
        }
    }

    protected abstract Guid GetCurrentDesktopId();

    public bool IsValid =>
        _virtualDesktopManager is not null &&
        VirtualDesktopManagerInternal is not null;

    public void MoveTo(IntPtr handle)
    {
        try
        {
            if (!IsValid)
            {
                return;
            }

            if (_virtualDesktopManager!.IsWindowOnCurrentVirtualDesktop(handle))
            {
                return;
            }

            _virtualDesktopManager!.MoveWindowToDesktop(handle, GetCurrentDesktopId());
        }
        catch
        {
            // Ignore.
        }
    }

    public void Dispose()
    {
        ReleaseComObject(_virtualDesktopManager);
        ReleaseComObject(VirtualDesktopManagerInternal);

        _virtualDesktopManager = null;
        VirtualDesktopManagerInternal = null;

        static void ReleaseComObject(object? o)
        {
            if (o is not null)
            {
                Marshal.FinalReleaseComObject(o);
            }
        }
    }
}

internal interface ICurrentVirtualDesktop: IDisposable
{
    bool IsValid { get; }

    void MoveTo(IntPtr handle);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("A5CD92FF-29BE-454C-8D04-D82879FB3F1B")]
internal interface IVirtualDesktopManager
{
    bool IsWindowOnCurrentVirtualDesktop(IntPtr topLevelWindow);
    Guid GetWindowDesktopId(IntPtr topLevelWindow);
    void MoveWindowToDesktop(IntPtr topLevelWindow, ref Guid desktopId);
}
