using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTool.AudioAnalyze;

public class AudioBGMController : CytusPlayer
{
    [Header("Component Ref")]
    [SerializeField]
    private MusicPlayer Player = null;

    public override bool IsPlaying => Player.AudioSource.isPlaying;
    public override float CurrentTime => (float)Player.AudioSource.time;
    public override double Duration => Player.AudioSource.time;
    
    public void Setup(AudioClip _audio)
    {
        Player.clip = _audio;
        if (AudioPeer.Instance != null)
            AudioPeer.Instance.RefreshSetting();
    }
    public override void Pause() => Player.AudioSource.Pause();

    public override void Play() => Player.AudioSource.Play();

    public override void Stop() => Player.AudioSource.Stop();
}