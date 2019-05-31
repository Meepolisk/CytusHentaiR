using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRecorder : CytusPlayer
{
    [SerializeField]
    private CytusPlayer MainCytusPlayer = null;

    private List<NoteProfile> noteList { get; set; }

    [SerializeField]
    private GameObject prefabs = null;
    
    private Coroutine coroutine = null;
    public override bool IsPlaying => MainCytusPlayer.IsPlaying;
    public override float CurrentTime => MainCytusPlayer.CurrentTime;
    public override double Duration => MainCytusPlayer.Duration;

    public override void Pause()
    {

    }

    public override void Play()
    {
        noteList = new List<NoteProfile>();
        EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;

        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(_Play());
    }

    private IEnumerator _Play()
    {
        while (MainCytusPlayer.IsPlaying == true)
        {
            yield return null;
        }
        Stop();
    }

    public override void Stop()
    {
        EasyTouch.On_TouchStart -= EasyTouch_On_TouchStart;
        if (coroutine != null)
            StopCoroutine(coroutine);
        if (noteList != null)
        {
            NoteProfileStorer.Save(noteList);
        }
    }

    private void EasyTouch_On_TouchStart(Gesture gesture)
    {
        var position = gesture.position;
        GameObject debug = Instantiate(prefabs, this.transform, false);
        debug.transform.localPosition = position;
        RecordNote(position);
    }
    private void RecordNote(Vector2 position)
    {
        noteList.Add(new NoteProfile(CurrentTime, position));
    }
}
