using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoBGMController : CytusPlayer
{
    public override bool IsPlaying => Player.isPlaying;
    public override float CurrentTime => (float)Player.time;
    public bool isPause => Player.isPaused;
    public bool isPrepared => Player.isPrepared;
    public override double Duration => Player.time;

    public VideoPlayer Player { get; private set; }

    public void Setup(VideoClip _video)
    {
        Player.clip = _video;
    }

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