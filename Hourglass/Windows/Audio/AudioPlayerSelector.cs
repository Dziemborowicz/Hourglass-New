using System;
using System.IO;
using System.Reflection;

using Hourglass.Extensions;

namespace Hourglass.Windows.Audio;

internal static class AudioPlayerSelector
{
    public static IAudioPlayer Create(EventHandler stoppedEventHandler)
    {
        const string nAudio = "Hourglass.NAudio";

        var nAudioAssemblyPath = Path.Combine(AssemblyExtensions.GetExecutableDirectoryName(), $"{nAudio}.dll");

        if (File.Exists(nAudioAssemblyPath))
        {
            try
            {
                return (IAudioPlayer)Activator.CreateInstanceFrom(
                    nAudioAssemblyPath,
                    $"{nAudio}.AudioPlayer",
                    false,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance,
                    null,
                    [stoppedEventHandler],
                    null,
                    null).Unwrap();
            }
            catch
            {
                // Ignored.
            }
        }

        return new AudioPlayer(stoppedEventHandler);
    }
}