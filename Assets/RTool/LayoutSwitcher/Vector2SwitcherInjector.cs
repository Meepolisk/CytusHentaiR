using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UI;
using RTool.LayoutSwitcher;

public class Vector2SwitcherInjector : DataSwitcherInjector<Vector2>
{
    [System.Serializable]
    protected class Vector2Event : UnityEvent<Vector2> { };
    [SerializeField]
    protected Vector2Event onTextureLoaded;

    protected override void OnManualSwitch(Vector2 _texture)
    {
        onTextureLoaded.Invoke(_texture);
    }
}