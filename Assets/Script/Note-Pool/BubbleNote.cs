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
        }
    }

    private const string animParamHit = "Hit";
    private const string animParamHitType = "HitType";
    protected override void Scoring(HitType hitType)
    {
        anim.SetInteger(animParamHitType, (int)hitType);
        anim.SetTrigger(animParamHit);
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Player.IsDebugMode && Player.CurrentTime >= (NoteProfile.HitTime - Time.deltaTime / 2f))
        {
            PlayerHit();
        }
    }
#endif
}
