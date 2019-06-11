using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;

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
        AppearTime = HitTime - _timeFrameProfile.LiveFrames;
    }

    public new string ToString()
    {
        return ("AppearTime: " + AppearTime);
    }
}

[System.Serializable]
public class SongNoteProfile
{
    [SerializeField]
    private string name = "";
    public string Name => name;
    [SerializeField]
    private string composer = "";
    public string Composer => composer;
    [SerializeField]
    private string singer = "";
    public string Singer => singer;

    [SerializeField]
    private Sprite icon = null;
    public Sprite Icon => icon;
    [SerializeField]
    private Sprite previewImage = null;
    public Sprite PreviewImage => previewImage;

    [SerializeField]
    private AudioClip audioClip = null;
    public AudioClip AudioClip => audioClip;

    [SerializeField]
    private float bpm = 120;
    public float BPM => bpm;

    [SerializeField]
    private List<NoteProfile> noteList = new List<NoteProfile>();
    public List<NoteProfile> NoteList => new List<NoteProfile>(noteList);
    public Queue<NoteProfile> NoteQueue => new Queue<NoteProfile>(noteList);

    public int NoteCount => noteList.Count;

    public void UpdateNote(List<NoteProfile> newNoteList)
    {
        noteList = newNoteList;
    }
}
