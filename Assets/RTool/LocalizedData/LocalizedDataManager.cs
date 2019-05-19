using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using REditor;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace RTool.Localization
{
    [CreateAssetMenu(menuName = "Localized Data Manager")]
    public class LocalizedDataManager : ScriptableObject
    {
        private int _defaultLanguageIndex = 0;
        public int DefaultLanguageIndex
        {
            get
            {
                return _defaultLanguageIndex;
            }
            private set
            {
                if (_defaultLanguageIndex == value)
                    return;

                _defaultLanguageIndex = value;
                DefaultLanguageChangeNoti();
            }
        }
        public string defaultLanguageID
        {
            get
            {
                return languageID[DefaultLanguageIndex];
            }
            set
            {
                if (languageID.Contains(value))
                {
                    DefaultLanguageIndex = languageID.IndexOf(value);
                    SaveDefaultLanguage();
                    return;
                }
                Debug.LogError("Couldnt found language id, change to " + languageID[0]);
                DefaultLanguageIndex = 0;
                SaveDefaultLanguage();
            }
        }

        [SerializeField]
        internal List<string> languageID;
        [SerializeField]
        internal StringDictionary stringLD = new StringDictionary();
        [SerializeField]
        internal TextureDictionary textureLD = new TextureDictionary();

        internal List<LocalizationDictionary> allDictionaries
        {
            get
            {
                List<LocalizationDictionary> result = new List<LocalizationDictionary>();
                result.Add(stringLD);
                result.Add(textureLD);
                return result;
            }
        }
#if UNITY_EDITOR
        private void DeserializeDictionaries()
        {
            foreach (var item in allDictionaries)
            {
                item.SaveAndDeserialize();
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif

        private void DefaultLanguageChangeNoti()
        {
            if (onChangeDefaultLanguage != null)
            {
                onChangeDefaultLanguage();
            }
        }
        private const string defaultLanguageCode = "DefaultLocalizedLanguage";
        private void SaveDefaultLanguage()
        {
            PlayerPrefs.SetString(defaultLanguageCode, defaultLanguageID);
            Debug.Log("Default Language saved: " + defaultLanguageID);
        }
        private void LoadDefaultLanguage()
        {
            if (!PlayerPrefs.HasKey(defaultLanguageCode))
            {
                GetCurentLanguageOfDevice();
                SaveDefaultLanguage();
                return;
            }
            defaultLanguageID = PlayerPrefs.GetString(defaultLanguageCode);
        }
        void GetCurentLanguageOfDevice()
        {
            SystemLanguage language = Application.systemLanguage;
            Debug.Log("XXXXXXX Default language of Device = " + language.ToString());
            switch(language)
            {
                case SystemLanguage.Japanese: defaultLanguageID = "Japanese"; break;
                default : defaultLanguageID = "English"; break;
            }
        }
        #region UnityCall

        private void Reset()
        {
            Debug.Log("LocalizedDataManager init");
            stringLD.Setup(this);
            textureLD.Setup(this);

            languageID = new List<string>();
            languageID.Add("English");
            languageID.Add("Japanese");

            stringLD.Reset(languageID, new List<string> { "stringA", "stringB", "stringC" });
            textureLD.Reset(languageID, new List<string> { "textureA", "textureB" });
        }

        private void OnEnable()
        {
            Debug.Log("LocalizedDataManager enabled...");
            LoadDefaultLanguage();
        }

        #endregion

        #region public call
        #region event
        public event Action onChangeDefaultLanguage;
        #endregion
        
        private LocalizationDictionary<T> GetLocalizationDictionary<T>()
        {
            foreach (var item in allDictionaries)
            {
                if (item.DataType == typeof(T))
                {
                    item.CheckToSerialize();
                    return item as LocalizationDictionary<T>;
                }
            }
            throw new Exception("Error typing");
        }
#if UNITY_EDITOR
        public LocalizationDictionary GetLocalizationDictionary(string _typeName)
        {
            foreach (var item in allDictionaries)
            {
                if (item.DataType.Name.ToLower() == _typeName.ToLower())
                    return item as LocalizationDictionary;
            }
            throw new Exception("Error typing");
        }

        public void AddLanguage(string _language)
        {
            languageID.Add(_language);
            int newIndex = languageID.Count - 1;
            foreach (var dict in allDictionaries)
            {
                dict.AddNewLanguage(newIndex);
            }
        }
        public void RemoveLanguage(int _languageIndex)
        {
            foreach (var dict in allDictionaries)
            {
                dict.RemoveLanguage(_languageIndex);
            }
            languageID.RemoveAt(_languageIndex);
        }
#endif

        #region DataExtract
        private int SafeLanguageIndex(int _languageIndex)
        {
            if (_languageIndex < 0 || _languageIndex >= languageID.Count)
                return DefaultLanguageIndex;
            return _languageIndex;
        }
        public T GetData<T>(string _idKey)
        {
            return GetData<T>(DefaultLanguageIndex, _idKey);
        }
        public T GetData<T>(int _languageID, string _idKey)
        {
            var localizedDict = GetLocalizationDictionary<T>();
            try
            {
                return localizedDict.dict[SafeLanguageIndex(_languageID)][_idKey];
            }
            catch
            {
                return localizedDict.DefaultData;
            }
        }
#if UNITY_EDITOR
        public List<string> GetIDList<T>()
        {
            var localizedDict = GetLocalizationDictionary<T>();
            return localizedDict.keyIDs;
        }
        public void SetData<T>(int _languageID, string _idKey, T _newValue)
        {
            var localizedDict = GetLocalizationDictionary<T>();
            localizedDict.dict[SafeLanguageIndex(_languageID)][_idKey] = _newValue;
        }
        public void SetData<T>(string _idKey, T _newValue)
        {
            var localizedDict = GetLocalizationDictionary<T>();
            localizedDict.dict[DefaultLanguageIndex][_idKey] = _newValue;
        }
#endif
        #endregion
        #endregion

        [System.Serializable]
        public abstract class UnityLocalizationDictionary<T> : LocalizationDictionary<T> where T : UnityEngine.Object
        {
            public sealed override bool HasAdvancedFilter => true;
            public sealed override bool Filter(T _object, string _filter)
            {
                if (_object == null)
                    return false;
                if (_object.name.Contains(_filter))
                {
                    return true;
                }
                return false;
            }
        }
        [System.Serializable]
        public abstract class LocalizationDictionary<T> : LocalizationDictionary
        {
            public List<T> dataSet;

            public Dictionary<int, Dictionary<string, T>> dict;

            internal bool isSerialized
            {
                get
                {
                    return (dict != null);
                }
            }
            public void Setup(LocalizedDataManager _handler)
            {
                handler = _handler;
            }
            internal void Reset(List<string> _keyLanguageDefault, List<string> _keyIDDefault)
            {
                keyIDs = _keyIDDefault;
                dataSet = new List<T>();
                foreach (var itemLanguage in _keyLanguageDefault)
                {
                    foreach (var item in keyIDs)
                    {
                        dataSet.Add(DefaultData);
                    }
                }
                CheckToSerialize();
            }
            public virtual T DefaultData
            {
                get
                {
                    return default;
                }
            }
            public virtual bool Filter(T _object, string _filter)
            {
                return true;
            }
            public virtual bool HasAdvancedFilter
            {
                get
                {
                    return false;
                }
            }
            #region public call
            internal sealed override Type DataType
            {
                get
                {
                    return typeof(T);
                }
            }
            internal sealed override void CheckToSerialize()
            {
                if (!isSerialized)
                    SerializeToDictionary();
            }
            internal sealed override void SerializeToDictionary()
            {
                Debug.Log("SerializeToDictionary " + this.GetType().Name + "...");
                dict = new Dictionary<int, Dictionary<string, T>>();
                int dataSetIndex = 0;
                for (int languageIndex = 0; languageIndex < handler.languageID.Count; languageIndex++)
                {
                    var curDict = new Dictionary<string, T>();
                    dict.Add(languageIndex, curDict);
                    for (int j = 0; j < keyIDs.Count; j++)
                    {
                        curDict.Add(keyIDs[j], dataSet[dataSetIndex]);
                        dataSetIndex++;
                    }
                }
            }
            internal T GetData(int _languageIndex, string _key)
            {
                try
                {
                    int safeIndex = handler.SafeLanguageIndex(_languageIndex);
                    return dict[safeIndex][_key];
                }
                catch
                {
                    return DefaultData;
                }
            }
#if UNITY_EDITOR
            internal sealed override void ShowDataPreview(Rect _rect, string _key)
            {
                REditorUtils.DoField(_rect, GetData(handler.DefaultLanguageIndex, _key));
            }
            internal sealed override void SaveAndDeserialize()
            {
                CheckToSerialize();
                Debug.Log("Deserialize and Save Data for " + this.GetType().Name + "...");

                dataSet = new List<T>();
                for (int languageIndex = 0; languageIndex < handler.languageID.Count; languageIndex++)
                {
                    for (int j = 0; j < keyIDs.Count; j++)
                    {
                        string keyID = this.keyIDs[j];
                        dataSet.Add(dict[languageIndex][keyID]);
                    }
                }
            }
            internal sealed override void AddNewLanguage(int _languageIndex)
            {
                Dictionary<string, T> newKeyDict = new Dictionary<string, T>();
                dict.Add(_languageIndex, newKeyDict);
                foreach (var keyID in keyIDs)
                {
                    newKeyDict.Add(keyID, DefaultData);
                }
            }
            internal sealed override void RemoveLanguage(int _languageIndex)
            {
                dict.Remove(_languageIndex);
                for (int i = _languageIndex; i < handler.languageID.Count - 1; i++)
                {
                    Dictionary<string, T> selectedDict = dict[i + 1];
                    dict.Remove(i + 1);
                    dict.Add(i, selectedDict);
                }
            }
            internal void AddNewID(string _key)
            {
                CheckToSerialize();
                keyIDs.Add(_key);
                foreach (var keyDict in dict)
                {
                    keyDict.Value.Add(_key, DefaultData);
                }
            }
            internal void RemoveOldID(string _key)
            {
                CheckToSerialize();

                if (!isSerialized)
                    return;

                foreach (var keyDict in dict)
                {
                    keyDict.Value.Remove(_key);
                }
                keyIDs.Remove(_key);
            }
            internal void ChangeID(string _oldID, string _newID)
            {
                if (_oldID == _newID)
                    return;

                //Debug.Log("Change " + _oldID + " to " + _newID);
                for (int langIndex = 0; langIndex < handler.languageID.Count; langIndex++)
                {
                    dict[langIndex].Add(_newID, dict[langIndex][_oldID]);
                    dict[langIndex].Remove(_oldID);
                }
                keyIDs.Add(_newID);
                keyIDs.Remove(_oldID);
            }
            internal void SetData(int _languageIndex, string _key, T value)
            {
                dict[handler.SafeLanguageIndex(_languageIndex)][_key] = value;
            }
#endif
            #endregion
        }

        [System.Serializable]
        public abstract class LocalizationDictionary
        {
            [SerializeField]
            protected LocalizedDataManager handler;
            public List<string> keyIDs;

#if UNITY_EDITOR
            internal abstract void ShowDataPreview(Rect _rect, string _keyID);
            internal abstract void SaveAndDeserialize();
            internal abstract void AddNewLanguage(int _languageIndex);
            internal abstract void RemoveLanguage(int _languageIndex);
#endif
            internal abstract Type DataType { get; }
            internal abstract void CheckToSerialize();
            internal abstract void SerializeToDictionary();
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(LocalizedDataManager))]
        public class LocalizationEditor : UnityObjectEditor<LocalizedDataManager>
        {
            [SerializeField]
            private int searchMethod = 2;
            private void ChangeSearchMethod()
            {
                searchMethod++;
                if (searchMethod > 2)
                    searchMethod = 0;
            }

            private static GUIStyle symbolStyle (int _fontSize)
            {
                GUIStyle result = new GUIStyle(EditorStyles.miniButtonRight);
                result.fontSize = _fontSize;
                result.onFocused.textColor = Color.red;
                return result;
            }
            protected override void OnEnable()
            {
                base.OnEnable();
                editHelper = new DataEditHelper<string>(this);
            }
            protected void OnDisable()
            {
                handler.DeserializeDictionaries();
            }

            public override void OnInspectorGUI()
            {
                UpdateFocusedControl();

                //Draw Header
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Localized Data", "A small but purfect tool for localization. E-meow me fur more detail at: hoa.nguyenduc1206@gmail.com"), EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndHorizontal();

                DrawMenuSelector();
                DrawingDetailHelper();
            }
            private int _currentDataTypeIndex = 0;
            private int currentDataTypeIndex
            {
                get
                {
                    return _currentDataTypeIndex;
                }
                set
                {
                    if (_currentDataTypeIndex == value)
                        return;
                    _currentDataTypeIndex = value;

                    editHelper = null;
                    switch (_currentDataTypeIndex)
                    {
                        case 0:
                            editHelper = new DataEditHelper<string>(this);
                            break;
                        case 1:
                            editHelper = new DataEditHelper<Texture>(this);
                            break;
                        default:
                            editHelper = new LanguageEditHelper(this);
                            break;
                    }
                    GUI.FocusControl(null);
                }
            }
            const int settingSize = 25;
            private void DrawMenuSelector()
            {
                EditorGUILayout.BeginHorizontal();
                Rect rect = EditorGUILayout.GetControlRect();
                Rect itemRect = new Rect(rect.x, rect.y, rect.width - settingSize, rect.height);
                Rect settingRect = new Rect(rect.x + rect.width - settingSize, rect.y, settingSize, rect.height);

                int count = handler.allDictionaries.Count;
                float cellSize = itemRect.width / count;
                for (int i = 0; i < count; i++)
                {
                    Rect r = new Rect(itemRect.x + (i * cellSize), itemRect.y, cellSize, itemRect.height);
                    bool isString = (currentDataTypeIndex == i);
                    isString = GUI.Toggle(r, isString, handler.allDictionaries[i].DataType.Name, (i == 0) ? EditorStyles.miniButtonLeft : EditorStyles.miniButtonMid);
                    if (isString)
                    {
                        currentDataTypeIndex = i;
                    }
                }

                bool isLanguage = (currentDataTypeIndex == -1);
                isLanguage = GUI.Toggle(settingRect, isLanguage, new GUIContent("☭", "Setting"), symbolStyle(12));

                if (isLanguage)
                {
                    currentDataTypeIndex = -1;
                }
                EditorGUILayout.EndHorizontal();
            }
            #region Draw ReorderableList
            private EditHelper editHelper;

            private abstract class EditHelper
            {
                protected const float keyRectScale = 0.4f;

                protected LocalizationEditor handler;

                internal ReorderableList reorderableList;
                internal abstract void DrawingStuff();

                protected string UniqueID(string _id, List<string> checkList)
                {
                    if (checkList.Contains(_id) == false)
                        return _id;

                    ushort count = 1;
                    while (true)
                    {
                        string result = _id + count.ToString();
                        if (checkList.Contains(result) == false)
                            return result;
                        count++;
                    }
                }

                protected bool CheckValidKey(string _old, string _new, List<string> _list)
                {
                    if (_old == _new)
                        return false;
                    if (string.IsNullOrEmpty(_new))
                        return false;
                    List<string> compare = new List<string>(_list);
                    compare.Remove(_old);
                    if (compare.Contains(_new))
                        return false;
                    return true;
                }
            }
            private class LanguageEditHelper : EditHelper
            {
                const float defaultW = 15f;
                const float buttonW = 20f;
                string editingData = "";
                internal LanguageEditHelper(LocalizationEditor _handler)
                {
                    handler = _handler;
                    foreach (var dict in handler.handler.allDictionaries)
                    {
                        dict.CheckToSerialize();
                    }
                    reorderableList = new ReorderableList(languageList, typeof(string), false, true, true, true);
                    reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        string languageControlName = "languageNameTextbox_" + index;
                        rect.y += 2;

                        float dataHeight = EditorGUIUtility.singleLineHeight;
                        Rect defaultRect = new Rect(rect.x, rect.y, defaultW, EditorGUIUtility.singleLineHeight);
                        Rect fullLanguageRect = new Rect(defaultRect.xMax, rect.y, rect.width - defaultW, EditorGUIUtility.singleLineHeight);

                        if (GUI.Toggle(defaultRect, (handler.handler.DefaultLanguageIndex == index), GUIContent.none))
                            handler.handler.DefaultLanguageIndex = index;

                        if (FocusedControl == languageControlName)
                        {
                            editingData = languageList[index];
                        }
                        if (isActive)
                        {
                            GUI.SetNextControlName(languageControlName);
                            if (ActiveControl == languageControlName)
                            {
                                bool valid = CheckValidKey(languageList[index], editingData, languageList);
                                if (!(editingData == languageList[index] || valid))
                                    GUI.backgroundColor = new Color(1, 0.3f, 0.3f, 0.75f);
                                editingData = GUI.TextField(fullLanguageRect, editingData);
                                GUI.backgroundColor = Color.white;
                            }
                            else
                                GUI.TextField(fullLanguageRect, languageList[index]);
                        }
                        else
                        {
                            GUI.Label(fullLanguageRect, languageList[index]);
                        }
                        if (UnfocusedControl == languageControlName)
                        {
                            bool save = CheckValidKey(languageList[index], editingData, languageList);
                            if (save)
                            {
                                reorderableList.GrabKeyboardFocus();
                                languageList[index] = editingData;
                            }
                            editingData = null;
                        }
                    };
                    reorderableList.headerHeight = 2;
                    reorderableList.onAddCallback = (list) =>
                    {
                        string newLanguage = UniqueID("newLanguage", languageList);
                        AddLanguage(newLanguage);
                        reorderableList.index = languageList.Count - 1;
                        reorderableList.GrabKeyboardFocus();
                    };
                    reorderableList.onCanRemoveCallback = (list) =>
                    {
                        return (languageList.Count > 2);
                    };
                    reorderableList.onRemoveCallback = (list) =>
                    {
                        if (EditorUtility.DisplayDialog("Warning!", "Remove language will affect your current data, are you sure?", "OK", "Cancel"))
                        {
                            RemoveLanguage(reorderableList.index);
                        }
                    };
                }
                private void AddLanguage(string _language)
                {
                    handler.handler.AddLanguage(_language);
                }

                private void RemoveLanguage(int _languageIndex)
                {
                    handler.handler.RemoveLanguage(_languageIndex);
                }

                List<String> languageList
                {
                    get
                    {
                        return handler.handler.languageID;
                    }
                }
                internal override void DrawingStuff()
                {
                    reorderableList.DoLayoutList();
                }
            }
            private abstract class DataEditHelper : EditHelper
            {
                protected List<string> filteredIDList;
                protected string filter = "";

                internal abstract void DrawItemValue(int _languageIndex);
            }
            private class DataEditHelper<T> : DataEditHelper
            {
                private string selectedID = "";
                private string editingID = "";
                private Dictionary<int, T> editingValue = new Dictionary<int, T>();
                const float refreshBtnW = 15f;
                const float refreshBtnH = 15f;
                const float searchMethodBtn = 20f;
                string filterText = "";

                private LocalizationDictionary<T> localizeDictionaryRef;
                private List<string> idList
                {
                    get
                    {
                        return localizeDictionaryRef.keyIDs;
                    }
                }

                private string reorderableList_editingKeyID = "";
                public DataEditHelper(LocalizationEditor _handler)
                {
                    selectedLanguageIndex = 0;
                    handler = _handler;
                    localizeDictionaryRef = handler.handler.GetLocalizationDictionary<T>();
                    localizeDictionaryRef.CheckToSerialize();
                    filteredIDList = new List<string>();
                    RefreshFilter(filterText);
                    reorderableList = new ReorderableList(filteredIDList, typeof(T), false, false, true, true);
                    reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        rect.y += 2;
                        string keyID = filteredIDList[index];
                        Dictionary<string, T> currentDictionary = localizeDictionaryRef.dict[selectedLanguageIndex];
                        string languageControlName = "keyID_" + keyID;

                        Rect keyRect = new Rect(rect.x, rect.y, rect.width * keyRectScale, EditorGUIUtility.singleLineHeight);
                        Rect valueRect = new Rect(rect.x + rect.width * keyRectScale + 5f, rect.y, rect.width * 0.6f - 5f, EditorGUIUtility.singleLineHeight);
                        if (FocusedControl == languageControlName)
                        {
                            reorderableList_editingKeyID = keyID;
                        }
                        if (isActive)
                        {
                            GUI.SetNextControlName(languageControlName);
                            if (ActiveControl == languageControlName)
                            {
                                bool valid = CheckValidKey(keyID, reorderableList_editingKeyID, idList);
                                if (!(reorderableList_editingKeyID == keyID || valid))
                                    GUI.backgroundColor = new Color(1, 0.3f, 0.3f, 0.75f);
                                reorderableList_editingKeyID = GUI.TextField(keyRect, reorderableList_editingKeyID);
                                GUI.backgroundColor = Color.white;
                            }
                            else
                                GUI.TextField(keyRect, keyID);
                            currentDictionary[keyID] = REditorUtils.DoField(valueRect, currentDictionary[keyID]);
                        }
                        else
                        {
                            EditorGUI.LabelField(keyRect, keyID);
                            GUI.enabled = false;
                            REditorUtils.DoField(valueRect, currentDictionary[keyID]);
                            GUI.enabled = true;
                        }
                        if (UnfocusedControl == languageControlName)
                        {
                            bool save = CheckValidKey(keyID, reorderableList_editingKeyID, localizeDictionaryRef.keyIDs);
                            if (save)
                            {
                                reorderableList.GrabKeyboardFocus();
                                ChangeKeyID(keyID, reorderableList_editingKeyID);
                            }
                            reorderableList_editingKeyID = null;
                        }
                    };
                    reorderableList.headerHeight = 2;
                    reorderableList.onSelectCallback = (list) =>
                    {
                        Edit_Select(filteredIDList[list.index]);
                    };
                    reorderableList.onAddCallback = (list) =>
                    {
                        string newName = UniqueID("newData", idList);
                        localizeDictionaryRef.AddNewID(newName);
                        filteredIDList.Add(newName);
                    };
                    reorderableList.onRemoveCallback = (list) =>
                    {
                        string removedKey = filteredIDList[reorderableList.index];
                        filteredIDList.RemoveAt(reorderableList.index);
                        localizeDictionaryRef.RemoveOldID(removedKey);
                    };
                    RefreshFilter();
                }
                private void ChangeKeyID(string _old, string _new)
                {
                    localizeDictionaryRef.ChangeID(_old, _new);
                    filteredIDList[filteredIDList.IndexOf(_old)] = _new;
                }

                internal void RefreshFilter()
                {
                    filteredIDList.Clear();
                    if (string.IsNullOrEmpty(filter))
                    {
                        filteredIDList.AddRange(localizeDictionaryRef.keyIDs);
                    }
                    else
                    {
                        if (handler.searchMethod == 0 || handler.searchMethod == 2)
                        {
                            foreach (var itemKey in localizeDictionaryRef.keyIDs)
                            {
                                if (!filteredIDList.Contains(itemKey) && itemKey.Contains(filter))
                                {
                                    filteredIDList.Add(itemKey);
                                }
                            }
                        }
                        if ((handler.searchMethod == 1 || handler.searchMethod == 2) && localizeDictionaryRef.HasAdvancedFilter)
                        {
                            foreach (var dataset in localizeDictionaryRef.dict[selectedLanguageIndex])
                            {
                                if (!filteredIDList.Contains(dataset.Key) && localizeDictionaryRef.Filter(dataset.Value, filter))
                                {
                                    filteredIDList.Add(dataset.Key);
                                }
                            }
                        }
                    }
                    if (reorderableList != null)
                        reorderableList.index = -1;
                    Edit_Select();
                }
                internal void RefreshFilter(string _filter)
                {
                    if (filter == _filter)
                        return;
                    filter = _filter;

                    RefreshFilter();
                }
                internal override void DrawItemValue(int _index)
                {
                    string label = handler.handler.languageID[_index];

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    Rect row = EditorGUILayout.GetControlRect();

                    EditorGUI.LabelField(row, label, EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.SetNextControlName("advancedEditBox_" + _index.ToString());
                    //var stuff = REditorUtils.DoFieldGUILayout(editingValue[_index], "Unsupported value", null, GUILayout.Height(100f));
                    //if (stuff.Equals(editingValue[_index]) == false)
                    //{
                    //    Debug.Log("record");
                    //    Undo.RecordObject(handler.handler, "ASDF");
                    //    editingValue[_index] = stuff;
                    //}
                    editingValue[_index] = REditorUtils.DoFieldGUILayout(editingValue[_index], "Unsupported value", null, GUILayout.Height(100f));
                    EditorGUILayout.EndVertical();
                }

                Vector2 scrollPos;
                internal override void DrawingStuff()
                {
                    DrawSelectRegion();
                    DrawEditRegion();
                }
                internal void DrawSelectRegion()
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    DrawLanguageSelector();
                    DrawSearchFilter();
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(250f));
                    reorderableList.DoLayoutList();
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                }

                private int selectedLanguageIndex { get; set; }
                private void DrawLanguageSelector()
                {
                    if (selectedLanguageIndex < 0)
                        selectedLanguageIndex = 0;

                    EditorGUILayout.BeginHorizontal();
                    Rect r = EditorGUILayout.GetControlRect();
                    Rect labelRect = new Rect(r.x, r.y, 85f, r.height);
                    Rect popupRect = new Rect(labelRect.xMax, r.y, r.width - labelRect.width, r.height);
                    EditorGUI.LabelField(labelRect, new GUIContent("Language", "Choose preview language"));
                    int newSelection = EditorGUI.Popup(popupRect, selectedLanguageIndex, handler.handler.languageID.ToArray());
                    if (newSelection != selectedLanguageIndex)
                    {
                        selectedLanguageIndex = newSelection;
                        RefreshFilter();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                private void DrawSearchFilter()
                {
                    EditorGUILayout.BeginHorizontal();
                    Rect rect = EditorGUILayout.GetControlRect();
                    Rect filterRect = rect;
                    if (localizeDictionaryRef.HasAdvancedFilter == true)
                    {
                        filterRect = new Rect(rect.x, rect.y, rect.width - searchMethodBtn, rect.height);
                        Rect searchMethodRect = new Rect(filterRect.xMax, rect.y, rect.width - filterRect.width, rect.height);
                        var searchMethod = handler.searchMethod;
                        var title = searchMethod == 0 ? "K" : (searchMethod == 1) ? "V" : "A";
                        if (GUI.Button(searchMethodRect, new GUIContent(title, "Search method: \n K: Key \n V: Value \n A: All"), symbolStyle(8)))
                        {
                            handler.ChangeSearchMethod();
                            RefreshFilter();
                        }
                    }
                    string tmpIDText = filterText;
                    GUI.SetNextControlName("filterTextBox");
                    tmpIDText = EditorGUI.TextField(filterRect, tmpIDText);
                    if (string.IsNullOrEmpty(tmpIDText) && ActiveControl != "filterTextBox")
                    {
                        Rect r2 = new Rect(filterRect);
                        r2.y += 2f;
                        GUI.Label(r2, " Search here...", REditorUtils.grayFont);
                    }
                    if (filterText != tmpIDText)
                    {
                        filterText = tmpIDText;
                        RefreshFilter(filterText);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                private bool showEditRegion = false;
                internal void DrawEditRegion()
                {
                    showEditRegion = GUILayout.Toggle(showEditRegion, new GUIContent("Advanced Editor",
                        "Show more detail on selected record and more function to control the data"),
                        EditorStyles.miniButton);
                    if (showEditRegion)
                    {
                        GUI.enabled = !string.IsNullOrEmpty(selectedID);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        editingID = EditorGUILayout.TextField(editingID);

                        for (int index = 0; index < handler.handler.languageID.Count; index++)
                            DrawItemValue(index);

                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = isDirty;
                        if (GUILayout.Button("Save"))
                            Edit_Save();
                        if (GUILayout.Button("Cancel"))
                            Edit_Cancel();
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                }
                private bool isDirty
                {
                    get
                    {
                        if (editingID != selectedID)
                            return true;
                        foreach (var item in editingValue)
                        {
                            if (CheckValueDirty(item.Key))
                                return true;
                        }
                        return false;
                    }
                }
                private bool CheckValueDirty(int _key)
                {
                    var _1 = localizeDictionaryRef.GetData(_key, selectedID);
                    var _2 = editingValue[_key];
                    if (ObjectIsNull(_1) && ObjectIsNull(_2))
                        return false;
                    if (_1 != null && !_1.Equals(_2))
                        return true;
                    return false;
                }
                private static bool ObjectIsNull(object _object)
                {
                    return (_object == null || ReferenceEquals(_object, null) || _object.Equals(null));
                }

                void Edit_Select(string _idKey = "")
                {
                    selectedID = _idKey;
                    Edit_Refresh();
                }
                void Edit_Save()
                {
                    string errorTitle = "";
                    string errorDecr = "";
                    string newName = "";
                    if (checkIDError(ref errorTitle, ref errorDecr, ref newName))
                    {
                        if (EditorUtility.DisplayDialog(errorTitle, errorDecr, "OK", "Cancel") == false)
                        {
                            return;
                        }
                    }
                    if (selectedID != newName)
                    {
                        localizeDictionaryRef.ChangeID(selectedID, newName);
                        int index = filteredIDList.IndexOf(selectedID);
                        filteredIDList[index] = newName;
                    }
                    for (int index = 0; index < handler.handler.languageID.Count; index++)
                    {
                        localizeDictionaryRef.SetData(index, newName, editingValue[index]);
                    }
                    selectedID = editingID;
                }
                bool checkIDError(ref string _title, ref string _description, ref string _newName, string dataName = "ID")
                {
                    if (string.IsNullOrEmpty(editingID))
                    {
                        _title = "Invalid " + dataName;
                        _newName = UniqueID("newData", filteredIDList);
                        _description = "New " + dataName + " can not be blank. Save as \"" + _newName + "\"?";
                        return true;
                    }
                    if (localizeDictionaryRef.keyIDs.Contains(editingID) == true)
                    {
                        if (editingID != selectedID)
                        {
                            _title = "Invalid " + dataName;
                            _newName = UniqueID(editingID, filteredIDList);
                            _description = "Already contain " + dataName + "  \"" + editingID + "\". Save as \"" + _newName + "\"?";
                            return true;
                        }
                    }
                    _newName = editingID;
                    return false;
                }

                void Edit_Cancel()
                {
                    reorderableList.index = -1;
                    selectedID = "";
                    Edit_Refresh();
                }
                void Edit_Refresh()
                {
                    editingID = selectedID;
                    editingValue.Clear();
                    for (int langIndex = 0; langIndex < handler.handler.languageID.Count; langIndex++)
                    {
                        editingValue.Add(langIndex, localizeDictionaryRef.GetData(langIndex, editingID));
                    }
                }
            }

            private void DrawingDetailHelper()
            {
                if (editHelper != null)
                {
                    editHelper.DrawingStuff();
                }
            }
            #region EditRegion


            #endregion

            internal static string ActiveControl { get; private set; }
            internal static string UnfocusedControl { get; private set; }
            internal static string FocusedControl { get; private set; }
            private static void UpdateFocusedControl()
            {
                FocusedControl = null;
                UnfocusedControl = null;
                string checkingControl = GUI.GetNameOfFocusedControl();
                if (checkingControl != ActiveControl)
                {
                    if (UnfocusedControl == null)
                    {
                        UnfocusedControl = ActiveControl;
                        Debug.Log("---Unfocus: " + UnfocusedControl);
                    }
                    if (FocusedControl == null)
                    {
                        FocusedControl = checkingControl;
                        Debug.Log("---Focus: " + FocusedControl);
                    }
                    Debug.Log("-------ActiveControl = " + checkingControl);
                    ActiveControl = checkingControl;
                }
            }
            #endregion
        }
#endif

    }

#if UNITY_EDITOR
    public class UnityObjectEditor<T> : Editor where T : UnityEngine.Object
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
#endif
}