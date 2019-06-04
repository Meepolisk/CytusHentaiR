using RTool.Attribute;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CytusNotePlayer : BubbleNotePlayer
{
    [Header("Config: Cytus")]
    [SerializeField, Range(1, 5), ReadOnlyWhenPlaying]
    private float cytusBarDuration = 1.5f;
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
        float timeTick = 0; float sign = 1f;
        while (MainCytusPlayer.IsPlaying == true)
        {
            timeTick += (sign * Time.deltaTime);
            if (timeTick >= cytusBarDuration)
            {
                timeTick = cytusBarDuration;
                sign = -1;
            }
            else if (timeTick <= 0)
            {
                timeTick = 0;
                sign = 1;
            }
            cytusBar.transform.position = new Vector2(cytusBar.transform.position.x, playZone.x + ((timeTick / cytusBarDuration) * playZone.width));
            yield return null;
        }
        Stop();
    }

    public override void Stop()
    {
        base.Stop();
        if (cytusCoroutine != null)
            StopCoroutine(cytusCoroutine);
    }

    protected override Vector2 CalculateNewPos(Vector2 _pos)
    {
        return (new Vector2(playZone.x + (_pos.x * playZone.width), cytusBar.transform.position.y));
    }

}
