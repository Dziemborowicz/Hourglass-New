// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AboutDialog.xaml.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Navigation;

using Extensions;
using Managers;

// ReSharper disable ExceptionNotDocumented
// ReSharper disable MismatchedFileName

/// <summary>
/// A window that displays information about the app.
/// </summary>
public sealed partial class AboutDialog
{
    /// <summary>
    /// The instance of the <see cref="AboutDialog"/> that is showing, or null if there is no instance showing.
    /// </summary>
    private static AboutDialog? _instance;

    /// <summary>
    /// The application name.
    /// </summary>
    public static readonly string AppName =
#if PORTABLE
        Properties.Resources.AppNamePortable;
#else
        Properties.Resources.AppName;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="AboutDialog"/> class.
    /// </summary>
    public AboutDialog()
    {
        InitializeComponent();
        InitializeMaxSize();
    }

    /// <summary>
    /// The application license.
    /// </summary>
    public static readonly string License = $"{Environment.NewLine}{Properties.Resources.License}{Environment.NewLine}";

    /// <summary>
    /// The application copyright.
    /// </summary>
    public static readonly string Copyright = Regex.Match(License, @"Copyright[^\r\n]+").Value;

    /// <summary>
    /// The application version.
    /// </summary>
    public static string Version
    {
        get
        {
            Version version = UpdateManager.CurrentVersion;

            return version.Revision != 0
                    ? version.ToString()
#pragma warning disable S3358
                    : version.ToString(version.Build != 0 ? 3 : 2);
#pragma warning restore S3358
        }
    }

    /// <summary>
    /// Shows or activates the <see cref="AboutDialog"/>. Call this method instead of the constructor to prevent
    /// multiple instances of the dialog.
    /// </summary>
    public static void ShowOrActivate()
    {
        if (_instance is null)
        {
            _instance = new();
            _instance.Show();
        }
        else
        {
            _instance.Activate();
        }
    }

    /// <summary>
    /// Initializes the <see cref="Window.MaxWidth"/> and <see cref="Window.MaxHeight"/> properties.
    /// </summary>
    private void InitializeMaxSize()
    {
        MaxWidth = 0.75 * SystemParameters.WorkArea.Width;
        MaxHeight = 0.75 * SystemParameters.WorkArea.Height;
    }

    private void AboutDialogClosed(object sender, EventArgs e)
    {
#pragma warning disable S2696
        _instance = null;
#pragma warning restore S2696
    }

    /// <summary>
    /// Invoked when navigation events are requested.
    /// </summary>
    /// <param name="sender">The hyperlink requesting navigation.</param>
    /// <param name="e">The event data.</param>
    private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Uri.Navigate();
    }


    private void UsageHyperlinkClick(object sender, RoutedEventArgs e)
    {
        UsageDialog.ShowOrActivate();
    }

    /// <summary>
    /// Invoked when the close button is clicked.
    /// </summary>
    /// <param name="sender">The close button.</param>
    /// <param name="e">The event data.</param>
    private void CloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}