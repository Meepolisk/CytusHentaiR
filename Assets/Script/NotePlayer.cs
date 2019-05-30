using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePlayer : CytusPlayer
{
    [SerializeField]
    private CytusPlayer MainCytusPlayer = null;
    [SerializeField]
    private BubbleNotePoolManager pool = null;

    [SerializeField]
    private List<NoteProfile> noteList = null;
    private Queue<NoteProfile> noteQueue { get; set; }
    
    private Coroutine coroutine = null;
    public override bool IsPlaying => MainCytusPlayer.IsPlaying;
    public override float CurrentTime => MainCytusPlayer.CurrentTime;
    public override double Duration => MainCytusPlayer.Duration;
    //private TimeFrameProfile timeFramProfile => pool.Prefabs.TimeFrameProfile;

    public override void Pause()
    {
    }

    public override void Play()
    {
        PrepareQueue();
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(_Play());
    }

    private IEnumerator _Play()
    {
        while (MainCytusPlayer.IsPlaying == true)
        {
            PerformAction();
            yield return null;
        }
        Stop();
    }

    public override void Stop()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }


    private NoteProfile nextNote { get; set; }
    private void PrepareQueue()
    {
        TimeFrameProfile timeFrameProfile = pool.Prefabs.TimeFrameProfile;
        foreach (var item in noteList)
        {
            item.CalculateAppearTime(timeFrameProfile);
        }
        noteQueue = new Queue<NoteProfile>(noteList);
        nextNote = noteQueue.Dequeue();
    }
    private void PerformAction()
    {
        //Debug.Log("Time: " + CurrentTime);
        CheckToPullNote();
    }
    private void CheckToPullNote()
    {
        if (nextNote != null && nextNote.CanBePull(CurrentTime))
        {
            SpawnNote();
            CheckToPullNote();
        }
    }
    private void SpawnNote()
    {
        var newNote = pool.Spawn(nextNote.Position);
        newNote.Setup(this);
        newNote.Refresh(nextNote);
        Debug.Log("Spawn: " + nextNote.ToString());
        //dequeue

        if (noteQueue.Count > 0)
            nextNote = noteQueue.Dequeue();
        else
        {
            nextNote = null;
            Stop();
        }
    }
    
}
