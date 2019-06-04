using UnityEngine;
using System.Collections;
using PoolingObject;

public class BubbleNote : NoteBase
{
    [Header("Component Ref")]
    [SerializeField]
    private Animator anim = null;
    [SerializeField]
    private Collider easyTouchDetector = null;

    public override void OnSpawn()
    {
        base.OnSpawn();
        transform.SetAsFirstSibling();
        easyTouchDetector.gameObject.SetActive(true);
        EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;
    }

    public override void Kill()
    {
        base.Kill();
        EasyTouch.On_TouchStart -= EasyTouch_On_TouchStart;
    }

    private void EasyTouch_On_TouchStart(Gesture gesture)
    {
        if (gesture.pickObject == easyTouchDetector.gameObject)
        {
            PlayerHit();
            easyTouchDetector.gameObject.SetActive(false);
        }
    }

    private const string animParamHit = "Hit";
    private const string animParamHitType = "HitType";
    protected override void Scoring(HitType hitType)
    {
        anim.SetInteger(animParamHitType, (int)hitType);
        anim.SetTrigger(animParamHit);
    }
}
