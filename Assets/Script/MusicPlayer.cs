using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTool.AudioAnalyze;

[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public AudioSource AudioSource { get; private set; }
    public AudioClip clip
    {
        set
        {
            AudioSource.clip = value;
            AudioPeer.TryRefreshSetting();
        }
        get => AudioSource.clip;
    }

    private void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
    }
}
