#if UNITY_EDITOR
using REditor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using REditor;

namespace RTool.Database
{
    public abstract partial class ScriptableDatabase : ScriptableObject
    {
        protected abstract Type dataType { get; }
        internal abstract IEnumerable<string> keyIDs { get; }

        internal abstract string GetName(string _key);
        internal abstract object GetObject(string _key);
        internal abstract void SetName(string _key, string _value);
        internal abstract void AddNewID(string _key);
        internal abstract void RemoveData(string _key);
        internal abstract void ChangeKey(string _oldID, string _newID);

        [CustomEditor(typeof(ScriptableDatabase),true)]
        private class CustomInspector : UnityObjectEditor<ScriptableDatabase>
        {
            protected override void OnEnable()
            {
                base.OnEnable();
                editHelper = new EditHelper(this);
            }
            protected void OnDisable()
            {
                handler.SerializeToList();
            }

            public override void OnInspectorGUI()
            {
                //UpdateFocusedControl();

                //Draw Header
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Scriptable Database", "A small but purfect tool for SQL-database-like. E-meow me fur more detail at: hoa.nguyenduc1206@gmail.com"), EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndHorizontal();

                DrawParentController();
                DrawingDetailHelper();
                
                DrawDefaultInspector();
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
                           "Should assign " + typeof(ScriptableDatabase).Name + ". Otherwise all data of parent class cant modify", MessageType.Warning, true);
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
                //private IEnumerable<string> idList;

                private ReorderableList reorderableList;
                protected List<string> filteredIDList;
                protected string filter = "";
                private string reorderableList_editingKeyID = "";

                public EditHelper(CustomInspector _handler)
                {
                    handler = _handler;
                    database.CheckDeserialize();
                    //idList = database.keyIDs;

                    selectedLanguageIndex = 0;
                    filteredIDList = new List<string>();
                    RefreshFilter(filterText);
                    reorderableList = new ReorderableList(filteredIDList, typeof(string), false, false, true, true);
                    reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        rect.y += 2;
                        string keyID = filteredIDList[index];
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
                            database.SetName(keyID, GUI.TextField(valueRect, database.GetName(keyID)));
                        }
                        else
                        {
                            EditorGUI.LabelField(keyRect, keyID);
                            GUI.enabled = false;
                            GUI.TextField(valueRect, database.GetName(keyID));
                            GUI.enabled = true;
                        }
                        if (UnfocusedControl == languageControlName)
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
                        database.AddNewID(newName);
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
                    {
                        filteredIDList.AddRange(database.keyIDs);
                    }
                    else
                    {
                        foreach (var itemID in database.keyIDs)
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
                private Dictionary<string, string> editingValue = new Dictionary<string, string>();
                const float refreshBtnW = 15f;
                const float refreshBtnH = 15f;
                internal void DrawItemValue(int _index)
                {
                    //bool isDirty = CheckValueDirty(_index);
                    //string label = database.languageID[_index];

                    //EditorGUILayout.BeginVertical();
                    //EditorGUILayout.BeginHorizontal();
                    //Rect row = EditorGUILayout.GetControlRect();

                    //if (isDirty)
                    //    label += "*";
                    //EditorGUI.LabelField(row, label, EditorStyles.centeredGreyMiniLabel);
                    //if (isDirty)
                    //{
                    //    Rect button = new Rect(row.xMax - refreshBtnW, row.y + (row.height - refreshBtnH), refreshBtnW, refreshBtnH);
                    //    GUIStyle style = new GUIStyle();
                    //    style.normal.textColor = Color.black;
                    //    style.hover.textColor = Color.gray;
                    //    style.active.textColor = Color.blue;
                    //    style.fontSize = 15;
                    //    style.fontStyle = FontStyle.Bold;
                    //    style.onFocused.textColor = Color.red;
                    //    if (GUI.Button(button, new GUIContent("↶", "Revert data to original"), style))
                    //    {
                    //        editingValue[_index] = localizeDictionaryRef.GetData(_index, selectedID);
                    //        GUI.FocusControl(null);
                    //    }
                    //}
                    //EditorGUILayout.EndHorizontal();
                    //editingValue[_index] = REditorUtils.DoFieldGUILayout(editingValue[_index], "Unsupported value", null, GUILayout.Height(100f));
                    //EditorGUILayout.EndVertical();
                }

                Vector2 scrollPos;
                internal void DrawingStuff()
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
                    //if (selectedLanguageIndex < 0)
                    //    selectedLanguageIndex = 0;

                    //EditorGUILayout.BeginHorizontal();
                    //Rect r = EditorGUILayout.GetControlRect();
                    //Rect labelRect = new Rect(r.x, r.y, 85f, r.height);
                    //Rect popupRect = new Rect(labelRect.xMax, r.y, r.width - labelRect.width, r.height);
                    //EditorGUI.LabelField(labelRect, new GUIContent("Language", "Choose preview language"));
                    //selectedLanguageIndex = EditorGUI.Popup(popupRect, selectedLanguageIndex, handler.handler.languageID.ToArray());
                    //EditorGUILayout.EndHorizontal();
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
                        "Show more detail on selected record"), EditorStyles.miniButton);
                    if (showEditRegion)
                    {
                        ClassObjectDrawer.Show(database.GetObject(selectedID));
                    }
                }

                void Edit_Select(string _idKey = "")
                {
                    selectedID = _idKey;
                    Edit_Refresh();
                }
                void Edit_Save()
                {
                    //string errorTitle = "";
                    //string errorDecr = "";
                    //string newName = "";
                    //if (checkIDError(ref errorTitle, ref errorDecr, ref newName))
                    //{
                    //    if (EditorUtility.DisplayDialog(errorTitle, errorDecr, "OK", "Cancel") == false)
                    //    {
                    //        return;
                    //    }
                    //}
                    //if (selectedID != newName)
                    //{
                    //    localizeDictionaryRef.ChangeID(selectedID, newName);
                    //    int index = filteredIDList.IndexOf(selectedID);
                    //    filteredIDList[index] = newName;
                    //}
                    //for (int index = 0; index < handler.handler.languageID.Count; index++)
                    //{
                    //    localizeDictionaryRef.SetData(index, newName, editingValue[index]);
                    //}
                    selectedID = editingID;
                }
                bool checkIDError(ref string _title, ref string _description, ref string _newName, string dataName = "ID")
                {
                    //if (string.IsNullOrEmpty(editingID))
                    //{
                    //    _title = "Invalid " + dataName;
                    //    _newName = UniqueID("newData", filteredIDList);
                    //    _description = "New " + dataName + " can not be blank. Save as \"" + _newName + "\"?";
                    //    return true;
                    //}
                    //if (localizeDictionaryRef.keyIDs.Contains(editingID) == true)
                    //{
                    //    if (editingID != selectedID)
                    //    {
                    //        _title = "Invalid " + dataName;
                    //        _newName = UniqueID(editingID, filteredIDList);
                    //        _description = "Already contain " + dataName + "  \"" + editingID + "\". Save as \"" + _newName + "\"?";
                    //        return true;
                    //    }
                    //}
                    //_newName = editingID;
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
                    //for (int langIndex = 0; langIndex < handler.handler.languageID.Count; langIndex++)
                    //{
                    //    editingValue.Add(langIndex, localizeDictionaryRef.GetData(langIndex, editingID));
                    //}
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
        internal sealed override IEnumerable<string> keyIDs => dataDict.Keys;

        internal override string GetName(string _key) => dataDict[_key].Name;
        internal override object GetObject(string _key) => dataDict[_key] as object;
        internal override void SetName(string _key, string _value) => dataDict[_key].Name = _value;
        internal override void AddNewID(string _key) => dataDict.Add(_key, new T());
        internal override void RemoveData(string _key) => dataDict.Remove(_key);
        internal override void ChangeKey(string _oldID, string _newID)
        {
            if (_oldID == _newID)
                return;

            dataDict.Add(_newID, dataDict[_oldID]);
            dataDict.Remove(_oldID);
        }
    }
}
#endif