#if UNITY_EDITOR
using REditor;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using RTool;

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
        internal abstract void SaveTemporaryObject(string newKey, string targetKey = "");

        internal int dataJumper = -1;

        [CustomEditor(typeof(ScriptableDatabase),true)]
        private partial class CustomInspector : RInspector<ScriptableDatabase>
        {
            //private bool dataCanHaveParrent => handler.GetType().IsSubclassOf(typeof(LinkedScriptableDatabase<>);
            //private ScriptableDatabase linkedParent => handler.parentDatabase;
            private ReorderableDictionaryGUIHelper<CustomInspector, string> editHelper { get; set; }

            private string selectedKey { get; set; }

            protected override void OnEnable()
            {
                base.OnEnable();
                SetupDictionaryGUIHelper();
            }
            protected void OnDisable()
            {
                EditorUtility.SetDirty(handler);
                AssetDatabase.SaveAssets();
            }
            private void SetupDictionaryGUIHelper()
            {
                editHelper = new ReorderableDictionaryGUIHelper<CustomInspector, string>(this);
                editHelper.keyConstraint = (string key) => ValidKeySyntax(key);
                editHelper.setValue = (value, selectedID) => { handler.SetName(selectedID, value); };
                editHelper.drawValue = (rect, selectedID) => { return GUI.TextField(rect, handler.GetName(selectedID)); };
                editHelper.onKeySelected = (key) => { selectedKey = key; };
                //editHelper.onKeyRemoved = (key) => { selectedKey = key; };
            }

            protected override void DrawGUI()
            {
                DrawHeader();
                //DrawParentController();
                editHelper?.DrawGUI();
                DrawEditDetail();
                DrawingJsonController();

                serializedObject.ApplyModifiedPropertiesWithoutUndo();

                if (RTool.IsDebug)
                    DrawDefaultInspector();
            }
            private new void DrawHeader()
            {
                EditorGUILayout.LabelField(new GUIContent("Scriptable Database: " + handler.dataType.Name
                    , "A small but purfect tool for SQL-database-like. E-meow me fur more detail at: hoa.nguyenduc1206@gmail.com"), EditorStyles.centeredGreyMiniLabel);
            }
            private void DrawEditDetail()
            {
                bool hasData = !string.IsNullOrEmpty(selectedKey);
                //if (hasData && handler.linkedParent != null)
                //{
                //    DrawSelectParent();
                //}

                GUIContent content = new GUIContent(hasData ?
                    "Selected " + handler.dataType.Name + " (" + selectedKey + ")"
                    : "New " + handler.dataType.Name);

                SerializedProperty property = serializedObject.FindProperty("temporaryData");
                EditorGUILayout.PropertyField(property, content, true, GUILayout.Height(EditorGUI.GetPropertyHeight(property)));

                if (property.isExpanded)
                {
                    GUILayout.BeginHorizontal();
                    GUI.enabled = hasData;
                    if (GUILayout.Button("Save"))
                        Edit_Save(selectedKey);
                    GUI.enabled = (string.IsNullOrEmpty(selectedKey) || (selectedKey != handler.TemporaryModifiedKey));
                    if (GUILayout.Button("Add New"))
                        Edit_Save();
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                }
            }

            void Edit_Save(string targetKey = null)
            {
                string newKey = handler.TemporaryModifiedKey;
                if (CheckErrorAndLeave(out newKey))
                    return;

                bool isCreateNew = string.IsNullOrEmpty(targetKey);
                //if (string.IsNullOrEmpty(targetKey)) //new key
                //    filteredIDList.Add(newKey);
                //else
                //    filteredIDList[filteredIDList.IndexOf(selectedKey)] = newKey;

                handler.SaveTemporaryObject(newKey, targetKey);
                selectedKey = newKey;
                reorderableList.index = filteredIDList.IndexOf(selectedKey);
            }
            private bool CheckErrorAndLeave(out string newKey)
            {
                newKey = handler.TemporaryModifiedKey;
                if (ValidKeySyntax(newKey) == false)
                {
                    newKey = UniqueID("newKey", filteredIDList);
                    return (!EditorUtility.DisplayDialog(
                        "Invalid Key Format",
                        "Key can not be empty or contain special character. Save as \"" + newKey + "\"?",
                        "OK", "Cancel"));
                }

                if (ValidKeyUnique(selectedKey, newKey, idList) == false)
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
            //private class asdasf
            //{
            //    public void RefreshFilter(string _filter)
            //    {
            //        filter = _filter;
            //        filteredIDList.Clear();
            //        if (string.IsNullOrEmpty(filter))
            //            filteredIDList.AddRange(database.keyIDs);
            //        else
            //        {
            //            foreach (var itemID in database.keyIDs)
            //                if (itemID.Contains(filter))
            //                {
            //                    filteredIDList.Add(itemID);
            //                }
            //        }
            //        if (reorderableList != null)
            //            reorderableList.index = -1;
            //        Edit_Select();
            //    }

            //    internal void DrawGUI()
            //    {
            //        DrawSelectRegion();
            //        DrawEditRegion();
            //    }

            //    Vector2 scrollPos;
            //    internal void DrawSelectRegion()
            //    {
            //        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            //        DrawSearchFilter();
            //        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(ContentHeight));
            //        //Hack: if reset called --> dict = null;
            //        if (!database.IsDeserialized)
            //            InitReorderableList();
            //        reorderableList.DoLayoutList();
            //        EditorGUILayout.EndScrollView();
            //        EditorGUILayout.EndVertical();
            //    }
            //    string filterText = "";
            //    private void DrawSearchFilter()
            //    {
            //        EditorGUILayout.BeginHorizontal();
            //        Rect r = EditorGUILayout.GetControlRect();
            //        string tmpText = filterText;
            //        GUI.SetNextControlName("filterTextBox");
            //        tmpText = EditorGUI.TextField(r, tmpText);
            //        if (string.IsNullOrEmpty(tmpText) && ActiveControl != "filterTextBox")
            //        {
            //            Rect r2 = new Rect(r);
            //            r2.y += 2f;
            //            GUI.Label(r2, " Search here...", REditorUtils.grayFont);
            //        }
            //        if (filterText != tmpText)
            //        {
            //            filterText = tmpText;
            //            RefreshFilter(filterText);
            //        }
            //        EditorGUILayout.EndHorizontal();
            //    }
            //    internal void DrawEditRegion()
            //    {

            //    }
            //    private List<String> parentKey { get; set; }
            //    private List<String> parentName { get; set; }
            //    private void SetParentKeyIndex(string _parentKey )
            //    {

            //    }
            //    private void GetParentKey (string _parentKey)
            //    {

            //    }
            //    private void DrawSelectParent()
            //    {
            //        //EditorGUILayout.Popup(new GUIContent("Parent", "Parent data"))
            //    }

            //    void Edit_Select(string _idKey = "")
            //    {
            //        selectedKey = _idKey;
            //        database.CreateTemporaryObject(selectedKey);
            //        handler.serializedObject.Update();
            //    }
            //    
            //}
            partial void DrawingJsonController();
        }
    }

    //public abstract partial class ScriptableDatabase<T> : ScriptableDatabase where T : IdenticalDataBase, new()
    //{
    //    protected sealed override Type dataType => typeof(T);
    //    internal sealed override IEnumerable<string> keyIDs => dataDict?.Keys;

    //    internal sealed override string GetName(string _key) => dataDict[_key].Name;
    //    internal sealed override object GetObject(string _key) => dataDict[_key] as object;
    //    internal sealed override object GetTemporaryObject() => new T() as object;
    //    internal sealed override void SetName(string _key, string _value) => dataDict[_key].Name = _value;
    //    internal sealed override void AddNewKey(string _key)
    //    {
    //        T newData = new T();
    //        newData.key = _key;
    //        dataDict.Add(_key, newData);
    //    }
    //    internal override void RemoveData(string _key) => dataDict.Remove(_key);
    //    internal override void ChangeKey(string _oldID, string _newID)
    //    {
    //        if (_oldID == _newID)
    //            return;

    //        dataDict.Add(_newID, dataDict[_oldID]);
    //        dataDict[_newID].key = _newID;
    //        dataDict.Remove(_oldID);
    //    }
        
    //    [SerializeField]
    //    internal T temporaryData = null; //do not rename this variable
    //    internal sealed override void CreateTemporaryObject(string key = "")
    //    {
    //        temporaryData = new T();
    //        if (string.IsNullOrEmpty(key) == false)
    //            temporaryData = dataDict[key].DeepClone();
    //    }
    //    internal sealed override string TemporaryModifiedKey => temporaryData.Key;
    //    internal sealed override void SaveTemporaryObject(string newKey, string targetKey = "")
    //    {
    //        temporaryData.key = newKey;
    //        if (string.IsNullOrEmpty(targetKey) == false)
    //            dataDict.Remove(targetKey);
    //        dataDict.Add(newKey, temporaryData);

    //        CreateTemporaryObject(newKey);
    //    }
    //}
}
#endif