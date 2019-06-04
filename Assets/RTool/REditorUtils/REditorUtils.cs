#if UNITY_EDITOR
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

namespace REditor
{
    public class UnityObjectEditor<T> : Editor where T : UObject
    {
        private T _handler;
        public T handler
        {
            protected set
            {
                _handler = value;
            }
            get
            {
                return _handler;
            }
        }

        protected virtual void OnEnable()
        {
            _handler = (T)target;
        }
    }
    public class REditorUtils
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

        private static readonly Dictionary<Type, Func<object, GUILayoutOption[], object>> _FieldsGuiLayout =
            new Dictionary<Type, Func<object, GUILayoutOption[], object>>()
        {
        { typeof(int), (value, option) => EditorGUILayout.IntField((int)value, option) },
        { typeof(float), (value, option) => EditorGUILayout.FloatField((float)value, option) },
        { typeof(string), (value, option) => EditorGUILayout.TextArea((string)value, option) },
        { typeof(bool), (value, option) => EditorGUILayout.Toggle((bool)value, option) },
        { typeof(Vector2), (value, option) => EditorGUILayout.Vector2Field(GUIContent.none, (Vector2)value, option) },
        { typeof(Vector3), (value, option) => EditorGUILayout.Vector3Field(GUIContent.none, (Vector3)value, option) },
        { typeof(Bounds), (value, option) => EditorGUILayout.BoundsField((Bounds)value, option) },
        { typeof(Rect), (value, option) => EditorGUILayout.RectField((Rect)value, option) }
        };

        public static T DoFieldGUILayout<T>(T _value, string errorText = "Unsupported value", GUIStyle errorGUIStyle = null, params GUILayoutOption[] _options)
        {
            Type type = typeof(T);
            Func<object, GUILayoutOption[], object> field;
            if (_FieldsGuiLayout.TryGetValue(type, out field))
                return (T)field(_value, _options);

            if (type.IsEnum)
                return (T)(object)EditorGUILayout.EnumPopup((Enum)(object)_value, _options);

            if (typeof(UObject).IsAssignableFrom(type))
                return (T)(object)EditorGUILayout.ObjectField((UObject)(object)_value, type, true, _options);

            EditorGUILayout.LabelField(errorText, (errorGUIStyle == null) ? redFont : errorGUIStyle);
            return _value;
        }
        public static T DoFieldGUILayout<T>(T _value, params GUILayoutOption[] _options)
        {
            return DoFieldGUILayout(_value, "Unsupported value", null, _options);
        }

    }
    
}
#endif