using UnityEngine;
using System.Collections;

public class BubbleNotePoolManager : PoolingObject.Manager<BubbleNote>
{
    [SerializeField]
    private BubbleNote _prefab = null;

    public override BubbleNote Prefabs
    {
        get
        {
            return _prefab;
        }
    }

}
