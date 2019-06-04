using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace RTool.Localization
{
    [System.Serializable]
    public abstract class LocalizedDataBase
    {
        [SerializeField]
        protected LocalizedDataManager source;
        
        [SerializeField]
        protected string idKey;

#if UNITY_EDITOR
        //[SerializeField]
        //private bool firstLoad = false;

        [CustomPropertyDrawer(typeof(LocalizedDataBase), true)]
        private class LocalizedStringCollectionDrawer : PropertyDrawer
        {
            const float dropBtnSize = 17f;

            SerializedProperty property;
            ReorderableList reorderableList;
            LocalizedDataManager source;
            LocalizedDataManager.LocalizationDictionary dictionaryRef;
            List<string> idList;
            Type type;

            private void FirstLoad()
            {

            }
            
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                this.property = property;
                EditorGUI.BeginProperty(position, label, property);
                {
                    GUIStyle desStyle = new GUIStyle();
                    desStyle.normal.textColor = Color.gray;
                    Rect desRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                    try
                    {
                        dictionaryRef.ShowDataPreview(new Rect(desRect.x, desRect.y, desRect.width, EditorGUIUtility.singleLineHeight),
                            property.FindPropertyRelative("idKey").stringValue);
                    }
                    catch
                    {
                        GUI.Label(desRect, "(" + property.type + ")", desStyle);
                    }
                    float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    Rect chaptersRect = new Rect(position.x, position.y + height, position.width, EditorGUIUtility.singleLineHeight);
                    if (reorderableList == null)
                    {
                        InitReorderableList();
                    }
                    reorderableList.DoList(chaptersRect);
                    //Rect warningRect = new Rect(position.x, position.y + 60f, position.width, 30f);
                }
                EditorGUI.EndProperty();
            }
            const float titleWidth = 60f;

            private void SourceAssignment (Rect dataRect)
            {
                LocalizedDataManager objRef = EditorGUI.ObjectField(dataRect, source, typeof(LocalizedDataManager), true) as LocalizedDataManager;
                if (objRef != source)
                {
                    source = objRef;
                    property.FindPropertyRelative("source").objectReferenceValue = objRef;
                }
                if (source != null)
                {
                    DictionaryRefAssignment();
                }
            }
            private void DictionaryRefAssignment()
            {
                dictionaryRef = source.GetLocalizationDictionary(property.FindPropertyRelative("previewData").type.Replace("PPtr<$", "").Replace(">", ""));
                idList = dictionaryRef.keyIDs;
            }
            private void InitReorderableList ()
            {
                source = property.FindPropertyRelative("source").objectReferenceValue as LocalizedDataManager;

                List<string> dummyData = new List<string>();
                dummyData.Add(property.FindPropertyRelative("idKey").stringValue);
                reorderableList = new ReorderableList(dummyData, typeof(string), false, true, false, false);
                
                reorderableList.drawHeaderCallback = (Rect rect) =>
                {
                    Rect titleRect = new Rect(rect.x, rect.y, titleWidth, EditorGUIUtility.singleLineHeight);
                    Rect dataRect = new Rect(titleRect.xMax, rect.y, rect.width - titleRect.width, EditorGUIUtility.singleLineHeight);
                    GUI.Label(titleRect, "Source");
                    SourceAssignment(dataRect);
                };
                reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (dictionaryRef != null)
                    {
                        Rect titleRect = new Rect(rect.x, rect.y, titleWidth, EditorGUIUtility.singleLineHeight);
                        Rect fieldRect = new Rect(titleRect.xMax, rect.y, rect.width - titleRect.width - dropBtnSize, EditorGUIUtility.singleLineHeight);
                        Rect buttonRect = new Rect(fieldRect.xMax, rect.y, dropBtnSize, EditorGUIUtility.singleLineHeight);

                        GUI.Label(titleRect, "KeyID");
                        string data = property.FindPropertyRelative("idKey").stringValue;
                        if (!idList.Contains(data))
                            GUI.backgroundColor = new Color(1f, 0.3f, 0.3f, 1f);
                        property.FindPropertyRelative("idKey").stringValue = EditorGUI.TextField(fieldRect, property.FindPropertyRelative("idKey").stringValue);
                        GUI.backgroundColor = Color.white;

                        string[] popupData = FilterList(data, idList);
                        GUI.enabled = (popupData.Length > 1);
                        int selectedIndex = EditorGUI.Popup(buttonRect, -1, popupData, EditorStyles.miniButtonRight);
                        GUI.Label(buttonRect, (popupData.Length > 1) ? " ▾" : " ");
                        if (selectedIndex >= 0)
                            property.FindPropertyRelative("idKey").stringValue = popupData[selectedIndex];
                        GUI.enabled = true;
                    }
                    else
                    {
                        GUIStyle gS = new GUIStyle();
                        gS.normal.textColor = Color.red;
                        gS.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(rect, "☠ Source cannot be null ☠", gS);
                    }
                };
                reorderableList.elementHeightCallback = (index) =>
                {
                    return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                };
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
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return 60f;
            }
        }
#endif
    }
    [System.Serializable]
    public abstract class LocalizedDataBase<T> : LocalizedDataBase
    {
        public T Value
        {
            get
            {
                return source.GetData<T>(idKey);
            }
        }

        //public LocalizedDataBase(string _idkey)
        //{
        //    idKey = _idkey;
        //}
#if UNITY_EDITOR
        [SerializeField]
        private T previewData;
#endif
    }
}