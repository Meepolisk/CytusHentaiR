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
        [HideInInspector]
        [SerializeField]
        protected LocalizedDataManager source;

        [HideInInspector]
        [SerializeField]
        private string _keyLanguage;
        public string keyLanguage
        {
            get
            {
                return _keyLanguage;
            }
#if UNITY_EDITOR
            protected set
            {
                if (_keyLanguage == value)
                    return;

                _keyLanguage = value;
            }
#endif
        }
        public int keyLanguageID
        {
            get
            {
                return source.languageID.IndexOf(keyLanguage);
            }
        }
        [HideInInspector]
        [SerializeField]
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
        //protected abstract void EditorReset();

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
                    DrawLanguageAndKeySelector();
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

            private void DrawLanguageAndKeySelector()
            {
                GUI.enabled = !handler.isEditMode;
                EditorGUILayout.BeginHorizontal();
                handler.keyLanguage = EditorGUI_LanguageSelector();
                handler.keyID = EditorGUI_IDSelector();
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            private string EditorGUI_LanguageSelector()
            {
                int currentLanguageIndex = 0;
                if (!string.IsNullOrEmpty(handler.keyLanguage))
                {
                    currentLanguageIndex = manager.languageID.IndexOf(handler.keyLanguage) + 1;
                    if (currentLanguageIndex < 0)
                    {
                        currentLanguageIndex = 0;
                    }
                }

                //define new list that have "default" option
                string defaultLanguageLabel = "Default ("+ manager.defaultLanguageID + ")";
                string[] newLanguageList = new string[manager.languageID.Count + 1];
                newLanguageList[0] = defaultLanguageLabel;
                for (int i = 1; i < newLanguageList.Length; i++)
                {
                    newLanguageList[i] = manager.languageID[i - 1];
                }

                currentLanguageIndex = EditorGUILayout.Popup(currentLanguageIndex, newLanguageList);

                //set handler data
                if (currentLanguageIndex == 0)
                    return string.Empty;
                return manager.languageID[currentLanguageIndex - 1];
            }

            private string EditorGUI_IDSelector()
            {
                int currentIdIndex = handler.IDList.IndexOf(handler.keyID);
                if (currentIdIndex < 0)
                    currentIdIndex = 0;

                currentIdIndex = EditorGUILayout.Popup(currentIdIndex, handler.IDList.ToArray());
                return handler.IDList[currentIdIndex];
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
                return source.GetData<T>(keyLanguageID, keyID);
            }
#if UNITY_EDITOR
            set
            {
                source.SetData(keyLanguageID, keyID, value);
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

