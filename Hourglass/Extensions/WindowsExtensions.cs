// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowsExtensions.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Extensions;

using System;
using System.Linq;
using System.Management;

/// <summary>
/// Provides utility methods for interacting with the Windows environment.
/// </summary>
public static class WindowsExtensions
{
    /// <summary>
    /// Shuts down the computer.
    /// </summary>
    public static void ShutDown()
    {
        try
        {
            ManagementClass os = new("Win32_OperatingSystem");
            os.Get();
            os.Scope.Options.EnablePrivileges = true;

            ManagementBaseObject parameters = os.GetMethodParameters("Win32Shutdown");
            parameters["Flags"] = "1"; // Shut down
            parameters["Reserved"] = "0";

            foreach (ManagementObject obj in os.GetInstances().Cast<ManagementObject>())
            {
                obj.InvokeMethod("Win32Shutdown", parameters, null /* options */);
            }
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            // Ignored.
        }
    }
}