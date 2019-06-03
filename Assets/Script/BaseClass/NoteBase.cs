﻿using System;
using UnityEngine;


public enum HitType
{
    Bad, Good, Great, Perfect
}

public struct TimeRange
{
    public float Min { get; private set; }
    public float Max { get; private set; }

    public TimeRange(float hitTime, TimeFrameProfile timeFrameProfile, float scale)
    {
        Min = hitTime - (timeFrameProfile.LiveFrames * scale);
        Max = hitTime + (timeFrameProfile.DecayFrames * scale);
    }
    public TimeRange(float hitTime, TimeFrameProfile timeFrameProfile)
    {
        Min = hitTime - timeFrameProfile.LiveFrames;
        Max = hitTime + timeFrameProfile.DecayFrames;
    }

    public bool IsCorrect(float _time)
    {
        return (_time >= Min && _time < Max);
    }
}

public abstract class NoteBase : PoolingObject.Object
{
    [Header("Prefab pre-config")]
    [SerializeField, Tooltip("Setup this to math the animation")]
    private TimeFrameProfile timeFrameProfile = null;
    public TimeFrameProfile TimeFrameProfile => timeFrameProfile;

    [SerializeField, Tooltip("Setup this as a total of 1")]
    private ScoreFrameProfile scoreFrameProfile = null;
    public ScoreFrameProfile ScoreFrameProfile => scoreFrameProfile;

    public TimeRange TR_Perfect { get; private set; }
    public TimeRange TR_Great { get; private set; }
    public TimeRange TR_Good { get; private set; }
    public NoteProfile NoteProfile { get; private set; }

    public static event Action<NoteBase, HitType> aNoteScored;

    //Insetup
    public NotePlayer Player { get; protected set; }
    public void Setup(NotePlayer _player)
    {
        Player = _player;
    }
    public virtual void Refresh(NoteProfile noteProfile)
    {
        NoteProfile = noteProfile;
        float hitTime = NoteProfile.HitTime;
        TR_Perfect = new TimeRange(hitTime, TimeFrameProfile, ScoreFrameProfile.PerfectScale);
        TR_Great = new TimeRange(hitTime, TimeFrameProfile, ScoreFrameProfile.GreateScale);
        TR_Good = new TimeRange(hitTime, TimeFrameProfile, ScoreFrameProfile.GoodScale);
        //TR_Bad = new TimeRange(hitTime, TimeFrameProfile);

        //todo: imple phân đoạn cắt animation dựa theo chênh lệch thời gian
    }

    protected virtual HitType Calculate(float _time)
    {
        if (TR_Perfect.IsCorrect(_time))
            return HitType.Perfect;
        else if (TR_Great.IsCorrect(_time))
            return HitType.Great;
        else if (TR_Good.IsCorrect(_time))
            return HitType.Good;
        return HitType.Bad;
    }

    protected void PlayerHit()
    {
        HitType hitType = Calculate(Player.CurrentTime);
        if (aNoteScored != null)
            aNoteScored(this, hitType);
        Scoring(hitType);
    }
    protected abstract void Scoring(HitType hitType);
}