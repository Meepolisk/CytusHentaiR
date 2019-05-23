using UnityEngine;
using System.Collections;

public class BubbleNotePoolManager : PoolingObject.Manager<BubbleNote>
{
    [SerializeField]
    private BubbleNote _prefab = null;

    protected override BubbleNote prefabs
    {
        get
        {
            return _prefab;
        }
    }
}
