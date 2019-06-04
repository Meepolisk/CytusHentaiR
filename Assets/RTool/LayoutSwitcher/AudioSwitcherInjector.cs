using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using RTool.LayoutSwitcher;

public class AudioSwitcherInjector : DataSwitcherInjector<AudioClip>
{
    protected override Dictionary<Type, Action<Component, AudioClip>> ComponentDictionary
    {
        get
        {
            return _dictionary;
        }
    }

    [System.Serializable]
    protected class AudioEvent : UnityEvent<AudioClip> { };
    [SerializeField]
    protected AudioEvent onAudioLoaded;

    protected override void OnManualSwitch(AudioClip _audioClip)
    {
        onAudioLoaded.Invoke(_audioClip);
    }

    private static readonly Dictionary<Type, Action<Component, AudioClip>> _dictionary
        = new Dictionary<Type, Action<Component, AudioClip>>()
        {
            {
                typeof(AudioSource), (_component, _clip) =>
                {
                    var audioSource = _component as AudioSource;
                    audioSource.clip = _clip;
                }
            },
        };
}