// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Sound.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Timing;

using System;
using System.IO;

using static System.IO.Path;

using Managers;
using Extensions;

// ReSharper disable ExceptionNotDocumented

/// <summary>
/// A sound that can be used to notify the user that a timer has expired.
/// </summary>
public sealed class Sound
{
    /// <summary>
    /// A method that returns a stream to the sound data.
    /// </summary>
    private readonly Func<UnmanagedMemoryStream>? _streamProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sound"/> class for a sound stored in the file system.
    /// </summary>
    /// <param name="path">The path to the sound file.</param>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/></exception>
    public Sound(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        Name = GetNameFromPath(path);
        Identifier = GetIdentifierFromPath(path);
        IsBuiltIn = false;
        Path = path;
        Duration = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sound"/> class for a sound stored in the assembly.
    /// </summary>
    /// <param name="invariantName">The culture-insensitive name of the sound.</param>
    /// <param name="name">The friendly name for the sound.</param>
    /// <param name="streamProvider">A method that returns a stream to the sound data.</param>
    /// <param name="duration">The length of the sound.</param>
    /// <exception cref="ArgumentNullException"><paramref name="invariantName"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="duration"/> is negative.</exception>
    public Sound(string invariantName, string name, Func<UnmanagedMemoryStream> streamProvider, TimeSpan duration)
    {
        if (string.IsNullOrWhiteSpace(invariantName))
        {
            throw new ArgumentNullException(nameof(invariantName));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        Name = name;
        Identifier = "resource:" + invariantName;
        IsBuiltIn = true;
        Duration = duration;

        _streamProvider = streamProvider ?? throw new ArgumentNullException(nameof(streamProvider));
    }

    /// <summary>
    /// Gets the default sound.
    /// </summary>
    public static Sound DefaultSound => SoundManager.Instance.DefaultSound;

    /// <summary>
    /// Gets a sound representing no sound.
    /// </summary>
    public static Sound? NoSound => null;

    /// <summary>
    /// Gets the friendly name for this sound.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the unique identifier for this sound.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Gets a value indicating whether this sound is stored in the assembly.
    /// </summary>
    public bool IsBuiltIn { get; }

    /// <summary>
    /// Gets the path to the sound file.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    /// Gets the length of the sound, or <c>null</c> if the length of the sound is unknown.
    /// </summary>
    public TimeSpan? Duration { get; }

    /// <summary>
    /// Returns a <see cref="Sound"/> for the specified identifier, or <c>null</c> if the identifier is <c>null</c>
    /// or empty.
    /// </summary>
    /// <param name="identifier">The identifier for the sound.</param>
    /// <returns>A <see cref="Sound"/> for the specified identifier, or <c>null</c> if the identifier is
    /// <c>null</c> or empty.</returns>
    public static Sound? FromIdentifier(string? identifier)
    {
        return SoundManager.Instance.GetSoundOrDefaultByIdentifier(identifier);
    }

    /// <summary>
    /// Returns a stream with the sound data.
    /// </summary>
    /// <returns>A stream with the sound data.</returns>
    public Stream GetStream()
    {
        return _streamProvider is not null
            ? _streamProvider()
            : new FileStream(Path!, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    /// <summary>
    /// Returns the friendly name for a sound file.
    /// </summary>
    /// <param name="path">The path to the sound file.</param>
    /// <returns>The friendly name for a sound file.</returns>
    private static string GetNameFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        return GetFileNameWithoutExtension(path);
    }

    /// <summary>
    /// Returns the unique identifier for a sound file.
    /// </summary>
    /// <param name="path">The path to the sound file.</param>
    /// <returns>The unique identifier for a sound file.</returns>
    private static string GetIdentifierFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        string appDirectory = AssemblyExtensions.GetExecutableDirectoryName();
        string fullPath = GetFullPath(path);

        // Return a relative path if the sound is in or under the app directory, or otherwise return the full path
        return fullPath.StartsWith(appDirectory, StringComparison.OrdinalIgnoreCase)
            ? "file:." + fullPath.Substring(appDirectory.Length)
            : "file:" + path;
    }
}