#if UNITY_EDITOR
using REditor;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RTool.Database
{
    public abstract partial class ScriptableDatabase : ScriptableObject
    {
        protected abstract Type dataType { get; }
        internal abstract IEnumerable<string> keyIDs { get; }

        internal abstract string GetName(string _key);
        internal abstract object GetObject(string _key);
        internal abstract object GetTemporaryObject();
        internal abstract void SetName(string _key, string _value);
        internal abstract void AddNewKey(string _key);
        internal abstract void RemoveData(string _key);
        internal abstract void ChangeKey(string _oldID, string _newID);

        internal abstract void CreateTemporaryObject(string key);
        internal abstract string TemporaryModifiedKey { get; }
        internal abstract void SaveTemporaryObject(string targetKey = "");

        internal int dataJumper = -1;

        [CustomEditor(typeof(ScriptableDatabase),true)]
        private class CustomInspector : UnityObjectEditor<ScriptableDatabase>
        {
            protected override void OnEnable()
            {
                base.OnEnable();
                editHelper = new EditHelper(this, handler.dataJumper);
            }
            protected void OnDisable()
            {
                handler.SerializeToList();
                EditorUtility.SetDirty(handler);
                AssetDatabase.SaveAssets();
            }

            public override void OnInspectorGUI()
            {
                UpdateFocusedControl();

                //Draw Header
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Scriptable Database: " + handler.dataType.Name
                    , "A small but purfect tool for SQL-database-like. E-meow me fur more detail at: hoa.nguyenduc1206@gmail.com"), EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndHorizontal();

                DrawParentController();
                DrawingDetailHelper();

                if (RTool.IsDebug)
                {
                    GUILayout.BeginVertical();
                    DrawDefaultInspector();
                    GUILayout.EndVertical();
                }
            }

            const int settingSize = 25;
            private bool dataCanHaveParrent => !handler.dataType.IsSubclassOf(typeof(IdenticalData));
            private void DrawParentController()
            {
                EditorGUILayout.BeginVertical();
                if (dataCanHaveParrent)
                {
                    handler.parentDatabase = EditorGUILayout.ObjectField(new GUIContent("ParentDatabase",
                    handler.dataType.Name + " need a parent database"),
                    handler.parentDatabase,
                    typeof(ScriptableDatabase), true) as ScriptableDatabase;
                    if (handler.parentDatabase == null)
                    {
                        EditorGUILayout.HelpBox(
                           "Should assign " + typeof(ScriptableDatabase).Name + " to show full information", MessageType.Warning, true);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            private void DrawingDetailHelper()
            {
                if (editHelper != null)
                {
                    editHelper.DrawingStuff();
                }
            }
            #region Draw ReorderableList
            private EditHelper editHelper;
            private class EditHelper
            {
                private CustomInspector handler;
                private ScriptableDatabase database => handler.handler;
                private IEnumerable<string> idList => database.keyIDs;

                private ReorderableList reorderableList;
                protected List<string> filteredIDList;
                protected string filter = "";
                private string reorderableList_editingKeyID = "";

                public EditHelper(CustomInspector _handler, int _jumpToData)
                {
                    handler = _handler;
                    InitReorderableList();
                }
                private void InitReorderableList()
                {
                    database.CheckDeserialize();
                    filteredIDList = new List<string>();
                    RefreshFilter(filterText);
                    reorderableList = new ReorderableList(filteredIDList, typeof(string), false, false, true, true);
                    reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        rect.y += 2;
                        string keyID = filteredIDList[index];
                        string keyControlName = "keyID_" + keyID;

                        Rect keyRect = new Rect(rect.x, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight);
                        Rect valueRect = new Rect(rect.x + rect.width * 0.4f + 5f, rect.y, rect.width * 0.6f - 5f, EditorGUIUtility.singleLineHeight);
                        if (FocusedControl == keyControlName)
                        {
                            reorderableList_editingKeyID = keyID;
                        }
                        if (isActive)
                        {
                            GUI.SetNextControlName(keyControlName);
                            if (ActiveControl == keyControlName)
                            {
                                bool valid = CheckValidKey(keyID, reorderableList_editingKeyID, idList);
                                if (!(reorderableList_editingKeyID == keyID || valid))
                                    GUI.backgroundColor = new Color(1, 0.3f, 0.3f, 0.75f);
                                reorderableList_editingKeyID = GUI.TextField(keyRect, reorderableList_editingKeyID);
                                GUI.backgroundColor = Color.white;
                            }
                            else
                                GUI.TextField(keyRect, keyID);
                            database.SetName(keyID, GUI.TextField(valueRect, database.GetName(keyID)));
                        }
                        else
                        {
                            EditorGUI.LabelField(keyRect, keyID);
                            GUI.enabled = false;
                            GUI.TextField(valueRect, database.GetName(keyID));
                            GUI.enabled = true;
                        }
                        if (UnfocusedControl == keyControlName)
                        {
                            bool save = CheckValidKey(keyID, reorderableList_editingKeyID, database.keyIDs);
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
                        string newName = UniqueID("newKey", idList);
                        database.AddNewKey(newName);
                        filteredIDList.Add(newName);
                    };
                    reorderableList.onRemoveCallback = (list) =>
                    {
                        string removedKey = filteredIDList[reorderableList.index];
                        filteredIDList.RemoveAt(reorderableList.index);
                        database.RemoveData(removedKey);
                    };
                }

                internal static string UniqueID(string _id, IEnumerable<string> _checkList)
                {
                    List<string> checkList = new List<string>(_checkList);
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
                internal static bool CheckValidKey(string _old, string _new, IEnumerable<string> _checkList)
                {
                    if (_old == _new)
                        return false;
                    if (string.IsNullOrEmpty(_new))
                        return false;
                    List<string> compare = new List<string>(_checkList);
                    compare.Remove(_old);
                    if (compare.Contains(_new))
                        return false;
                    return true;
                }

                private void ChangeKeyID(string _old, string _new)
                {
                    database.ChangeKey(_old, _new);
                    filteredIDList[filteredIDList.IndexOf(_old)] = _new;
                }

                public void RefreshFilter(string _filter)
                {
                    filter = _filter;
                    filteredIDList.Clear();
                    if (string.IsNullOrEmpty(filter))
                        filteredIDList.AddRange(database.keyIDs);
                    else
                    {
                        foreach (var itemID in database.keyIDs)
                            if (itemID.Contains(filter))
                            {
                                filteredIDList.Add(itemID);
                            }
                    }
                    if (reorderableList != null)
                        reorderableList.index = -1;
                    Edit_Select();
                }

                private string selectedKey = "";

                Vector2 scrollPos;
                internal void DrawingStuff()
                {
                    DrawSelectRegion();
                    DrawEditRegion();
                }
                internal void DrawSelectRegion()
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    DrawSearchFilter();
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200f));
                    //Hack: if reset called --> dict = null;
                    if (!database.IsDeserialized)
                        InitReorderableList();
                    reorderableList.DoLayoutList();
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
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
                internal void DrawEditRegion()
                {
                    //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    bool hasData = !string.IsNullOrEmpty(selectedKey);
                    GUIContent content = new GUIContent(hasData ?
                        "Selected " + database.dataType.Name + " (" +selectedKey + ")"
                        : "New " + database.dataType.Name);

                    SerializedProperty property = handler.serializedObject.FindProperty("temporaryData");
                    EditorGUILayout.PropertyField(property, content, true, GUILayout.Height(EditorGUI.GetPropertyHeight(property)));

                    if (property.isExpanded)
                    {
                        GUILayout.BeginHorizontal();
                        GUI.enabled = hasData;
                        if (GUILayout.Button("Save"))
                            Edit_Save(selectedKey);
                        GUI.enabled = true;
                        if (GUILayout.Button("Add New"))
                        {
                            Edit_Save();
                        }
                        GUILayout.EndHorizontal();
                    }
                }

                void Edit_Save(string targetKey = null)
                {
                    handler.serializedObject.Update();
                    string newKey = database.TemporaryModifiedKey;
                    if (CheckErrorAndLeave(ref newKey))
                        return;
                    //if (selectedKey != newKey)
                    //{
                    //    localizeDictionaryRef.ChangeID(selectedKey, newKey);
                    //    int index = filteredIDList.IndexOf(selectedID);
                    //    filteredIDList[index] = newKey;
                    //}
                    database.SaveTemporaryObject(targetKey);
                    selectedKey = newKey;
                }

                void Edit_Select(string _idKey = "")
                {
                    selectedKey = _idKey;
                    database.CreateTemporaryObject(selectedKey);
                    handler.serializedObject.Update();
                }
                private bool CheckErrorAndLeave(ref string newKey)
                { 
                    if (string.IsNullOrEmpty(newKey) || Regex.Match(newKey, @"^[/^\w+$/]+$").Success == false)
                    {
                        newKey = UniqueID("newKey", filteredIDList);
                        return (!EditorUtility.DisplayDialog(
                            "Invalid Key Format",
                            "Key can not be empty or contain special character. Save as \"" + newKey + "\"?",
                            "OK", "Cancel"));
                    }
                    List<string> listIDs = new List<string>(database.keyIDs);
                    if (!string.IsNullOrEmpty(selectedKey))
                        listIDs.Remove(selectedKey);

                    if (listIDs.Contains(newKey) == true)
                    {
                        string lastKey = newKey;
                        newKey = UniqueID(newKey, filteredIDList);
                        return (!EditorUtility.DisplayDialog(
                            "Key existed",
                            "Already contain member \"" + lastKey + "\". Save as \"" + newKey + "\"?",
                            "OK", "Cancel"));
                    }
                    return false;
                }
            }

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
                    }
                    if (FocusedControl == null)
                    {
                        FocusedControl = checkingControl;
                    }
                    ActiveControl = checkingControl;
                }
            }
            #endregion
        }
    }

    public abstract partial class ScriptableDatabase<T> : ScriptableDatabase where T : IdenticalDataBase, new()
    {
        protected sealed override Type dataType => typeof(T);
        internal sealed override IEnumerable<string> keyIDs => dataDict?.Keys;

        internal sealed override string GetName(string _key) => dataDict[_key].Name;
        internal sealed override object GetObject(string _key) => dataDict[_key] as object;
        internal sealed override object GetTemporaryObject() => new T() as object;
        internal sealed override void SetName(string _key, string _value) => dataDict[_key].Name = _value;
        internal sealed override void AddNewKey(string _key)
        {
            T newData = new T();
            newData.key = _key;
            dataDict.Add(_key, newData);
        }
        internal override void RemoveData(string _key) => dataDict.Remove(_key);
        internal override void ChangeKey(string _oldID, string _newID)
        {
            if (_oldID == _newID)
                return;

            dataDict.Add(_newID, dataDict[_oldID]);
            dataDict.Remove(_oldID);
        }
        
        [SerializeField]
        internal T temporaryData = null; //do not rename this variable
        internal sealed override void CreateTemporaryObject(string key = "")
        {
            if (string.IsNullOrEmpty(key) == false)
                temporaryData = dataDict[key];
            else
                temporaryData = new T();
        }
        internal sealed override string TemporaryModifiedKey => temporaryData.Key;
        internal sealed override void SaveTemporaryObject(string targetKey = "")
        {
            if (string.IsNullOrEmpty(targetKey) == false)
                dataDict.Remove(targetKey);
            dataDict.Add(temporaryData.key, temporaryData);
        }
    }
}
#endif