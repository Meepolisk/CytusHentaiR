using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTool.LayoutSwitcher
{
    public class SwitcherLayoutManager : MonoBehaviour
    {
        private static SwitcherLayoutManager Instance;
        private static readonly Vector2Int defaultResolution = new Vector2Int(800, 600);

        [SerializeField]
        private List<Layout> LayoutList = new List<Layout>();

        public static Layout Get(int _index)
        {
            if (Instance == null)
                return null;
            return Instance.LayoutList[_index];
        }
        private static Vector2Int GetResolution(int _index)
        {
            if (Instance == null)
                return defaultResolution;
            return Instance.LayoutList[_index].Resolution;
        }
        private static Vector2Int GetResolution(string _name)
        {
            if (Instance == null)
                return defaultResolution;
            foreach (var layout in Instance.LayoutList)
            {
                if (layout.Name == _name)
                    return layout.Resolution;
            }
            return defaultResolution;
        }

#if UNITY_EDITOR
        public static string GetName(int _index)
        {
            try
            {
                if (Instance == null)
                    return "Layout " + _index;

                string res = Instance.LayoutList[_index].Name;
                if (string.IsNullOrEmpty(res))
                    return "Unamed(" + _index + ")";
                return res;
            }
            catch
            {
                return "Layout " + _index;
            }
        }

        public static void CheckToSet(int _index)
        {
            if (Instance == null)
                return;
            if (Instance.LayoutList.Count == 0)
                return;

            if (!(_index < 0 || _index >= Instance.LayoutList.Count))
            {
                Instance.SetSize(
                    Mathf.Clamp(Instance.LayoutList[_index].Resolution.x, 2, 4092),
                    Mathf.Clamp(Instance.LayoutList[_index].Resolution.y, 2, 4092));
            }
        }

        //private void Reset()
        //{
        //    if (Instance != null)
        //    {
        //        EditorUtility.DisplayDialog("Error!", "There is already a " + this.GetType().Name + " in Asset, named " + Instance.name + "!", "Okay");
        //        Debug.LogWarning("There is already a " + this.GetType().Name + " in Asset, named " + Instance.name + "!", Instance);
        //        DestroyImmediate(this);
        //    }
        //}

        public void SetSize(int _w, int _h)
        {
            GameViewUtils.SetSize(_w, _h);
        }

        internal static class GameViewUtils
        {
            static object gameViewSizesInstance;
            static MethodInfo getGroup;

            static GameViewUtils()
            {
                // gameViewSizesInstance  = ScriptableSingleton<GameViewSizes>.instance;
                var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
                var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var instanceProp = singleType.GetProperty("instance");
                getGroup = sizesType.GetMethod("GetGroup");
                gameViewSizesInstance = instanceProp.GetValue(null, null);
            }

            public enum GameViewSizeType
            {
                AspectRatio, FixedResolution
            }

            public static void SetSize(int _w, int _h)
            {
                GameViewSizeGroupType currentGroupType = GetCurrentGroupType();
                int count = 0;
                int idx = FindSize(currentGroupType, _w, _h, ref count);
                if (idx == -1)
                {
                    AddCustomSize(GameViewSizeType.FixedResolution, currentGroupType, _w, _h);
                    idx = count;
                }
                SetSize(idx);
            }

            private static void SetSize(int index)
            {
                var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
                var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var gvWnd = EditorWindow.GetWindow(gvWndType);
                selectedSizeIndexProp.SetValue(gvWnd, index, null);
            }

            private static void AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height)
            {
                // GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupTyge);
                // group.AddCustomSize(new GameViewSize(viewSizeType, width, height, text);

                var group = GetGroup(sizeGroupType);
                var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize"); // or group.GetType().
                var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
                var ctor = gvsType.GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });
                var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, string.Empty });
                addCustomSize.Invoke(group, new object[] { newSize });
            }

            private static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
            {
                // GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupType);
                // string[] texts = group.GetDisplayTexts();
                // for loop...

                var group = GetGroup(sizeGroupType);
                var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
                var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
                for (int i = 0; i < displayTexts.Length; i++)
                {
                    string display = displayTexts[i];
                    // the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
                    // so if we're querying a custom size text we substring to only get the name
                    // You could see the outputs by just logging
                    // Debug.Log(display);
                    int pren = display.IndexOf('(');
                    if (pren != -1)
                        display = display.Substring(0, pren - 1); // -1 to remove the space that's before the prens. This is very implementation-depdenent
                    if (display == text)
                        return i;
                }
                return -1;
            }

            private static int FindSize(GameViewSizeGroupType sizeGroupType, int width, int height, ref int _maxCount)
            {
                // goal:
                // GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupType);
                // int sizesCount = group.GetBuiltinCount() + group.GetCustomCount();
                // iterate through the sizes via group.GetGameViewSize(int index)

                var group = GetGroup(sizeGroupType);
                var groupType = group.GetType();
                var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
                var getCustomCount = groupType.GetMethod("GetCustomCount");
                int bultingCount = (int)getBuiltinCount.Invoke(group, null);
                int sizesCount = bultingCount + (int)getCustomCount.Invoke(group, null);
                var getGameViewSize = groupType.GetMethod("GetGameViewSize");
                var gvsType = getGameViewSize.ReturnType;
                var widthProp = gvsType.GetProperty("width");
                var heightProp = gvsType.GetProperty("height");
                var indexValue = new object[1];
                _maxCount = sizesCount;
                for (int i = bultingCount; i < sizesCount; i++)
                {
                    indexValue[0] = i;
                    var size = getGameViewSize.Invoke(group, indexValue);
                    int sizeWidth = (int)widthProp.GetValue(size, null);
                    int sizeHeight = (int)heightProp.GetValue(size, null);
                    if (sizeWidth == width && sizeHeight == height)
                        return i;
                }
                return -1;
            }

            //public static bool SizeExists(GameViewSizeGroupType sizeGroupType, int width, int height)
            //{
            //    return FindSize(sizeGroupType, width, height, _maxCount) != -1;
            //}

            static object GetGroup(GameViewSizeGroupType type)
            {
                return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
            }

            public static void LogCurrentGroupType()
            {
                Debug.Log(GetCurrentGroupType());
            }
            public static GameViewSizeGroupType GetCurrentGroupType()
            {
                var getCurrentGroupTypeProp = gameViewSizesInstance.GetType().GetProperty("currentGroupType");
                return (GameViewSizeGroupType)(int)getCurrentGroupTypeProp.GetValue(gameViewSizesInstance, null);
            }
        }

        //[CustomEditor(typeof(SwitcherDebugHelper))]
        //public class LocalizationEditor : Editor
        //{
        //    SwitcherDebugHelper main;
        //    ReorderableList reorderableList;

        //    private void OnEnable()
        //    {
        //        main = target as SwitcherDebugHelper;
        //        reorderableList = new ReorderableList(main.LayoutList, typeof(Layout), true, true, true, true);
        //        reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
        //        {
        //            Vector2Int data = main.LayoutList[index];

        //            bool isEnabled = (main.currentLayout == index);
        //            data.Draw(rect, main.transform, isEnabled, isActive, main.isRecordMode, "Layout " + index);
        //        };
        //        //reorderableList.elementHeightCallback = (index) =>
        //        //{
        //        //    float row = 1;
        //        //    if (reorderableList.index == index)
        //        //    {
        //        //        row += 3;
        //        //        if (main.transform is RectTransform)
        //        //        {
        //        //            row += 2.3f;
        //        //        }
        //        //    }
        //        //    return row * EditorGUIUtility.singleLineHeight + (row) * EditorGUIUtility.standardVerticalSpacing;
        //        //};
        //        //reorderableList.drawHeaderCallback = (rect) =>
        //        //{
        //        //    GUI.Label(rect, "Transform Data Collection");
        //        //};
        //    }

        //    public override void OnInspectorGUI()
        //    {

        //    }
        //}


        #region RMonoBehaviour
        //private static int? switcherInstanceID = null;
        private void OnValidate()
        {
            //if (switcherInstanceID != null && string.IsNullOrEmpty(gameObject.scene.name) == false)
            if (Instance == null)
            {
                Debug.Log("SwitcherGameviewHelper is active, current instance: " + this.name, this);
                //switcherInstanceID = GetInstanceID();
                Instance = this;
                EditorApplication.hierarchyChanged += OnHierachyChange;
            }
            else
            {
                Debug.Log("SwitcherGameviewHelper is active already: " + Instance.gameObject.name, Instance.gameObject);
                EditorApplication.delayCall = () => { DestroyImmediate(this); };
            }
        }
        private void OnHierachyChange()
        {
            if (this == null)
            {
                Instance = null;
                EditorApplication.hierarchyChanged -= OnHierachyChange;
            }

        }
        #endregion
#endif
    }

    [System.Serializable]
    public class Layout
    {
        [SerializeField]
        private string _name;
        public string Name
        {
            get { return _name; }
        }

        [SerializeField]
        private Vector2Int _resolution = new Vector2Int(800, 600);
        public Vector2Int Resolution
        {
            get { return _resolution; }
        }

        public Layout(string name, Vector2Int resolution)
        {
            _name = name; _resolution = resolution;
        }
    }
}