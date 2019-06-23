#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace REditor.GUIHelper
{
    public class ReorderableDictionaryGUIHelper<Teditor, TValue1> where Teditor : RInspector
    {
        public float ContentHeight = 200f;
        protected float keyRectScale = 0.4f;
        protected float valueRectScale = 0.6f;

        public delegate void KeyDelegate(string selectedID);
        public KeyDelegate onKeySelected;
        public KeyDelegate onKeyRemoved;
        public KeyDelegate onKeyAdded;

        public delegate void KeyKeyDelegate(string oldID, string newID);
        public KeyKeyDelegate onKeyChanged;

        public Func<IEnumerable<string>> getListID;
        public Func<string, bool> keyConstraint = ((id) => true);
        public delegate TValue1 GetValueRectDelegate(Rect rect, string selectedID);
        public delegate void SetValueRectDelegate(TValue1 value, string selectedID);
        public GetValueRectDelegate drawValue;
        public SetValueRectDelegate setValue;

        private Teditor editor { get; set; }
        private ReorderableList reorderableList { get; set; }
        private IEnumerable<string> idList { get; set; }
        protected List<string> filteredIDList { get; set; }
        private string selectedID { get; set; }
        private string editingID { get; set; }
        protected string filterText { get; set; }

        public ReorderableDictionaryGUIHelper(Teditor _handler)
        {
            editor = _handler;
            InitReorderableList();
        }
        private void InitReorderableList()
        {
            filteredIDList = new List<string>();
            RefreshFilter(filterText);
            reorderableList = new ReorderableList(filteredIDList, typeof(string), false, false, true, true);
            reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.y += 2;
                string indexKeyID = filteredIDList[index];
                string keyControlName = "keyID_" + indexKeyID;

                //key
                Rect keyRect = new Rect(rect.x, rect.y, rect.width * keyRectScale, EditorGUIUtility.singleLineHeight);
                if (editor.FocusedControl == keyControlName)
                    editingID = indexKeyID;
                if (isActive)
                {
                    GUI.SetNextControlName(keyControlName);
                    if (editor.ActiveControl == keyControlName)
                    {
                        bool valid = CheckValidKey(indexKeyID, editingID, idList);
                        if (valid == false && editingID != indexKeyID)
                            GUI.backgroundColor = new Color(1, 0.3f, 0.3f, 0.75f);
                        editingID = GUI.TextField(keyRect, editingID);
                        GUI.backgroundColor = Color.white;
                    }
                    else
                        GUI.TextField(keyRect, indexKeyID);
                }
                else
                {
                    EditorGUI.LabelField(keyRect, indexKeyID);
                }
                //value
                DrawSetValueFields(isActive, indexKeyID, rect);

                if (editor.UnfocusedControl == keyControlName)
                {
                    bool save = CheckValidKey(indexKeyID, editingID, idList);
                    if (save)
                    {
                        reorderableList.GrabKeyboardFocus();
                        ChangeKeyID(indexKeyID, editingID);
                    }
                    editingID = null;
                }
            };
            reorderableList.headerHeight = 2;
            reorderableList.onSelectCallback = (list) =>
            {
                Edit_Select(filteredIDList[list.index]);
            };
            reorderableList.onAddCallback = (list) =>
            {
                string newKey = UniqueID("newKey", idList);
                onKeyAdded(newKey);
                filteredIDList.Add(newKey);
                Edit_Select(newKey);
            };
            reorderableList.onRemoveCallback = (list) =>
            {
                string removedKey = filteredIDList[reorderableList.index];
                filteredIDList.RemoveAt(reorderableList.index);
                onKeyRemoved(removedKey);
                Edit_Select();
            };
        }
        protected virtual void DrawSetValueFields(bool isAtive, string indexKeyID, Rect rect)
        {
            Rect valueRect = new Rect(rect.x + rect.width * keyRectScale + 5f, rect.y, rect.width * valueRectScale - 5f, EditorGUIUtility.singleLineHeight);
            if (isAtive)
                setValue(drawValue(valueRect, indexKeyID), indexKeyID);
            else
            {
                GUI.enabled = false;
                drawValue(valueRect, indexKeyID);
                GUI.enabled = true;
            }
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
        internal bool CheckValidKey(string _old, string _new, IEnumerable<string> _checkList)
        {
            return keyConstraint(_new) && ValidKeyUnique(_old, _new, _checkList);
        }
        private static bool ValidKeySyntax(string key) => !string.IsNullOrEmpty(key) && new Regex(@"^[a-zA-Z0-9_]+$").IsMatch(key);
        private static bool ValidKeyUnique(string selectedKey, string newKey, IEnumerable<string> keyList)
        {
            List<string> listIDs = new List<string>(keyList);
            if (!string.IsNullOrEmpty(selectedKey))
                listIDs.Remove(selectedKey);

            return listIDs.Contains(newKey) == false;
        }

        private void ChangeKeyID(string _old, string _new)
        {
            onKeyChanged(_old, _new);
            filteredIDList[filteredIDList.IndexOf(_old)] = _new;
        }

        public void RefreshFilter(string _filter)
        {
            filterText = _filter;
            filteredIDList.Clear();
            if (string.IsNullOrEmpty(filterText))
                filteredIDList.AddRange(idList);
            else
            {
                foreach (var itemID in idList)
                    if (itemID.Contains(filterText))
                        filteredIDList.Add(itemID);
            }
            if (reorderableList != null)
                reorderableList.index = -1;
            Edit_Select();
        }

        Vector2 scrollPos;
        public void DrawGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawSearchFilter();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(ContentHeight));
            //Hack: if reset called --> dict = null;
            //InitReorderableList();
            reorderableList.DoLayoutList();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        private void DrawSearchFilter()
        {
            EditorGUILayout.BeginHorizontal();
            Rect r = EditorGUILayout.GetControlRect();
            string tmpText = filterText;
            GUI.SetNextControlName("filterTextBox");
            tmpText = EditorGUI.TextField(r, tmpText);
            if (string.IsNullOrEmpty(tmpText) && editor.ActiveControl != "filterTextBox")
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
        void Edit_Select(string _idKey = "")
        {
            selectedID = _idKey;
        }
    }
}
#endif