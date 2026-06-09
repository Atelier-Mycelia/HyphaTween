using System;

namespace AtMycelia.HyphaTween
{
    public interface ITweenHandle
    {
        void Kill();
        bool IsPlaying { get; }
        ITweenHandle SetOnComplete(Action onComplete);
    }
}