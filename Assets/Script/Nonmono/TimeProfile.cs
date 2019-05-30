using UnityEngine;
using System.Collections;

[System.Serializable]
public class TimeFrameProfile
{
    [SerializeField, Range(0, 1)]
    private float initFrames = 0.2f;
    public float InitFrames => initFrames;

    [SerializeField, Range(0, 2)]
    private float liveFrames = 1f;
    public float LiveFrames => liveFrames;

    [SerializeField, Range(0, 2)]
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