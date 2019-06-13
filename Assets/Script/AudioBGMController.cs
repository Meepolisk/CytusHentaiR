using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTool.AudioAnalyze;

public class AudioBGMController : CorePlayerBase
{
    [Header("Component Ref")]
    [SerializeField]
    private MusicPlayer musicPlayer = null;

    public override bool IsPlaying => musicPlayer.AudioSource.isPlaying;
    public override float CurrentTime => (float)musicPlayer.AudioSource.time;
    public override double Duration => musicPlayer.AudioSource.time;
    
    public void Setup(AudioClip _audio) => musicPlayer.clip = _audio;

    public override void Play()
    {
        base.Play();
        musicPlayer.AudioSource.Play();
    }
    public override void Pause()
    {
        base.Pause();
        musicPlayer.AudioSource.Pause();
    }

    public override void Stop()
    {
        base.Stop();
        musicPlayer.AudioSource.Stop();
    }
}