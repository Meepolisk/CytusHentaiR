using UnityEngine;
using System.Collections;

public class TouchEffectPoolManager : PoolingObject.Manager<TouchEffect>
{
    [SerializeField]
    private TouchEffect prefab = null;
    public override TouchEffect Prefabs => prefab;
}
