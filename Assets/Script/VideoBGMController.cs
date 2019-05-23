using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoBGMController : CytusPlayer
{
    public VideoPlayer Player { get; private set; }
    public override bool isPlaying { get { return Player.isPlaying; } }
    public bool isPause { get { return Player.isPaused; } }
    public bool isPrepared { get { return Player.isPrepared; } }

    public override double Duration
    {
        get
        {
            return Player.time;
        }
    }

    [SerializeField]
    private VideoPlayer videoPlayer = null;

    private void Awake()
    {
        Player = GetComponent<VideoPlayer>();
    }

    public override void Pause()
    {
        Player.Pause();
    }

    public override void Play()
    {
        Player.Play();
    }

    public override void Stop()
    {
        Player.Stop();
    }
}

public abstract class CytusPlayer : MonoBehaviour
{
    public abstract void Play();
    public abstract void Pause();
    public abstract void Stop();

    public abstract bool isPlaying { get; }
    public abstract double Duration { get; }
}