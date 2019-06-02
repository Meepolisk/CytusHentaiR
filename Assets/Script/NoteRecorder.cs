using RTool.Attribute;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRecorder : CytusPlayer
{
    [SerializeField]
    private CytusPlayer MainCytusPlayer = null;

    [SerializeField]
    private TouchEffectPoolManager pool = null;

    [SerializeField, ReadOnly]
    private List<NoteProfile> noteList = null;

    [SerializeField]
    private Rect playZone;
    
    private Coroutine coroutine = null;
    public override bool IsPlaying => MainCytusPlayer.IsPlaying;
    public override float CurrentTime => MainCytusPlayer.CurrentTime;
    public override double Duration => MainCytusPlayer.Duration;

    public override void Pause()
    {

    }

    public override void Play()
    {
        //EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;

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
        //EasyTouch.On_TouchStart -= EasyTouch_On_TouchStart;
        if (coroutine != null)
            StopCoroutine(coroutine);
        if (noteList != null)
        {
            SongStorer.Save(noteList);
        }
    }

    private void EasyTouch_On_TouchStart(Gesture gesture)
    {
        var position = gesture.position;
        RecordNote(position);
    }
    private void ToWorldPosition(Vector2 position)
    {

    }
    private void RecordNote(Vector2 scaledPos)
    {
        Debug.Log("Record: " + scaledPos);
        noteList.Add(new NoteProfile(CurrentTime, scaledPos));
    }
    private void CreateEffectTouch(Vector2 position)
    {
        pool.Spawn(position);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(playZone.center, playZone.size);
    }
    private readonly Dictionary<KeyCode, Vector2> bubbleEmuPos = new Dictionary<KeyCode, Vector2>
    {
        { KeyCode.Keypad1, new Vector2(1f /6f, 1f /6f) },
        { KeyCode.Keypad2, new Vector2(3f /6f, 1f /6f) },
        { KeyCode.Keypad3, new Vector2(5f /6f, 1f /6f) },
        { KeyCode.Keypad4, new Vector2(1f /6f, 3f /6f) },
        { KeyCode.Keypad5, new Vector2(3f /6f, 3f /6f) },
        { KeyCode.Keypad6, new Vector2(5f /6f, 3f /6f) },
        { KeyCode.Keypad7, new Vector2(1f /6f, 5f /6f) },
        { KeyCode.Keypad8, new Vector2(3f /6f, 5f /6f) },
        { KeyCode.Keypad9, new Vector2(5f /6f, 5f /6f) }
    };
    private void Update()
    {
        if (IsPlaying)
        {
            RecordDebug();
        }
    }
    private void RecordDebug()
    {
        //Debug.Log(Time.time);
        foreach (var item in bubbleEmuPos)
        {
            if (Input.GetKeyDown(item.Key))
            {
                Vector2 scaledPos = item.Value;
                Vector2 realPos = new Vector2(playZone.x + (scaledPos.x * playZone.width), playZone.y + (scaledPos.y * playZone.height));
                CreateEffectTouch(realPos);
                RecordNote(scaledPos);
            }
        }
    }
#endif
}
