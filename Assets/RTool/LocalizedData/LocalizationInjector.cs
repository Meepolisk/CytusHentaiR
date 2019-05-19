using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTool.Localization
{
    public abstract class LocalizationInjector : MonoBehaviour
    {
        [HideInInspector, SerializeField]
        protected LocalizedDataManager source;
                
        [HideInInspector, SerializeField]
        private string _keyID;
        public string keyID
        {
            get
            {
                return _keyID;
            }
#if UNITY_EDITOR
            protected set
            {
                if (_keyID == value)
                    return;

                _keyID = value;
            }
#endif
        }

#if UNITY_EDITOR
        protected bool isEditMode = false;
        protected abstract List<string> IDList { get; }
        protected abstract void EditorDrawDataPreview();
        protected abstract void EnterEditMode();
        protected abstract void ExitEditMode(bool _willSave);

        [CustomEditor(typeof(LocalizationInjector),true)]
        private class EditorDrawer : Editor
        {
            private LocalizationInjector handler;
            private LocalizedDataManager manager;

            private void OnEnable()
            {
                handler = target as LocalizationInjector;
                handler.isEditMode = false;
            }
            private void OnDisable()
            {
                handler.ExitEditMode(false);
            }

            public override void OnInspectorGUI()
            {
                if (handler == null)
                    return;

                serializedObject.Update();
                manager = handler.source;
                handler.source = EditorGUILayout.ObjectField("Source", handler.source, typeof(LocalizedDataManager), false) as LocalizedDataManager;
                if (manager == null)
                {
                    DrawNoManagerError();
                }
                else
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    DrawKeySelector();
                    DrawDataPreview();
                    DrawEditSaveCancelButton();
                    EditorGUILayout.EndVertical();

                    DrawCustomHeader();
                    DrawPropertiesExcluding(serializedObject, "m_Script");
                }
                serializedObject.ApplyModifiedProperties();
            }

            private void DrawNoManagerError()
            {
                DisplayError("No " + typeof(LocalizedDataManager).Name + " found!");
            }

            private void DrawCustomHeader()
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Script Implementations", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndHorizontal();
            }

            private void DrawKeySelector()
            {
                GUI.enabled = !handler.isEditMode;
                EditorGUI_IDSelector();
                GUI.enabled = true;
            }

            const float dropBtnSize = 17f;
            const float titleWidth = 60f;
            bool isValidKey = true;
            private void EditorGUI_IDSelector()
            {
                Rect rect = EditorGUILayout.GetControlRect();
                Rect titleRect = new Rect(rect.x, rect.y, titleWidth, EditorGUIUtility.singleLineHeight);
                Rect fieldRect = new Rect(titleRect.xMax, rect.y, rect.width - titleRect.width - dropBtnSize, EditorGUIUtility.singleLineHeight);
                Rect buttonRect = new Rect(fieldRect.xMax, rect.y, dropBtnSize, EditorGUIUtility.singleLineHeight);

                GUI.Label(titleRect, "KeyID");
                string data = handler.keyID;
                if (handler.IDList.Contains(data) == false)
                {
                    isValidKey = false;
                    GUI.backgroundColor = new Color(1f, 0.3f, 0.3f, 1f);
                }
                else
                {
                    Undo.RecordObject(handler, "KeyID changed");
                    isValidKey = true;
                }
                handler.keyID = EditorGUI.TextField(fieldRect, handler.keyID);
                GUI.backgroundColor = Color.white;

                string[] popupData = FilterList(data, handler.IDList);
                GUI.enabled = (popupData.Length > 1);
                int selectedIndex = EditorGUI.Popup(buttonRect, -1, popupData, EditorStyles.miniButtonRight);
                GUI.Label(buttonRect, (popupData.Length > 1) ? " ▾" : " ");
                if (selectedIndex >= 0 && handler.isEditMode == false)
                {
                    Undo.RecordObject(handler, "KeyID changed");
                    isValidKey = true;
                    handler.keyID = popupData[selectedIndex];
                }
                GUI.enabled = true;
            }
            const float maxDataSuggestRow = 10;
            public string[] FilterList(string _value, List<string> _list)
            {
                List<string> result = new List<string>();
                int count = 0;
                foreach (var item in _list)
                {
                    if (item.Contains(_value))
                    {
                        result.Add(item);
                        count++;
                        if (count > maxDataSuggestRow)
                            break;
                    }
                }
                return result.ToArray();
            }

            private void DisplayError(string _error)
            {
                GUIStyle redStyle = new GUIStyle();
                redStyle.normal.textColor = Color.red;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(_error, redStyle);

                EditorGUILayout.EndVertical();
            }

            private void DrawDataPreview()
            {
                handler.EditorDrawDataPreview();
            }
            private void DrawEditSaveCancelButton()
            {
                if (Application.isPlaying)
                    return;

                GUI.enabled = isValidKey;
                EditorGUILayout.BeginHorizontal();
                if (handler.isEditMode)
                {
                    if (GUILayout.Button("Save"))
                    {
                        handler.ExitEditMode(true);
                    }
                    if (GUILayout.Button("Cancel"))
                    {
                        handler.ExitEditMode(false);
                    }
                }
                else
                {
                    if (GUILayout.Button("Edit"))
                    {
                        handler.EnterEditMode();
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
            }
        }
#endif
    }
    
    public abstract class LocalizationInjector<T> : LocalizationInjector
    {
        protected UnityEvent<T> objectLoaded;
        protected abstract void RefreshData();

        protected T localizedData
        {
            get
            {
                return source.GetData<T>(keyID);
            }
#if UNITY_EDITOR
            set
            {
                source.SetData(keyID, value);
            }
#endif
        }

        protected virtual void OnEnable()
        {
            source.onChangeDefaultLanguage += RefreshData;
            RefreshData();
        }
        protected virtual void OnDisable()
        {
            source.onChangeDefaultLanguage -= RefreshData;
        }
        protected virtual void OnBecameVisible()
        {
            RefreshData();
        }

#if UNITY_EDITOR
        protected abstract T DrawDataPreview(T _data);
        protected override List<string> IDList
        {
            get
            {
                return source.GetIDList<T>();
            }
        }
        protected sealed override void EditorDrawDataPreview()
        {
            if (isEditMode)
            {
                tmpData = DrawDataPreview(tmpData);
            }
            else
            {
                GUI.enabled = isEditMode;
                DrawDataPreview(localizedData);
                GUI.enabled = true;
            }
        }

        protected sealed override void EnterEditMode()
        {
            isEditMode = true;
            tmpData = localizedData;
        }
        protected sealed override void ExitEditMode(bool _willSave)
        {
            isEditMode = false;
            if (_willSave)
            {
                localizedData = tmpData;
                //tmpData = source.GetLocalizationDictionary<T>().DefaultData;
            }
        }

        private T tmpData { get; set; }
#endif
    }
}

