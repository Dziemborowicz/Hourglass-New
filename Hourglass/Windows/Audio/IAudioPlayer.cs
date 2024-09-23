using System;

namespace Hourglass.Windows.Audio;

public interface IAudioPlayer : IDisposable
{
    void Open(string uri);
    void Play();
    void Stop();
}
