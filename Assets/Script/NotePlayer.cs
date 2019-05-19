using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePlayer : CytusPlayer
{
    [SerializeField]
    private CytusPlayer MainCytusPlayer = null;

    [SerializeField]
    private List<iNoteProfile> noteList = null;
    
    private float timeTick = 0f;
    private Coroutine coroutine = null;

    private bool _isPlaying = false;
    public override bool isPlaying
    {
        get
        {
            return _isPlaying;
        }
    }

    public override double Duration
    {
        get
        {
            return MainCytusPlayer.Duration;
        }
    }

    public override void Pause()
    {
        _isPlaying = false;
    }

    public override void Play()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(_Play());
    }

    private IEnumerator _Play()
    {
        _isPlaying = true;
        timeTick = 0f;
        float maxTime = (float)Duration;
        while (isPlaying)
        {
            yield return null;
            timeTick += Time.deltaTime;
            if (timeTick >= maxTime)
            {
                timeTick = maxTime;
                break;
            }
            else
            {
                PerformAction();
            }
        }
        Stop();
    }

    public override void Stop()
    {
        _isPlaying = false;
    }

    private void PerformAction()
    {
        //imple here
    }
}

[System.Serializable]
public class iNoteProfile
{
    float TimeAppear;
}
