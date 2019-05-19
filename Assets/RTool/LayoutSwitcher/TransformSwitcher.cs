using System.Collections.Generic;
using UnityEngine;
using RTool.LayoutSwitcher;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using System;
#endif

public sealed class TransformSwitcher : LayoutSwitcherBase
{
    [System.Serializable]
    private class TransformData
    {
        public int CloneID = -1;
        public bool IsCloned(List<TransformData> list)
        {
            return (!(CloneID < 0 || CloneID > list.Count - 1));
        }

        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 SizeDelta;
        public Vector2 Pivot;
        public Vector2 OffsetMin;
        public Vector2 OffsetMax;

        public void RecordAll(Transform transform)
        {
            RectTransform rectTrans = transform as RectTransform;
            if (rectTrans != null)
            {
                AnchorMin = rectTrans.anchorMin;
                AnchorMax = rectTrans.anchorMax;
                SizeDelta = rectTrans.sizeDelta;
                Pivot = rectTrans.pivot;
                OffsetMin = rectTrans.offsetMin;
                OffsetMax = rectTrans.offsetMax;

                //Position = rectTrans.offsetMin;
                Rotation = rectTrans.localEulerAngles;
                Scale = rectTrans.localScale;
            }
            else
            {
                Position = transform.localPosition;
                Rotation = transform.localEulerAngles;
                Scale = transform.localScale;
            }
        }
        public void RecordAttribute(Transform transform, int index)
        {
            switch (index)
            {
                case 0:
                    try
                    {
                        RectTransform rectTrans = transform as RectTransform;
                        Position = rectTrans.offsetMin;
                        return;
                    }
                    catch
                    {
                        Position = transform.localPosition;
                        return;
                    }
                case 1:
                    Rotation = transform.localEulerAngles;
                    return;
                case 2:
                    Scale = transform.localScale;
                    return;
                case 3:
                    {
                        RectTransform rectTrans = transform as RectTransform;
                        AnchorMin = rectTrans.anchorMin;
                        AnchorMax = rectTrans.anchorMax;
                        return;
                    }
                case 4:
                    {
                        RectTransform rectTrans = transform as RectTransform;
                        Pivot = rectTrans.pivot;
                        return;
                    }
                case 5:
                    {
                        RectTransform rectTrans = transform as RectTransform;
                        SizeDelta = rectTrans.sizeDelta;
                        return;
                    }
                case 6:
                    {
                        RectTransform rectTrans = transform as RectTransform;
                        OffsetMin = rectTrans.offsetMin;
                        OffsetMax = rectTrans.offsetMax;
                        return;
                    }
            }
        }
        public void Set(Transform transform, List<TransformData> transformList, TransformAttribute _attribute)
        {
            TransformData data = this;
            if (IsCloned(transformList))
                data = transformList[CloneID];

            RectTransform rectTrans = transform as RectTransform;
            if (rectTrans != null)
            {
                if (_attribute.anchors)
                {
                    rectTrans.anchorMin = data.AnchorMin;
                    rectTrans.anchorMax = data.AnchorMax;
                }
                if (_attribute.sizeDelta)
                    rectTrans.sizeDelta = data.SizeDelta;
                if (_attribute.pivot)
                    rectTrans.pivot = data.Pivot;

                if (_attribute.position)
                {
                    rectTrans.offsetMin = data.OffsetMin;
                    rectTrans.offsetMax = data.OffsetMax;
                }
            }
            else
            {
                if (_attribute.position)
                    transform.localPosition = data.Position;
            }
            if (_attribute.rotation)
                transform.localEulerAngles = data.Rotation;
            if (_attribute.scale)
                transform.localScale = data.Scale;
        }

#if UNITY_EDITOR
        //private readonly Color dirtyColor = new Color(1f, 0.3f, 0.3f, 1f);
        public void Draw(Rect rect, Transform _transform, bool _isEnable, bool _isActive, bool _isRecord, string _name, List<TransformData> _transformList)
        {
            bool enabled = !_isRecord;
            GUI.enabled = enabled;

            float slh = EditorGUIUtility.singleLineHeight, svs = EditorGUIUtility.standardVerticalSpacing;
            float btnSize = slh + 3f;
            Rect topRect = new Rect(rect.x, rect.y, rect.width, slh);
            Rect nameRect = new Rect(rect.x, rect.y, rect.width / 2f, slh);
            Rect cloneRect = new Rect(nameRect.xMax, rect.y, rect.width - nameRect.width - btnSize, slh);

            GUIStyle gS = new GUIStyle();
            if (_isEnable)
                gS.normal.textColor = new Color(1, 0.3f, 0.3f, 1f);
            gS.alignment = TextAnchor.MiddleCenter;
            GUI.Label(nameRect, _name, gS);

            if (_isActive)
            {
                List<int> list = new List<int>();
                int max = _transformList.Count - 1;
                List<GUIContent> gContents = new List<GUIContent>();
                gContents.Add(new GUIContent("Custom"));
                list.Add(-1);
                for (int i = 0; i <= max; i++)
                {
                    TransformData item = _transformList[i];
                    if (/*item.IsCustom && */item != this)
                    {
                        list.Add(i);
                        gContents.Add(new GUIContent("clone: " + i));
                    }
                }
                int selectedIndex = list.IndexOf(CloneID);

                int newCloneID = EditorGUI.Popup(cloneRect, selectedIndex, gContents.ToArray());
                if (list[newCloneID] != selectedIndex)
                {
                    CloneID = list[newCloneID];
                }

                bool isMatching = false;

                RectTransform rectTrans = _transform as RectTransform;
                if (rectTrans != null)
                {
                    Rect oMinTit = new Rect(rect.x, topRect.yMax + svs + 1f, btnSize, slh * 2f + svs);
                    Rect oMinData = new Rect(oMinTit.xMax, oMinTit.y, rect.width - btnSize, slh);
                    Rect oMaxData = new Rect(oMinTit.xMax, oMinData.yMax + svs, rect.width - btnSize, slh);

                    GUI.enabled = Compare(_isRecord, OffsetMin != rectTrans.offsetMin || OffsetMax != rectTrans.offsetMax, ref isMatching);
                    if (GUI.Button(oMinTit, new GUIContent("P", "Anchored Offset. First row is minimum, Second row is maximum")))
                        RecordAttribute(_transform, 6);
                    GUI.enabled = enabled;

                    OffsetMin = EditorGUI.Vector2Field(oMinData, GUIContent.none, OffsetMin);
                    OffsetMax = EditorGUI.Vector2Field(oMaxData, GUIContent.none, OffsetMax);

                    Rect boxRect = new Rect(rect.x, oMaxData.yMax + svs, rect.width, slh * 2 + 6f);
                    EditorGUI.HelpBox(boxRect, "", MessageType.None);
                    float left = boxRect.x + 2f, top = boxRect.y + svs;

                    float ancBtnSize = btnSize + 4f;
                    Rect ancTit = new Rect(left, boxRect.center.y - (ancBtnSize / 2f), ancBtnSize, ancBtnSize);
                    Rect pivTit = new Rect(boxRect.center.x + 2f, top, btnSize, slh);
                    Rect sizTit = new Rect(pivTit.x, pivTit.yMax + svs, btnSize, slh);

                    Rect ancMinData = new Rect(ancTit.xMax + 2f, top, boxRect.width / 2f - ancTit.width - 5f, slh);
                    Rect ancMaxData = new Rect(ancMinData.x, ancMinData.yMax + svs, ancMinData.width, slh);
                    Rect pivData = new Rect(pivTit.xMax + 2f, top, boxRect.width / 2f - ancTit.width - 5f, slh);
                    Rect sizData = new Rect(sizTit.xMax + 2f, pivData.yMax + svs, pivData.width, slh);

                    GUI.enabled = Compare(_isRecord, AnchorMin != rectTrans.anchorMin || AnchorMax != rectTrans.anchorMax, ref isMatching);
                    if (GUI.Button(ancTit, new GUIContent("A", "RectTransform Anchors. First row is minimum, Second row is maximum")))
                        RecordAttribute(_transform, 3);
                    GUI.enabled = Compare(_isRecord, Pivot != rectTrans.pivot, ref isMatching);
                    if (GUI.Button(pivTit, new GUIContent("P", "RectTransform Pivot")))
                        RecordAttribute(_transform, 4);
                    GUI.enabled = Compare(_isRecord, SizeDelta != rectTrans.sizeDelta, ref isMatching);
                    if (GUI.Button(sizTit, new GUIContent("S", "RectTransform DeltaSize, prefer to the Width and Height of RectTransform")))
                        RecordAttribute(_transform, 5);
                    GUI.enabled = enabled;

                    AnchorMin = EditorGUI.Vector2Field(ancMinData, GUIContent.none, AnchorMin);
                    AnchorMax = EditorGUI.Vector2Field(ancMaxData, GUIContent.none, AnchorMax);
                    Pivot = EditorGUI.Vector2Field(pivData, GUIContent.none, Pivot);
                    SizeDelta = EditorGUI.Vector2Field(sizData, GUIContent.none, SizeDelta);

                    Rect rotTit = new Rect(rect.x, boxRect.yMax + svs, btnSize, slh);
                    Rect rotData = new Rect(rotTit.xMax, rotTit.y, rect.width - btnSize, slh);
                    Rect scaTit = new Rect(rect.x, rotTit.yMax + svs, btnSize, slh);
                    Rect scaData = new Rect(scaTit.xMax, scaTit.y, rect.width - btnSize, slh);

                    GUI.enabled = Compare(_isRecord, Rotation != _transform.localEulerAngles, ref isMatching);
                    if (GUI.Button(rotTit, new GUIContent("R", "Local Rotation")))
                        RecordAttribute(_transform, 1);
                    GUI.enabled = Compare(_isRecord, Scale != _transform.localScale, ref isMatching);
                    if (GUI.Button(scaTit, new GUIContent("S", "Local Scale")))
                        RecordAttribute(_transform, 2);
                    GUI.enabled = enabled;

                    Rotation = EditorGUI.Vector3Field(rotData, GUIContent.none, Rotation);
                    Scale = EditorGUI.Vector3Field(scaData, GUIContent.none, Scale);
                }
                else
                {
                    Rect posTit = new Rect(rect.x, topRect.yMax + svs + 1f, btnSize, slh);
                    Rect posData = new Rect(posTit.xMax, posTit.y, rect.width - btnSize, slh);
                    Rect rotTit = new Rect(rect.x, posTit.yMax + svs, btnSize, slh);
                    Rect rotData = new Rect(rotTit.xMax, rotTit.y, posData.width, slh);
                    Rect scaTit = new Rect(rect.x, rotTit.yMax + svs, btnSize, slh);
                    Rect scaData = new Rect(scaTit.xMax, scaTit.y, posData.width, slh);

                    GUI.enabled = Compare(_isRecord, Position != _transform.localPosition, ref isMatching);
                    if (GUI.Button(posTit, new GUIContent("P", "Local Position")))
                        RecordAttribute(_transform, 0);
                    GUI.enabled = Compare(_isRecord, Rotation != _transform.localEulerAngles, ref isMatching);
                    if (GUI.Button(rotTit, new GUIContent("R", "Local Rotation")))
                        RecordAttribute(_transform, 1);
                    GUI.enabled = Compare(_isRecord, Scale != _transform.localScale, ref isMatching);
                    if (GUI.Button(scaTit, new GUIContent("S", "Local Scale")))
                        RecordAttribute(_transform, 2);
                    GUI.enabled = enabled;

                    Position = EditorGUI.Vector3Field(posData, GUIContent.none, Position);
                    Rotation = EditorGUI.Vector3Field(rotData, GUIContent.none, Rotation);
                    Scale = EditorGUI.Vector3Field(scaData, GUIContent.none, Scale);
                }

                GUI.enabled = isMatching;
                Rect recAllTit = new Rect(topRect.xMax - btnSize, topRect.y, btnSize, slh);
                if (GUI.Button(recAllTit, new GUIContent("☄", "Click to record the current transform to this data")))
                    RecordAll(_transform);
            }
            else
            {
                GUI.Label(cloneRect, IsCloned(_transformList) ? "clone " + CloneID.ToString() : "Custom");
            }
            GUI.enabled = true;
        }
        private bool Compare(bool _isRecord, bool _bool, ref bool _isMatching)
        {
            if (_isRecord)
                return false;
            if (_bool)
            {
                _isMatching = true;
                return true;
            }
            return false;
        }
#endif
    }

    [System.Serializable]
    private class TransformAttribute
    {
        public bool position = true;
        public bool rotation = true;
        public bool scale = true;
        public bool anchors = true;
        public bool pivot = true;
        public bool sizeDelta = true;
    }

    [SerializeField]
    private List<TransformData> dataList = new List<TransformData>();
    [SerializeField]
    private TransformAttribute transformAttribute = new TransformAttribute();

    protected sealed override int currentLayoutMax
    {
        get
        {
            return dataList.Count;
        }
    }

    public void Record(Transform transform)
    {
        dataList[currentLayout].RecordAll(transform);
    }

    protected sealed override void Switch()
    {
        dataList[currentLayout].Set(transform, dataList, transformAttribute);
    }

#if UNITY_EDITOR
    private bool _isRecordMode = false;
    private static bool _isGlobalRecordMode = false;
    private bool isRecordMode
    {
        get
        {
            return _isRecordMode || _isGlobalRecordMode;
        }
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            if (isRecordMode && transform.hasChanged)
            {
                Record(transform);
            }
        }
    }
    protected override void Reset()
    {
        base.Reset();
        dataList.Clear();
        dataList.Add(new TransformData());
        dataList.Add(new TransformData());
        dataList[0].RecordAll(transform);
        dataList[1].RecordAll(transform);
    }

    [CustomEditor(typeof(TransformSwitcher))]
    private class Inspector : BaseInspector
    {
        private TransformSwitcher main;
        private ReorderableList reorderableList;

        public override void OnEnable()
        {
            base.OnEnable();
            main = mainBase as TransformSwitcher;

            reorderableList = new ReorderableList(main.dataList, typeof(TransformData), false, true, true, true);
            reorderableList.onCanRemoveCallback = (reoderableList) =>
            {
                return reorderableList.count > 2;
            };
            reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                TransformData data = main.dataList[index];

                bool isEnabled = (main.currentLayout == index);
                data.Draw(rect, main.transform, isEnabled, isActive, main.isRecordMode, SwitcherLayoutManager.GetName(index), main.dataList);
            };
            reorderableList.elementHeightCallback = (index) =>
            {
                float row = 1;
                if (reorderableList.index == index)
                {
                    row += 4;
                    if (main.transform is RectTransform)
                    {
                        row += 2.5f;
                    }
                }
                return row * EditorGUIUtility.singleLineHeight + (row) * EditorGUIUtility.standardVerticalSpacing;
            };
            reorderableList.drawHeaderCallback = (rect) =>
            {
                GUI.Label(rect, "Transform Data Collection");
            };
        }
        private void OnDisable()
        {
            main._isRecordMode = false;
        }
        const float size = 20f;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.DrawLayoutSelector();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            Rect rect = EditorGUILayout.GetControlRect();
            Rect singleRecord = new Rect(rect.x, rect.y, rect.width - size, EditorGUIUtility.singleLineHeight);
            Rect allRecord = new Rect(singleRecord.xMax, rect.y, size, EditorGUIUtility.singleLineHeight);

            main._isRecordMode = GUI.Toggle(singleRecord, main._isRecordMode, new GUIContent("RECORD", "Toggle auto tracking transform this session only"), EditorStyles.miniButtonLeft);
            _isGlobalRecordMode = GUI.Toggle(allRecord, _isGlobalRecordMode, new GUIContent("◍", "Toggle auto tracking for all TransformSwitcher"), EditorStyles.miniButtonRight);
            EditorGUILayout.EndHorizontal();
            reorderableList.DoLayoutList();

            DrawCustomAttribute();

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
        }
        private bool _showCustomAttribute = false;
        private void DrawCustomAttribute()
        {
            _showCustomAttribute = GUILayout.Toggle(_showCustomAttribute, new GUIContent(CustomAttributeTitle(),
                           "Show more detail on selected record and more function to control the data"),
                           EditorStyles.miniButton);
            if (_showCustomAttribute)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (main.transform is RectTransform)
                {
                    main.transformAttribute.position = GUILayout.Toggle(main.transformAttribute.position, new GUIContent("Position", "Rect Transform's offset min and offset max"));
                    main.transformAttribute.anchors = GUILayout.Toggle(main.transformAttribute.anchors, new GUIContent("Anchors", "Rect Transform's anchor min and anchor max"));
                    main.transformAttribute.pivot = GUILayout.Toggle(main.transformAttribute.pivot, new GUIContent("Pivot", "Rect Transform's scale"));
                    main.transformAttribute.sizeDelta = GUILayout.Toggle(main.transformAttribute.sizeDelta, new GUIContent("Size Delta", "Transform's scale"));
                }
                else
                {
                    main.transformAttribute.position = GUILayout.Toggle(main.transformAttribute.position, new GUIContent("Position", "Transform's position"));
                }
                main.transformAttribute.rotation = GUILayout.Toggle(main.transformAttribute.rotation, new GUIContent("Rotation", "Transform's rotation"));
                main.transformAttribute.scale = GUILayout.Toggle(main.transformAttribute.scale, new GUIContent("Scale", "Transform's scale"));

                EditorGUILayout.EndVertical();
            }
        }
        private bool Attribute_WillApplyAll
        {
            get
            {
                if (main.transformAttribute.position == false)
                    return false;
                if (main.transformAttribute.anchors == false)
                    return false;
                if (main.transformAttribute.pivot == false)
                    return false;
                if (main.transformAttribute.sizeDelta == false)
                    return false;
                if (main.transformAttribute.rotation == false)
                    return false;
                if (main.transformAttribute.scale == false)
                    return false;
                return true;
            }
        }
        private string CustomAttributeTitle()
        {
            if (Attribute_WillApplyAll)
                return "Attribute: Apply all";
            return "Attribute: custom";
        }
    }
#endif
}
