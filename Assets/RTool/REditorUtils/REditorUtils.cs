#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace REditor
{
    /// <summary>
    /// Inspector of Unity with GUI control id handle support
    /// </summary>
    public abstract class RInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            UpdateFocusedControl();
            DrawGUI();
        }
        protected abstract void DrawGUI();

        public string ActiveControl { get; private set; }
        public string UnfocusedControl { get; private set; }
        public string FocusedControl { get; private set; }
        private void UpdateFocusedControl()
        {
            FocusedControl = null;
            UnfocusedControl = null;
            string checkingControl = GUI.GetNameOfFocusedControl();
            if (checkingControl != ActiveControl)
            {
                if (UnfocusedControl == null)
                {
                    UnfocusedControl = ActiveControl;
                }
                if (FocusedControl == null)
                {
                    FocusedControl = checkingControl;
                }
                ActiveControl = checkingControl;
            }
        }
    }

    /// <summary>
    /// Inspector of Unity with GUI control id handle support
    /// </summary>
    public abstract class RInspector<T> : RInspector where T : UObject
    {
        internal T handler { private set; get; }

        protected virtual void OnEnable() => handler = target as T;

    }
    public static class REditorUtils
    {
        public static List<GameObject> GetGameObjectLoaded()
        {
            List<GameObject> results = new List<GameObject>();
            if (!Application.isPlaying)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (s.isLoaded)
                    {
                        var allGameObjects = s.GetRootGameObjects();
                        for (int j = 0; j < allGameObjects.Length; j++)
                        {
                            Debug.Log(allGameObjects[j].name, allGameObjects[j]);
                            results.Add(allGameObjects[j]);
                        }
                    }
                }
            }
            return results;
        }
        
        private static GUIStyle _redFont;
        public static GUIStyle redFont
        {
            get
            {
                if (_redFont == null)
                {
                    _redFont = new GUIStyle();
                    _redFont.normal.textColor = Color.red;
                }
                return _redFont;
            }
        }
        private static GUIStyle _grayFont;
        public static GUIStyle grayFont
        {
            get
            {
                if (_grayFont == null)
                {
                    _grayFont = new GUIStyle();
                    _grayFont.normal.textColor = Color.gray;
                }
                return _grayFont;
            }
        }

        private static readonly Dictionary<Type, Func<Rect, object, object>> _Fields =
        new Dictionary<Type, Func<Rect, object, object>>()
        {
        { typeof(int), (rect, value) => EditorGUI.IntField(rect, (int)value) },
        { typeof(float), (rect, value) => EditorGUI.FloatField(rect, (float)value) },
        { typeof(string), (rect, value) => EditorGUI.TextField(rect, (string)value) },
        { typeof(bool), (rect, value) => EditorGUI.Toggle(rect, (bool)value) },
        { typeof(Vector2), (rect, value) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value) },
        { typeof(Vector3), (rect, value) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value) },
        { typeof(Bounds), (rect, value) => EditorGUI.BoundsField(rect, (Bounds)value) },
        { typeof(Rect), (rect, value) => EditorGUI.RectField(rect, (Rect)value) }
        };
        private static readonly Dictionary<Type, Func<GUIContent, object, GUILayoutOption[], object>> _FieldsGuiLayout =
            new Dictionary<Type, Func<GUIContent, object, GUILayoutOption[], object>>()
        {
        { typeof(int), (content, value, option) => EditorGUILayout.IntField(content, (int)value, option) },
        { typeof(float), (content, value, option) => EditorGUILayout.FloatField(content, (float)value, option) },
        { typeof(string), (content, value, option) => EditorGUILayout.TextField(content, (string)value, option) },
        { typeof(bool), (content, value, option) => EditorGUILayout.Toggle(content, (bool)value, option) },
        { typeof(Vector2), (content, value, option) => EditorGUILayout.Vector2Field(content, (Vector2)value, option) },
        { typeof(Vector3), (content, value, option) => EditorGUILayout.Vector3Field(content, (Vector3)value, option) },
        { typeof(Bounds), (content, value, option) => EditorGUILayout.BoundsField(content, (Bounds)value, option) },
        { typeof(Rect), (content, value, option) => EditorGUILayout.RectField(content, (Rect)value, option) }
        };
        
        public static T DoField<T>(Rect rect, T value, string errorText = "Unsupported value", GUIStyle errorGUIStyle = null)
        {
            Type type = typeof(T);
            Func<Rect, object, object> field;
            if (_Fields.TryGetValue(type, out field))
                return (T)field(rect, value);

            if (type.IsEnum)
                return (T)(object)EditorGUI.EnumPopup(rect, (Enum)(object)value);

            if (typeof(UObject).IsAssignableFrom(type))
                return (T)(object)EditorGUI.ObjectField(rect, (UObject)(object)value, type, true);

            GUI.Label(rect, errorText, (errorGUIStyle == null) ? redFont : errorGUIStyle);
            return value;
        }
        public static object DoFieldGUILayout(object _value, Type _type, GUIContent guiContent = null, params GUILayoutOption[] _options)
        {
            if (guiContent == null)
                guiContent = GUIContent.none;

            Func<GUIContent, object, GUILayoutOption[], object> field;
            if (_FieldsGuiLayout.TryGetValue( _type, out field))
                return field(guiContent, _value, _options);

            if (_type.IsEnum)
                return EditorGUILayout.EnumPopup(guiContent, (Enum)_value, _options);
            
            if (typeof(UObject).IsAssignableFrom(_type))
            {
                return EditorGUILayout.ObjectField(guiContent, (UObject)_value, _type, true, _options);
            }

            EditorGUILayout.LabelField("[" + _type.Name + "]", redFont);
            return _value;
        }
        public static T DoFieldGUILayout<T>(T _value, GUIContent guiContent = null, params GUILayoutOption[] _options)
        {
            return (T)DoFieldGUILayout(_value, typeof(T), guiContent, _options);
        }
    }
    
}
#endif