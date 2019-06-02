using UnityEngine;
using System.Collections;

public class BubbleNotePoolManager : PoolingObject.Manager<BubbleNote>
{
    [SerializeField]
    private BubbleNote prefab = null;
    public override BubbleNote Prefabs => prefab;
}
