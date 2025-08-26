using System;

namespace Vertigo.Wheel
{
    public interface IUIAnimation
    {
        void Play();
        void Stop();
        Action OnComplete { get; set; } 
    }
}