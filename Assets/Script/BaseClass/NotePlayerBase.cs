//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class NotePlayerBase : CytusPlayer
//{
//#if UNITY_EDITOR
//    [Header("Debug")]
//    [SerializeField]
//    private bool isDebugMode = false;
//    public bool IsDebugMode => isDebugMode;
//#endif

//    [Header("Component Ref")]
//    [SerializeField]
//    protected CytusPlayer MainCytusPlayer = null;

//    [SerializeField]
//    protected Rect playZone;

//    protected Queue<NoteProfile> noteQueue { get; set; }

//    protected Coroutine coroutine = null;
//    public override bool IsPlaying => MainCytusPlayer.IsPlaying;
//    public override float CurrentTime => MainCytusPlayer.CurrentTime;
//    public override double Duration => MainCytusPlayer.Duration;
    
//    public void Setup()
//    {
//        PrepareQueue();
//    }

//    public override void Pause()
//    {
//    }
//    public override void Play()
//    {
//        if (coroutine != null)
//            StopCoroutine(coroutine);
//        coroutine = StartCoroutine(_Play());
//    }

//    private IEnumerator _Play()
//    {
//        yield return new WaitUntil(() => { return MainCytusPlayer.IsPlaying == true;});
//        while (MainCytusPlayer.IsPlaying == true)
//        {
//            PerformAction();
//            yield return null;
//        }
//        Stop();
//    }

//    public override void Stop()
//    {
//        if (coroutine != null)
//            StopCoroutine(coroutine);
//    }


//    private NoteProfile nextNote { get; set; }
//    private void PrepareQueue()
//    {
//        noteQueue = SongSelector.LoadQueue();

//        if (noteQueue.Count == 0)
//            return;

//        Debug.Log("Song Loaded: " + noteQueue.Count + " notes");
//        TimeFrameProfile timeFrameProfile = pool.Prefabs.TimeFrameProfile;
//        foreach (var item in noteQueue)
//        {
//            item.CalculateAppearTime(timeFrameProfile);
//        }
//        nextNote = noteQueue.Dequeue();
//    }
//    private void PerformAction()
//    {
//        CheckToPullNote();
//    }
//    private void CheckToPullNote()
//    {
//        if (nextNote != null && nextNote.CanBePull(CurrentTime))
//        {
//            SpawnNote();
//            CheckToPullNote();
//        }
//    }
//    private void SpawnNote()
//    {
//        Vector2 realPos = new Vector2(playZone.x + (nextNote.Position.x * playZone.width), playZone.y + (nextNote.Position.y * playZone.height));
//        BubbleNote newNote = pool.Spawn(realPos);
//        newNote.Setup(this);
//        newNote.Refresh(nextNote);
//        //Debug.Log("Spawn: " + nextNote.ToString());
//        //dequeue

//        if (noteQueue.Count > 0)
//            nextNote = noteQueue.Dequeue();
//        else
//        {
//            nextNote = null;
//            Stop();
//        }
//    }
//#if UNITY_EDITOR
//    private void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.green;
//        Gizmos.DrawWireCube(playZone.center, playZone.size);
//    }
//    private readonly Dictionary<KeyCode, Vector2> bubbleEmuPos = new Dictionary<KeyCode, Vector2>
//    {
//        { KeyCode.Alpha1, new Vector2(1/6, 1/6) },
//        { KeyCode.Alpha2, new Vector2(3/6, 1/6) },
//        { KeyCode.Alpha3, new Vector2(5/6, 1/6) },
//        { KeyCode.Alpha4, new Vector2(1/6, 3/6) },
//        { KeyCode.Alpha5, new Vector2(3/6, 3/6) },
//        { KeyCode.Alpha6, new Vector2(5/6, 3/6) },
//        { KeyCode.Alpha7, new Vector2(1/6, 5/6) },
//        { KeyCode.Alpha8, new Vector2(3/6, 5/6) },
//        { KeyCode.Alpha9, new Vector2(5/6, 5/6) }
//    };
//#endif

//}
