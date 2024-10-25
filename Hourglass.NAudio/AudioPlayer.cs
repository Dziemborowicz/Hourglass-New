using System;

using NAudio.Vorbis;
using NAudio.Wave;

using Hourglass.Windows.Audio;

namespace Hourglass.NAudio;

public class AudioPlayer: IAudioPlayer
{
    private readonly WaveOutEvent _waveOutEvent = new();

    private WaveStream? _audioFile;

    public AudioPlayer(EventHandler stoppedEventHandler) =>
        _waveOutEvent.PlaybackStopped += (s, e) => stoppedEventHandler(s, e);

    public void Open(string uri)
    {
        _audioFile?.Dispose();
        _audioFile = null;
        _audioFile = IsOgg()
            ? new VorbisWaveReader(uri)
            : new AudioFileReader(uri);

        _waveOutEvent.Init(_audioFile);

        bool IsOgg() =>
            uri.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase);
    }

    public void Play()
    {
        _audioFile!.Position = 0;
        _waveOutEvent.Play();
    }

    public void Stop()
    {
        _waveOutEvent.Stop();

        _audioFile?.Dispose();
        _audioFile = null;
    }

    public void Dispose()
    {
        Stop();

        _waveOutEvent.Dispose();
    }
}
