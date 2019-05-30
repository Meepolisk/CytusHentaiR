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
        anim.Rebind();
        EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;
    }

    public override void Kill()
    {
        base.Kill();
        EasyTouch.On_TouchStart -= EasyTouch_On_TouchStart;
    }
    //private void OnEnable()
    //{
    //    EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;
    //}

    //private void OnDisable()
    //{
    //    EasyTouch.On_TouchStart -= EasyTouch_On_TouchStart;
    //}

    private void EasyTouch_On_TouchStart(Gesture gesture)
    {
        if (gesture.pickObject == easyTouchDetector.gameObject)
        {
            PlayerHit();
        }
    }

    public void PlayerHit()
    {
        anim.SetTrigger("Hit");
        HitType hitType = Calculate(Player.CurrentTime);
        switch (hitType)
        {
            case HitType.Perfect:
                Debug.Log("Perfect");
                break;
            case HitType.Great:
                Debug.Log("Great");
                break;
            case HitType.Good:
                Debug.Log("Good");
                break;
            default:
                Debug.Log("Bad");
                break;
        }
    }
}
