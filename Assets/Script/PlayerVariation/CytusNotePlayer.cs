using RTool.Attribute;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CytusNotePlayer : BubbleNotePlayer
{
    [Header("Config: Cytus")]
    [SerializeField, Range(1, 5), ReadOnlyWhenPlaying]
    private float cytusRoundTime = 2.5f;
    [SerializeField]
    private Transform cytusBar = null;

    private Coroutine cytusCoroutine { get; set; }
    public override void Play()
    {
        base.Play();
        if (cytusCoroutine != null)
            StopCoroutine(cytusCoroutine);
        cytusCoroutine = StartCoroutine(_CytusBarChange());
    }

    private IEnumerator _CytusBarChange()
    {
        yield return new WaitUntil(() => { return MainCytusPlayer.IsPlaying == true;});
        while (MainCytusPlayer.IsPlaying == true)
        {
            UpdateCytusBar();
            yield return null;
        }
        Stop();
    }
    private float GetCytusYPos(float time)
    {
        float round = time / cytusRoundTime;
        float roundScale = (round - (int)round);
        float yScale = (1f - Mathf.Abs((roundScale * 2f) - 1f));
        return playZone.y + (yScale * playZone.height);
    }
    private void UpdateCytusBar()
    {
        cytusBar.transform.position = new Vector3(cytusBar.transform.position.x, GetCytusYPos(CurrentTime), transform.position.z);
    }

    public override void Stop()
    {
        base.Stop();
        if (cytusCoroutine != null)
            StopCoroutine(cytusCoroutine);
    }

    protected override Vector2 CalculateNewPos(NoteProfile note)
    {
        return (new Vector2(playZone.x + (note.Position.x * playZone.width), GetCytusYPos(note.HitTime)));
    }

}
