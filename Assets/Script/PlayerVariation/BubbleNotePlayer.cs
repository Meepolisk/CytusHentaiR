using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleNotePlayer : CoreFollower
{
#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField]
    private bool isDebugMode = false;
    public bool IsDebugMode => isDebugMode;
#endif

    [Header("Component Ref")]
    [SerializeField]
    protected BubbleNotePoolManager pool = null;

    protected Rect playZone => CorePlayer.PlayZone;
    
    private Queue<NoteProfile> noteQueue { get; set; }
    private Coroutine coroutine = null;
    
    public void Setup()
    {
        PrepareQueue();
    }

    public override void OnPlay()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(_Play());
    }

    public override void OnPause() { }
    public override void OnResume() { }

    public override void OnStop()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }

    private IEnumerator _Play()
    {
        yield return new WaitUntil(() => { return CorePlayer.IsPlaying == true;});
        while (CorePlayer.IsPlaying == true)
        {
            PerformAction();
            yield return null;
        }
        OnStop();
    }


    private NoteProfile nextNote { get; set; }
    private void PrepareQueue()
    {
        noteQueue = SongSelector.LoadQueue();

        if (noteQueue.Count == 0)
            return;

        Debug.Log("Song Loaded: " + noteQueue.Count + " notes");
        TimeFrameProfile timeFrameProfile = pool.Prefabs.TimeFrameProfile;
        foreach (var item in noteQueue)
        {
            item.CalculateAppearTime(timeFrameProfile);
        }
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
        Vector2 realPos = CalculateNewPos(nextNote);
        BubbleNote newNote = pool.Spawn(realPos);
        newNote.Setup(this);
        newNote.Refresh(nextNote);
        //dequeue

        if (noteQueue.Count > 0)
            nextNote = noteQueue.Dequeue();
        else
            nextNote = null;
    }
    protected virtual Vector2 CalculateNewPos(NoteProfile note)
    {
        return (new Vector2(playZone.x + (note.Position.x * playZone.width), playZone.y + (note.Position.y * playZone.height)));
    }
#if UNITY_EDITOR
    private readonly Dictionary<KeyCode, Vector2> bubbleEmuPos = new Dictionary<KeyCode, Vector2>
    {
        { KeyCode.Alpha1, new Vector2(1/6, 1/6) },
        { KeyCode.Alpha2, new Vector2(3/6, 1/6) },
        { KeyCode.Alpha3, new Vector2(5/6, 1/6) },
        { KeyCode.Alpha4, new Vector2(1/6, 3/6) },
        { KeyCode.Alpha5, new Vector2(3/6, 3/6) },
        { KeyCode.Alpha6, new Vector2(5/6, 3/6) },
        { KeyCode.Alpha7, new Vector2(1/6, 5/6) },
        { KeyCode.Alpha8, new Vector2(3/6, 5/6) },
        { KeyCode.Alpha9, new Vector2(5/6, 5/6) }
    };
#endif

}
