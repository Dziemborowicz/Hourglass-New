// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SoundManager.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Managers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Extensions;
using Properties;
using Timing;

/// <summary>
/// Manages notification sounds.
/// </summary>
public sealed class SoundManager : Manager
{
    /// <summary>
    /// Singleton instance of the <see cref="SoundManager"/> class.
    /// </summary>
    public static readonly SoundManager Instance = new();

    /// <summary>
    /// The extensions of the supported sound files.
    /// </summary>
    private static readonly string[] SupportedTypes =
    [
        "*.aac",
        "*.m4a",
        "*.mid",
        "*.midi",
        "*.mp3",
        "*.wav",
        "*.wma"
    ];

    /// <summary>
    /// A collection of sounds.
    /// </summary>
    private readonly List<Sound> _sounds = [];

    /// <summary>
    /// Prevents a default instance of the <see cref="SoundManager"/> class from being created.
    /// </summary>
    private SoundManager()
    {
    }

    /// <summary>
    /// Gets the default sound.
    /// </summary>
    public Sound DefaultSound => GetSoundByIdentifier("resource:Normal beep")!;

    /// <summary>
    /// Gets a collection of the sounds stored in the assembly.
    /// </summary>
#pragma warning disable S2365
    public IReadOnlyCollection<Sound> BuiltInSounds => _sounds.Where(static s => s.IsBuiltIn).ToArray();
#pragma warning restore S2365

    /// <summary>
    /// Gets a collection of the sounds stored in the file system.
    /// </summary>
#pragma warning disable S2365
    public IReadOnlyCollection<Sound> UserProvidedSounds => _sounds.Where(static s => !s.IsBuiltIn).ToArray();
#pragma warning restore S2365

    /// <summary>
    /// Initializes the class.
    /// </summary>
    public override void Initialize()
    {
        _sounds.Clear();
        _sounds.AddRange(GetBuiltInSounds());
        _sounds.AddRange(GetUserProvidedSounds());
    }

    /// <summary>
    /// Returns the sound for the specified identifier, or <c>null</c> if no such sound is loaded.
    /// </summary>
    /// <param name="identifier">The identifier for the sound.</param>
    /// <returns>The sound for the specified identifier, or <c>null</c> if no such sound is loaded.</returns>
    public Sound? GetSoundByIdentifier(string? identifier)
    {
        return string.IsNullOrWhiteSpace(identifier)
                ? null
                : _sounds.Find(s => s.Identifier == identifier);
    }

    /// <summary>
    /// Returns the sound for the specified identifier, or <see cref="DefaultSound"/> if no such sound is loaded.
    /// </summary>
    /// <param name="identifier">The identifier for the sound.</param>
    /// <returns>The sound for the specified identifier, or <see cref="DefaultSound"/> if no such sound is loaded.</returns>
    public Sound? GetSoundOrDefaultByIdentifier(string? identifier)
    {
        return string.IsNullOrWhiteSpace(identifier)
                ? null
                : GetSoundByIdentifier(identifier!) ?? DefaultSound;
    }

    /// <summary>
    /// Returns the first sound for the specified name, or <c>null</c> if no such sound is loaded.
    /// </summary>
    /// <param name="name">The name for the sound.</param>
    /// <param name="stringComparison">One of the enumeration values that specifies how the strings will be
    /// compared.</param>
    /// <returns>The first sound for the specified name, or <c>null</c> if no such sound is loaded.</returns>
    public Sound? GetSoundByName(string name, StringComparison stringComparison = StringComparison.Ordinal)
    {
        return string.IsNullOrWhiteSpace(name)
                ? null
                : _sounds.Find(s => string.Equals(s.Name, name, stringComparison));
    }

    /// <summary>
    /// Loads the collection of sounds stored in the assembly.
    /// </summary>
    /// <returns>A collection of sounds stored in the assembly.</returns>
    private static IList<Sound> GetBuiltInSounds()
    {
        return
        [
            new(
                "Loud beep",
                Resources.SoundManagerLoudBeep,
                static () => Resources.BeepLoud,
                TimeSpan.FromMilliseconds(600)),

            new(
                "Normal beep",
                Resources.SoundManagerNormalBeep,
                static () => Resources.BeepNormal,
                TimeSpan.FromMilliseconds(600)),

            new(
                "Quiet beep",
                Resources.SoundManagerQuietBeep,
                static () => Resources.BeepQuiet,
                TimeSpan.FromMilliseconds(600))
        ];
    }

    /// <summary>
    /// Loads the collection of sounds stored in the file system.
    /// </summary>
    /// <returns>A collection of sounds stored in the file system.</returns>
    private static IEnumerable<Sound> GetUserProvidedSounds()
    {
        const string soundsDirectory = "Sounds";

        try
        {
            string appDirectory = AssemblyExtensions.GetExecutableDirectoryName();
            string appSoundsDirectory = Path.Combine(appDirectory, soundsDirectory);
            string localAppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Hourglass");
            string localAppDataSoundsDirectory = Path.Combine(localAppDataDirectory, soundsDirectory);

            List<Sound> list =
            [
                ..GetUserProvidedSounds(appDirectory),
                ..GetUserProvidedSounds(appSoundsDirectory),
                ..GetUserProvidedSounds(localAppDataDirectory),
                ..GetUserProvidedSounds(localAppDataSoundsDirectory)
            ];
            list.Sort(static (a, b) => string.Compare(a.Name, b.Name, CultureInfo.CurrentCulture, CompareOptions.StringSort));
            return list;
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            // Not worth raising an exception
            return [];
        }
    }

    /// <summary>
    /// Loads the collection of sounds stored in the file system at the specified path.
    /// </summary>
    /// <param name="path">A path to a directory.</param>
    /// <returns>A collection of sounds stored in the file system at the specified path.</returns>
    private static IEnumerable<Sound> GetUserProvidedSounds(string path)
    {
        try
        {
            List<Sound> list = [];

            if (Directory.Exists(path))
            {
                foreach (string supportedType in SupportedTypes)
                {
                    IEnumerable<string> filePaths = Directory.GetFiles(path, supportedType);
                    IEnumerable<Sound> fileSounds = filePaths.Select(static p => new Sound(p));
                    list.AddRange(fileSounds);
                }
            }

            return list;
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            // Not worth raising an exception
            return [];
        }
    }
}