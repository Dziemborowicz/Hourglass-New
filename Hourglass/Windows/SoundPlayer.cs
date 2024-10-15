// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SoundPlayer.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Windows.Media;
using System.Windows.Threading;

using Audio;
using Extensions;
using Timing;

// ReSharper disable ExceptionNotDocumented

/// <summary>
/// Plays <see cref="Sound"/>s stored in the file system.
/// </summary>
public sealed class SoundPlayer : IDisposable
{
    #region Private Members

    /// <summary>
    /// A <see cref="System.Media.SoundPlayer"/> that can play *.wav files.
    /// </summary>
    private readonly System.Media.SoundPlayer _soundPlayer;

    /// <summary>
    /// A <see cref="DispatcherTimer"/> used to raise events.
    /// </summary>
    private readonly DispatcherTimer _dispatcherTimer;

    /// <summary>
    /// A <see cref="IAudioPlayer"/> that can play most audio files.
    /// </summary>
    private readonly IAudioPlayer _audioPlayer;

    private bool _isLooping;

    /// <summary>
    /// Indicates whether this object has been disposed.
    /// </summary>
    private bool _disposed;

    #endregion

    public SoundPlayer()
    {
        // Resource sound player
        _soundPlayer = new();

        _dispatcherTimer = new()
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _dispatcherTimer.Tick += DispatcherTimerTick;

        // File audio player
        _audioPlayer = AudioPlayerSelector.Create(OnAudioStopped);
    }

    /// <summary>
    /// Raised when sound playback has completed.
    /// </summary>
    public event EventHandler? PlaybackCompleted;

    /// <summary>
    /// Gets a value indicating whether the player is playing a sound.
    /// </summary>
    public bool IsPlaying { get; private set; }

    #region Public Methods

    /// <summary>
    /// Plays a <see cref="Sound"/> asynchronously.
    /// </summary>
    /// <param name="sound">A <see cref="Sound"/>.</param>
    /// <param name="loop">A value indicating whether playback should be looped.</param>
    public void Play(Sound? sound, bool loop)
    {
        ThrowIfDisposed();

        // Stop all playback
        if (!Stop())
        {
            return;
        }

        // Do not play anything
        if (sound is null)
        {
            return;
        }

        try
        {
            IsPlaying = true;
            _isLooping = loop;

            if (sound.IsBuiltIn)
            {
                // Use the sound player
                _soundPlayer.Stream = sound.GetStream();

                if (loop)
                {
                    // Asynchronously play looping sound
                    _soundPlayer.PlayLooping();
                }
                else
                {
                    // Asynchronously play sound once
                    _soundPlayer.Play();

                    // Start a timer to notify the completion of playback if we know the duration
                    if (sound.Duration.HasValue)
                    {
                        _dispatcherTimer.Interval = sound.Duration.Value;
                        _dispatcherTimer.Start();
                    }
                }
            }
            else
            {
                // Use the audio player
                _audioPlayer.Open(sound.Path!);
                _audioPlayer.Play();
            }
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            // Ignore.
        }
    }

    /// <summary>
    /// Stops playback of a <see cref="Sound"/> if playback is occurring.
    /// </summary>
    /// <returns><c>true</c> if playback is stopped successfully or no playback was occurring, or <c>false</c> otherwise.</returns>
    public bool Stop()
    {
        ThrowIfDisposed();

        try
        {
            IsPlaying = false;
            _isLooping = false;

            // Stop the sound player
            _soundPlayer.Stop();
            _dispatcherTimer.Stop();

            _soundPlayer.Stream?.Dispose();
            _soundPlayer.Stream = null;

            // Stop the audio player
            _audioPlayer.Stop();
        }
        catch (Exception ex) when (ex.CanBeHandled())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Disposes the timer.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Disposes the timer.
    /// </summary>
    /// <param name="disposing">A value indicating whether this method was invoked by an explicit call to <see
    /// cref="Dispose"/>.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (disposing)
        {
            IsPlaying = false;
            _isLooping = false;

            // Dispose the sound player
            _soundPlayer.Stop();
            _soundPlayer.Stream?.Dispose();
            _soundPlayer.Dispose();

            _dispatcherTimer.Stop();

            // Dispose the media player
            _audioPlayer.Dispose();
        }
    }

    /// <summary>
    /// Throws a <see cref="ObjectDisposedException"/> if the object has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Invoked when the <see cref="DispatcherTimer"/> interval has elapsed.
    /// </summary>
    /// <param name="sender">The <see cref="DispatcherTimer"/>.</param>
    /// <param name="e">The event data.</param>
    private void DispatcherTimerTick(object sender, EventArgs e)
    {
        _dispatcherTimer.Stop();
        PlaybackCompleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Invoked when the media has finished playback in the <see cref="MediaPlayer"/>.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event data.</param>
    private void OnAudioStopped(object sender, EventArgs e)
    {
        if (!_isLooping)
        {
            PlaybackCompleted?.Invoke(this, EventArgs.Empty);
            return;
        }

        _audioPlayer.Play();
    }

    #endregion
}