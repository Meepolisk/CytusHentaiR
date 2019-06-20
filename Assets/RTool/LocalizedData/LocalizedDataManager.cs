using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using REditor;
using UnityEditor;
using UnityEditorInternal;
#endif

namespace RTool.Localization
{
    [CreateAssetMenu(menuName = "Localized Data Manager")]
    public class LocalizedDataManager : SemitonScriptableObject<LocalizedDataManager>
    {
        [System.Serializable]
        public class StringDictionary : LocalizationDictionary<string>
        {
            internal override string DefaultData => string.Empty;
        }
        [System.Serializable]
        public class TextureDictionary : LocalizationDictionary<Texture> { }

        private int _defaultLanguageIndex = 0;
        public int DefaultLanguageIndex
        {
            get => _defaultLanguageIndex;
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
            get => languageID[DefaultLanguageIndex];
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
            //todo: chỗ này có thể sửa thành config được trong setting (và lúc mới khởi tạo scripable object),
            //tạo 1 đống class sẵn có rồi add zô, serializedField
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
            if (this == null)
                return;

            foreach (var item in allDictionaries)
            {
                item.SaveAndSerializeToList();
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif

        private void DefaultLanguageChangeNoti()
        {
            onChangeDefaultLanguage?.Invoke();
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

        protected override void OnEnable()
        {
            base.OnEnable();
            Debug.Log("LocalizedDataManager enabled...", this);
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
                    item.CheckToDeserialize();
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
            //Debug.LogError(_typeName);
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
        //public bool CheckValidKey<T>(string _idKey)
        //{
        //    var localizedDict = GetLocalizationDictionary<T>();
        //    return localizedDict.checkValidKey(_idKey);
        //}
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
#endif
        //public List<string> GetIDList(string _typeName)
        //{
        //    var localizedDict = GetLocalizationDictionary(_typeName);
        //    return localizedDict.keyIDs;
        //}
#endregion
#endregion

        [System.Serializable]
        public abstract class LocalizationDictionary<T> : LocalizationDictionary
        {
            public List<T> dataSet;

            public Dictionary<int, Dictionary<string, T>> dict;

            internal bool isSerialized => (dict != null);
            internal Type dataType =>  typeof(T);
            public void Setup(LocalizedDataManager _handler)
            {
                handler = _handler;
            }

            internal virtual T DefaultData =>  default;
            public void Reset(List<string> _keyLanguageDefault, List<string> _keyIDDefault)
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
                CheckToDeserialize();
            }

            #region public call
            internal sealed override Type DataType => typeof(T);
            internal sealed override void CheckToDeserialize()
            {
                if (!isSerialized)
                    DeserializeToDictionary();
            }
            internal sealed override void DeserializeToDictionary()
            {
                Debug.Log("DeserializeToDictionary " + this.GetType().Name + "...");
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
#if UNITY_EDITOR
            internal sealed override void ShowDataPreview(Rect _rect, string _key)
            {
                REditorUtils.DoField(_rect, GetData(handler.DefaultLanguageIndex, _key));
            }
            internal sealed override void SaveAndSerializeToList()
            {
                CheckToDeserialize();
                Debug.Log("Serialize and Save Data for " + this.GetType().Name + "...");

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
                CheckToDeserialize();
                keyIDs.Add(_key);
                foreach (var keyDict in dict)
                {
                    keyDict.Value.Add(_key, DefaultData);
                }
            }
            internal void RemoveOldID(string _key)
            {
                CheckToDeserialize();

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

                Debug.Log("Change " + _oldID + " to " + _newID);
                for (int langIndex = 0; langIndex < handler.languageID.Count; langIndex++)
                {
                    //T temp = dict[langName][_oldID];
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
            internal abstract void SaveAndSerializeToList();
            internal abstract void AddNewLanguage(int _languageIndex);
            internal abstract void RemoveLanguage(int _languageIndex);
#endif
            internal abstract Type DataType { get; }
            internal abstract void CheckToDeserialize();
            internal abstract void DeserializeToDictionary();
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(LocalizedDataManager))]
        public class LocalizationEditor : RInspector<LocalizedDataManager>
        {
            protected override void OnEnable()
            {
                base.OnEnable();
                editHelper = new DataEditHelper<string>(this);
            }
            protected void OnDisable()
            {
                handler.DeserializeDictionaries();
            }

            protected override void DrawGUI()
            {
                DrawLabel();
                DrawMenuSelector();
                DrawingDetailHelper();
            }
            private void DrawLabel()
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Localized Data", "A small but purfect tool for localization. E-meow me fur more detail at: hoa.nguyenduc1206@gmail.com"), EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndHorizontal();
            }
            private int _currentDataTypeIndex = 0;
            private int currentDataTypeIndex
            {
                get => _currentDataTypeIndex;
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
                GUIStyle style = new GUIStyle(EditorStyles.miniButtonRight);
                style.fontSize = 13;
                style.onFocused.textColor = Color.red;
                isLanguage = GUI.Toggle(settingRect, isLanguage, new GUIContent("☭", "Setting"), style);

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
                protected LocalizationEditor editor;

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
                    editor = _handler;
                    foreach (var dict in editor.handler.allDictionaries)
                    {
                        dict.CheckToDeserialize();
                    }
                    reorderableList = new ReorderableList(languageList, typeof(string), false, true, true, true);
                    reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        string languageControlName = "languageNameTextbox_" + index;
                        rect.y += 2;

                        float dataHeight = EditorGUIUtility.singleLineHeight;
                        Rect defaultRect = new Rect(rect.x, rect.y, defaultW, EditorGUIUtility.singleLineHeight);
                        Rect fullLanguageRect = new Rect(defaultRect.xMax, rect.y, rect.width - defaultW, EditorGUIUtility.singleLineHeight);

                        if (GUI.Toggle(defaultRect, (editor.handler.DefaultLanguageIndex == index), GUIContent.none))
                            editor.handler.DefaultLanguageIndex = index;

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
                private void AddLanguage(string _language) => editor.handler.AddLanguage(_language);
                private void RemoveLanguage(int _languageIndex) => editor.handler.RemoveLanguage(_languageIndex);
                List<String> languageList => editor.handler.languageID;

                internal override void DrawingStuff()
                {
                    reorderableList.DoLayoutList();
                }
            }
            private abstract class DataEditHelper : EditHelper
            {
                protected List<string> filteredIDList;
                protected string filter = "";

                public abstract void RefreshFilter(string _filter);
                public abstract void DrawItemValue(int _languageIndex);
            }
            private class DataEditHelper<T> : DataEditHelper
            {
                private LocalizationDictionary<T> localizeDictionaryRef;
                private List<string> idList => localizeDictionaryRef.keyIDs;

                private string reorderableList_editingKeyID = "";
                public DataEditHelper(LocalizationEditor _handler)
                {
                    selectedLanguageIndex = 0;
                    editor = _handler;
                    localizeDictionaryRef = editor.handler.GetLocalizationDictionary<T>();
                    localizeDictionaryRef.CheckToDeserialize();
                    filteredIDList = new List<string>();
                    RefreshFilter(filterText);
                    reorderableList = new ReorderableList(filteredIDList, typeof(T), false, false, true, true);
                    //reorderableList.elementHeightCallback = (index) =>
                    //{
                    //    if (reorderableList.index != index)
                    //        return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2f;
                    //    return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing * 3f;
                    //};
                    reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        rect.y += 2;
                        string keyID = filteredIDList[index];
                        Dictionary<string, T> currentDictionary = localizeDictionaryRef.dict[selectedLanguageIndex];
                        string languageControlName = "keyID_" + keyID;

                        Rect keyRect = new Rect(rect.x, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight);
                        Rect valueRect = new Rect(rect.x + rect.width * 0.4f + 5f, rect.y, rect.width * 0.6f - 5f, EditorGUIUtility.singleLineHeight);
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
                }
                private void ChangeKeyID(string _old, string _new)
                {
                    localizeDictionaryRef.ChangeID(_old, _new);
                    filteredIDList[filteredIDList.IndexOf(_old)] = _new;
                }

                public override void RefreshFilter(string _filter)
                {
                    filter = _filter;
                    filteredIDList.Clear();
                    if (string.IsNullOrEmpty(filter))
                    {
                        filteredIDList.AddRange(localizeDictionaryRef.keyIDs);
                    }
                    else
                    {
                        foreach (var itemID in localizeDictionaryRef.keyIDs)
                        {
                            if (itemID.Contains(filter))
                            {
                                filteredIDList.Add(itemID);
                            }
                        }
                    }
                    if (reorderableList != null)
                        reorderableList.index = -1;
                    Edit_Select();
                }
                private string selectedID = "";
                private string editingID = "";
                private Dictionary<int, T> editingValue = new Dictionary<int, T>();
                const float refreshBtnW = 15f;
                const float refreshBtnH = 15f;
                public override void DrawItemValue(int _index)
                {
                    bool isDirty = CheckValueDirty(_index);
                    string label = editor.handler.languageID[_index];

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    Rect row = EditorGUILayout.GetControlRect();

                    if (isDirty)
                        label += "*";
                    EditorGUI.LabelField(row, label, EditorStyles.centeredGreyMiniLabel);
                    if (isDirty)
                    {
                        Rect button = new Rect(row.xMax - refreshBtnW, row.y + (row.height - refreshBtnH), refreshBtnW, refreshBtnH);
                        GUIStyle style = new GUIStyle();
                        style.normal.textColor = Color.black;
                        style.hover.textColor = Color.gray;
                        style.active.textColor = Color.blue;
                        style.fontSize = 15;
                        style.fontStyle = FontStyle.Bold;
                        style.onFocused.textColor = Color.red;
                        if (GUI.Button(button, new GUIContent("↶", "Revert data to original"), style))
                        {
                            editingValue[_index] = localizeDictionaryRef.GetData(_index, selectedID);
                            GUI.FocusControl(null);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    editingValue[_index] = REditorUtils.DoFieldGUILayout(editingValue[_index], _options: GUILayout.Height(100f));
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
                    selectedLanguageIndex = EditorGUI.Popup(popupRect, selectedLanguageIndex, editor.handler.languageID.ToArray());
                    EditorGUILayout.EndHorizontal();
                }
                string filterText = "";
                private void DrawSearchFilter()
                {
                    EditorGUILayout.BeginHorizontal();
                    Rect r = EditorGUILayout.GetControlRect();
                    string tmpText = filterText;
                    GUI.SetNextControlName("filterTextBox");
                    tmpText = EditorGUI.TextField(r, tmpText);
                    if (string.IsNullOrEmpty(tmpText) && ActiveControl != "filterTextBox")
                    {
                        Rect r2 = new Rect(r);
                        r2.y += 2f;
                        GUI.Label(r2, " Search here...", REditorUtils.grayFont);
                    }
                    if (filterText != tmpText)
                    {
                        filterText = tmpText;
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

                        for (int index = 0; index < editor.handler.languageID.Count; index++)
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
                    for (int index = 0; index < editor.handler.languageID.Count; index++)
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
                    for (int langIndex = 0; langIndex < editor.handler.languageID.Count; langIndex++)
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
            #endregion
        }
#endif
    }
    
}