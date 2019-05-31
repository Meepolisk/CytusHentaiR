using UnityEngine;
using System.Collections;

[System.Serializable]
public class NoteProfile
{
    [SerializeField]
    private float hitTime = 0f;
    public float HitTime => hitTime;

    [SerializeField]
    private Vector2 position = Vector2.zero;
    public Vector2 Position => position;

    public NoteProfile (float _hitTime, Vector2 _pos)
    {
        hitTime = _hitTime;
        position = _pos;
    }

    public float AppearTime { private set; get; }
    public bool CanBePull(float _time) => (_time >= AppearTime);

    public void CalculateAppearTime(TimeFrameProfile _timeFrameProfile)
    {
        AppearTime = HitTime - _timeFrameProfile.LiveFrames - _timeFrameProfile.InitFrames;
    }

    public new string ToString()
    {
        return ("AppearTime: " + AppearTime);
    }
}
