using UnityEngine;
using UnityEngine.Events;
using RTool.Localization;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LocalizedTextureInjector : LocalizationInjector<Texture>
{
    [System.Serializable]   
    public class TextureEvent : UnityEvent<Texture> { }

    public TextureEvent textureChanged;

    protected override void RefreshData()
    {
        textureChanged.Invoke(localizedData);
    }

#if UNITY_EDITOR
    private const float maxSize = 150f;
    protected override Texture DrawDataPreview(Texture _data)
    {
        GUILayout.BeginVertical();
        GUI.enabled = true;
        GUILayout.Label(_data, GUILayout.Height(maxSize));
        GUI.enabled = isEditMode;
        Texture res = (UnityEngine.Texture)EditorGUILayout.ObjectField(_data, typeof(Texture2D), false);
        GUILayout.EndVertical();
        return res;
    }
#endif
}
