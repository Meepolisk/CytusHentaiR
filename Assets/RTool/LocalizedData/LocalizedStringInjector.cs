using UnityEngine;
using UnityEngine.Events;
using RTool.Localization;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LocalizedStringInjector : LocalizationInjector<string>
{
    [System.Serializable]
    public class StringEvent : UnityEvent<string> { }

    public StringEvent stringChanged;

    protected override void RefreshData()
    {
        stringChanged.Invoke(localizedData);
    }

#if UNITY_EDITOR
    protected override string DrawDataPreview(string _data)
    {
        return EditorGUILayout.TextArea(_data, GUILayout.Height(80));
    }
#endif
}
