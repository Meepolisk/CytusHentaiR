using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using RTool.LayoutSwitcher;

public class StringSwitcherInjector : DataSwitcherInjector<String>
{
    protected override Dictionary<Type, Action<Component, String>> ComponentDictionary
    {
        get
        {
            return _dictionary;
        }
    }

    [System.Serializable]
    protected class StringEvent : UnityEvent<String> { };
    [SerializeField]
    protected StringEvent onStringLoaded;

    protected override void OnManualSwitch(String _string)
    {
        onStringLoaded.Invoke(_string);
    }

    private static readonly Dictionary<Type, Action<Component, String>> _dictionary
        = new Dictionary<Type, Action<Component, String>>()
        {
            {
                typeof(Text), (_component, _text) =>
                {
                    var text = _component as Text;
                    text.text = _text;
                }
            },
        };
}