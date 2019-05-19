using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using RTool.LayoutSwitcher;

public class ColorSwitcherInjector : DataSwitcherInjector<Color>
{
    protected override Dictionary<Type, Action<Component, Color>> ComponentDictionary
    {
        get
        {
            return _dictionary;
        }
    }

    [System.Serializable]
    protected class TextureEvent : UnityEvent<Color> { };

    [SerializeField]
    protected TextureEvent onColorLoad;

    protected override void OnManualSwitch(Color _color)
    {
        onColorLoad.Invoke(_color);
    }

    private static readonly Dictionary<Type, Action<Component, Color>> _dictionary
        = new Dictionary<Type, Action<Component, Color>>()
        {
            {
                typeof(Graphic), (_component, _color) =>
                {
                    Graphic graphic = _component as Graphic;
                    graphic.color = _color;
                }
            },
            {
                typeof(SpriteRenderer), (_component, _color) =>
                {
                    SpriteRenderer spriteRenderer = _component as SpriteRenderer;
                    spriteRenderer.color = _color;
                }
            },
        };
}