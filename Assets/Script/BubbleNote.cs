using UnityEngine;
using System.Collections;
using PoolingObject;

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

    public bool IsCorrect (float _time)
    {
        return (_time >= Min && _time < Max);
    }
}

[System.Serializable]
public class TimeFrameProfile
{
    [SerializeField,Range (0,1)]
    private float initFrames = 0.2f;
    public float InitFrames => initFrames;

    [SerializeField,Range (0,2)]
    private float liveFrames = 1f;
    public float LiveFrames => liveFrames;

    [SerializeField,Range (0,2)]
    private float decayFrames = 0.6f;
    public float DecayFrames => decayFrames;
}

[System.Serializable]
public class ScoreFrameProfile
{
    [SerializeField]
    private float perfectScale = 0.2f;
    public float PerfectScale => perfectScale;

    [SerializeField]
    private float greateScale = 0.4f;
    public float GreateScale => greateScale;

    [SerializeField]
    private float goodScale = 0.7f;
    public float GoodScale => goodScale;
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
    //public TimeRange TR_Bad { get; private set; }

    //Insetup
    public NotePlayer Player { get; protected set; }
    public void Setup(NotePlayer _player)
    {
        Player = _player;
    }
    public virtual void Refresh(NoteProfile noteProfile)
    {
        float hitTime = noteProfile.HitTime;
        TR_Perfect = new TimeRange(hitTime, TimeFrameProfile, ScoreFrameProfile.PerfectScale);
        TR_Great = new TimeRange(hitTime, TimeFrameProfile, ScoreFrameProfile.GreateScale);
        TR_Good = new TimeRange(hitTime, TimeFrameProfile, ScoreFrameProfile.GoodScale);
        //TR_Bad = new TimeRange(hitTime, TimeFrameProfile);

        //todo: imple phân đoạn cắt animation dựa theo chênh lệch
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
}

public class BubbleNote : NoteBase
{
    [Header("Component Ref")]
    [SerializeField]
    private Animator anim = null;
    [SerializeField]
    private Collider easyTouchDetector = null;

    public override void OnSpawn()
    {
        base.OnSpawn();
        anim.Rebind();
        EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;
    }

    public override void Kill()
    {
        base.Kill();
        EasyTouch.On_TouchStart -= EasyTouch_On_TouchStart;
    }
    //private void OnEnable()
    //{
    //    EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;
    //}

    //private void OnDisable()
    //{
    //    EasyTouch.On_TouchStart -= EasyTouch_On_TouchStart;
    //}

    private void EasyTouch_On_TouchStart(Gesture gesture)
    {
        if (gesture.pickObject == easyTouchDetector.gameObject)
        {
            PlayerHit();
        }
    }

    public void PlayerHit()
    {
        anim.SetTrigger("Hit");
        HitType hitType = Calculate(Player.CurrentTime);
        switch (hitType)
        {
            case HitType.Perfect:
                Debug.Log("Perfect");
                break;
            case HitType.Great:
                Debug.Log("Great");
                break;
            case HitType.Good:
                Debug.Log("Good");
                break;
            default:
                Debug.Log("Bad");
                break;
        }
    }
}
