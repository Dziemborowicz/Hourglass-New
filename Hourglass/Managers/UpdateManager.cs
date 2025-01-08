// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateManager.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Managers;

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Extensions;
using Properties;
using Serialization;

// ReSharper disable LocalSuppression

/// <summary>
/// Manages app updates.
/// </summary>
public sealed class UpdateManager : Manager, INotifyPropertyChanged
{
    /// <summary>
    /// Singleton instance of the <see cref="UpdateManager"/> class.
    /// </summary>
    public static readonly UpdateManager Instance = new();

    /// <summary>
    /// The URL for the XML file containing information about the latest version of the app.
    /// </summary>
#pragma warning disable S1075
    private const string UpdateCheckUrl = "https://raw.githubusercontent.com/i2van/hourglass/develop/latest.xml";
#pragma warning restore S1075

    /// <summary>
    /// Prevents a default instance of the <see cref="UpdateManager"/> class from being created.
    /// </summary>
    private UpdateManager()
    {
    }

    /// <summary>
    /// Raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the name of the app.
    /// </summary>
    public static string AppName => Assembly.GetExecutingAssembly().GetName().Name;

    /// <summary>
    /// Gets the current version of the app.
    /// </summary>
    public static Version CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version;

    /// <summary>
    /// Gets a value indicating whether a newer version of the app is available.
    /// </summary>
    public bool HasUpdates => LatestVersion > CurrentVersion;

    /// <summary>
    /// Gets the latest version of the app.
    /// </summary>
    public Version? LatestVersion { get; private set; }

    /// <summary>
    /// Gets the unique identifier of this instance of the app.
    /// </summary>
    private static Guid UniqueId
    {
        get
        {
            if (Settings.Default.UniqueId == Guid.Empty)
            {
                Settings.Default.UniqueId = Guid.NewGuid();
            }

            return Settings.Default.UniqueId;
        }
    }

    /// <summary>
    /// Gets the URI to download the update to the latest version of the app.
    /// </summary>
    public Uri UpdateUri { get; private set; } = null!;

    /// <summary>
    /// Initializes the class.
    /// </summary>
    public override void Initialize()
    {
        if (!Settings.Default.CheckForUpdates)
        {
            return;
        }

        ServicePointManager.Expect100Continue = true;
        try
        {
            // Try to use TLS 1.3 if it's supported.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }
        // ReSharper disable once UncatchableException
        catch (NotSupportedException)
        {
            // Otherwise, fall back to using TLS 1.2.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        Task.Run(async () => SetUpdateInfo(await FetchUpdateInfoAsync()));
    }

    /// <summary>
    /// Fetches the latest <see cref="UpdateInfo"/> from the <see cref="UpdateCheckUrl"/>.
    /// </summary>
    /// <returns>An <see cref="UpdateInfo"/>.</returns>
    private static async Task<UpdateInfo?> FetchUpdateInfoAsync()
    {
        try
        {
            using HttpClient httpClient = new();

            httpClient.DefaultRequestHeaders.CacheControl = new()
            {
                NoStore = true,
                NoCache = true
            };

            httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                $"Mozilla/5.0 ({Environment.OSVersion.VersionString}) {AppName}/{CurrentVersion} (UUID: {UniqueId})");

            using Stream responseStream = await httpClient.GetStreamAsync(UpdateCheckUrl);

            return (UpdateInfo)new XmlSerializer(typeof(UpdateInfo)).Deserialize(responseStream);
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the properties of this manager from an <see cref="UpdateInfo"/>.
    /// </summary>
    /// <param name="updateInfo">An <see cref="UpdateInfo"/>.</param>
    private void SetUpdateInfo(UpdateInfo? updateInfo)
    {
        if (updateInfo is null)
        {
            return;
        }

        try
        {
            LatestVersion = new(updateInfo.LatestVersion);
            UpdateUri = new(updateInfo.UpdateUrl);
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            return;
        }

        PropertyChanged.Notify(this,
            nameof(HasUpdates),
            nameof(LatestVersion),
            nameof(UpdateUri));
    }
}