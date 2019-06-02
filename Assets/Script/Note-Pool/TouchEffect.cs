using UnityEngine;
using System.Collections;
using PoolingObject;

public class TouchEffect : PoolingObject.Object
{
    [Header("Component Ref")]
    [SerializeField]
    private Animator anim = null;

    public override void OnSpawn()
    {
        base.OnSpawn();
        anim.SetTrigger("Play");
    }
}
