using System;
using System.Windows.Media;

namespace Hourglass.Windows.Audio;

internal class AudioPlayer: IAudioPlayer
{
    private readonly MediaPlayer _mediaPlayer = new();

    public AudioPlayer(EventHandler stoppedEventHandler) =>
        _mediaPlayer.MediaEnded += stoppedEventHandler;

    public void Open(string uri) =>
        _mediaPlayer.Open(new(uri));

    public void Play()
    {
        _mediaPlayer.Position = TimeSpan.Zero;
        _mediaPlayer.Play();
    }

    public void Stop()
    {
        _mediaPlayer.Stop();
        _mediaPlayer.Close();
    }

    public void Dispose() =>
        Stop();
}
