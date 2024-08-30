// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorManager.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Managers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

using Extensions;
using Properties;
using Windows;

/// <summary>
/// Manages global errors.
/// </summary>
public sealed class ErrorManager : Manager
{
    /// <summary>
    /// Singleton instance of the <see cref="ErrorManager"/> class.
    /// </summary>
    public static readonly ErrorManager Instance = new();

    /// <summary>
    /// The maximum number of error dump files to keep in the temporary folder.
    /// </summary>
    private const int MaxErrorDumps = 5;

    /// <summary>
    /// Prevents a default instance of the <see cref="ErrorManager"/> class from being created.
    /// </summary>
    private ErrorManager()
    {
    }

    /// <summary>
    /// Initializes the class.
    /// </summary>
    public override void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
    }

    /// <summary>
    /// Reports an error by displaying an error dialog and logging the error to a text file.
    /// </summary>
    /// <param name="errorMessage">An error message.</param>
    public static void ReportError(string errorMessage)
    {
        // Dump the error to a file
        if (TryDumpError(errorMessage, out var dumpPath))
        {
            errorMessage += Environment.NewLine;
            errorMessage += Environment.NewLine;
            errorMessage += string.Format(
                Resources.ResourceManager.GetEffectiveProvider(),
                Resources.ErrorManagerErrorHasBeenWritten,
                dumpPath);
        }

        // Clean up old error dumps
        if (!TryCleanErrorDumps())
        {
            errorMessage += Environment.NewLine;
            errorMessage += Environment.NewLine;
            errorMessage += Resources.ErrorManagerFailedToClean;
        }

        // Show an error dialog
        ErrorDialog errorDialog = new();
        errorDialog.ShowDialog(Resources.ErrorManagerUnexpectedError, details: errorMessage);
    }

    /// <summary>
    /// Invoked when an exception is not caught.
    /// </summary>
    /// <param name="sender">The <see cref="AppDomain"/>.</param>
    /// <param name="e">The event data.</param>
    private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            ReportError(e.ExceptionObject.ToString());
        }
        finally
        {
            Application.Current.Shutdown(1);
        }
    }

    /// <summary>
    /// Tries to write an error message to a file in the temporary files' folder.
    /// </summary>
    /// <param name="errorMessage">An error message.</param>
    /// <param name="dumpPath">The path of the that was written.</param>
    /// <returns><c>true</c> if the error message is successfully written, or <c>false</c> otherwise.</returns>
    private static bool TryDumpError(string errorMessage, out string? dumpPath)
    {
        try
        {
            dumpPath = GetErrorDumpPath(DateTime.Now);
            File.WriteAllText(dumpPath, errorMessage);
            return true;
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            dumpPath = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to limit the number of error messages written to files in the temporary files folder to <see
    /// cref="MaxErrorDumps"/>.
    /// </summary>
    /// <returns><c>true</c> if the limit was successfully applied, or <c>false</c> otherwise.</returns>
    private static bool TryCleanErrorDumps()
    {
        try
        {
            // Get the temporary files directory
            DirectoryInfo directory = new(Path.GetTempPath());

            // Get the dump files with newest files first
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            string dumpPathPattern = string.Format(CultureInfo.InvariantCulture, "{0}-Crash.*", appName);
#pragma warning disable IDE0305
            List<FileInfo> dumpFiles = directory.GetFiles(dumpPathPattern).OrderBy(static f => f.LastWriteTimeUtc).ToList();
#pragma warning restore IDE0305

            // Delete dump files until we have only MaxErrorDumps left
            while (dumpFiles.Count > MaxErrorDumps)
            {
                FileInfo dumpFile = dumpFiles[0];
                dumpFile.Delete();
                dumpFiles.Remove(dumpFile);
            }

            return true;
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the path for the error dump with the specified suffix.
    /// </summary>
    /// <param name="dateTime">The suffix for the error dump path.</param>
    /// <returns>The path for the error dump with the specified suffix.</returns>
    private static string GetErrorDumpPath(DateTime dateTime)
    {
        string appName = Assembly.GetExecutingAssembly().GetName().Name;
        string directory = Path.GetTempPath();
        string filename = string.Format(
            CultureInfo.InvariantCulture,
            "{0}-Crash.{1:yyyyMMdd-HHMMss-fffffff}.txt",
            appName,
            dateTime);

        return Path.Combine(directory, filename);
    }
}