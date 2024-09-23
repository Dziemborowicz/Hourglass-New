using System;

using NAudio.Wave;

using Hourglass.Windows.Audio;

namespace Hourglass.NAudio;

public class AudioPlayer: IAudioPlayer
{
    private readonly WaveOutEvent _waveOutEvent = new();

    private AudioFileReader? _audioFile;

    public AudioPlayer(EventHandler stoppedEventHandler) =>
        _waveOutEvent.PlaybackStopped += (s, e) => stoppedEventHandler(s, e);

    public void Open(string uri)
    {
        _audioFile?.Dispose();
        _audioFile = null;
        _audioFile = new AudioFileReader(uri);

        _waveOutEvent.Init(_audioFile);
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
