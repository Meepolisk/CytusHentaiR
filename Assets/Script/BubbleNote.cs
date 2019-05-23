using UnityEngine;
using System.Collections;
using PoolingObject;

public class BubbleNote : PoolingObject.Object
{
    [Header("Component Ref")]
    [SerializeField]
    private Animator anim = null;
    [SerializeField]
    private Collider easyTouchDetector = null;

    [Header("Prefab pre-config")]
    [SerializeField]
    private float initFrames = 0.2f;
    [SerializeField]
    private float liveFrames = 1f;
    [SerializeField]
    private float decayFrames = 0.6f;
    [SerializeField]
    private float perfectScale = 0.2f;
    [SerializeField]
    private float greateScale = 0.4f;
    [SerializeField]
    private float goodScale = 0.7f;

    //Insetup
    private NotePlayer player { get; set; }

    public void Setup(NotePlayer _player)
    {
        player = _player;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
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

    public void PlayerHit()
    {
        anim.SetTrigger("Hit");
    }
}
