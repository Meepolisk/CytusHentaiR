using RTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRecorder : CoreFollower
{
    [SerializeField]
    private TouchEffectPoolManager pool = null;

    [SerializeField, ReadOnly]
    private List<NoteProfile> noteList = null;


    private Rect playZone => CorePlayer.PlayZone;
    
    private Coroutine coroutine = null;

    public override void OnPlay()
    {
        EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;

        Debug.Log("RecordPlay");
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(_Play());
    }
    public override void OnPause() { }
    public override void OnResume() { }

    private IEnumerator _Play()
    {
        yield return new WaitUntil(() => { return (CorePlayer.IsPlaying == true); });
        while (CorePlayer.IsPlaying == true)
        {
            yield return null;
        }
        OnStop();
    }

    public override void OnStop()
    {
        Debug.Log("RecordStopped");
        EasyTouch.On_TouchStart -= EasyTouch_On_TouchStart;
        if (coroutine != null)
            StopCoroutine(coroutine);
        if (noteList != null)
        {
            SongSelector.Save(noteList);
        }
    }

    private void EasyTouch_On_TouchStart(Gesture gesture)
    {
        var position = gesture.position;
        RecordNote(position);
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
    private const float verticalSteps = 6f;
    private const float horizontalSteps = 6f;
    private readonly Dictionary<KeyCode, Vector2> bubbleEmuPos = new Dictionary<KeyCode, Vector2>
    {
        { KeyCode.Keypad1, new Vector2(1f /horizontalSteps, 1f /verticalSteps) },
        { KeyCode.Keypad2, new Vector2(3f /horizontalSteps, 1f /verticalSteps) },
        { KeyCode.Keypad3, new Vector2(5f /horizontalSteps, 1f /verticalSteps) },
        { KeyCode.Keypad4, new Vector2(1f /horizontalSteps, 3f /verticalSteps) },
        { KeyCode.Keypad5, new Vector2(3f /horizontalSteps, 3f /verticalSteps) },
        { KeyCode.Keypad6, new Vector2(5f /horizontalSteps, 3f /verticalSteps) },
        { KeyCode.Keypad7, new Vector2(1f /horizontalSteps, 5f /verticalSteps) },
        { KeyCode.Keypad8, new Vector2(3f /horizontalSteps, 5f /verticalSteps) },
        { KeyCode.Keypad9, new Vector2(5f /horizontalSteps, 5f /verticalSteps) },
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
