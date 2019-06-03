using System;
using UnityEngine;

public abstract class CytusPlayer : MonoBehaviour
{
    public event Action<CytusPlayer> onPlayed;
    public event Action<CytusPlayer> onStopped;

    public abstract void Play();
    public abstract void Pause();
    public abstract void Stop();

    public abstract float CurrentTime { get; }
    public abstract bool IsPlaying { get; }
    public abstract double Duration { get; }


}