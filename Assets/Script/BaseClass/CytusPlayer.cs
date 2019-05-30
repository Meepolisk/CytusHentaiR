using UnityEngine;

public abstract class CytusPlayer : MonoBehaviour
{
    public abstract void Play();
    public abstract void Pause();
    public abstract void Stop();

    public abstract float CurrentTime { get; }
    public abstract bool IsPlaying { get; }
    public abstract double Duration { get; }
}