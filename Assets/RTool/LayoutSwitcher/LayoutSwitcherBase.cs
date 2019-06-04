using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
#endif

namespace RTool.LayoutSwitcher
{
    [ExecuteInEditMode]
    public abstract class LayoutSwitcherBase : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            if (Application.isPlaying)
            {
                activeSwitchers.Add(this);
                baseSwitch();
                Debug.Log("activeSwitcher add " + this.GetType().Name, gameObject);
            }
            //#if UNITY_EDITOR
            //            else
            //            {
            //                Debug.Log("OnEnable");
            //                baseSwitch();
            //            }
            //#endif
        }

        protected virtual void OnDisable()
        {
            if (Application.isPlaying)
            {
                if (activeSwitchers.Contains(this))
                {
                    activeSwitchers.Remove(this);
                    Debug.Log("activeSwitcher remove " + this.GetType().Name, gameObject);
                }
            }
        }
#if UNITY_EDITOR
        protected virtual void Reset()
        {
            baseSwitch();
            //ApplyCurrentLayout();
        }

        private void ApplyCurrentLayout()
        {
            if (currentLayout != GlobalLayout)
            {
                //currentLayout = GlobalLayout;
            }
        }
#endif
        [HideInInspector]
        [SerializeField]
        internal int currentLayout = 0;
        protected abstract int currentLayoutMax { get; }

        #region depth level check
        private int _depthLevel = 0;
        public int DepthLevel
        {
            get
            {
                return _depthLevel;
            }
        }
        public void GetParentIndex()
        {
            _depthLevel = 0;
            Transform checkingTransform = transform.parent;
            while (checkingTransform != null)
            {
                _depthLevel++;
                checkingTransform = checkingTransform.parent;
            }
        }
        #endregion

        protected void baseSwitch()
        {
            if (currentLayout == GlobalLayout)
                return;
            if (!GlobalLayoutIsValid())
                return;
            currentLayout = GlobalLayout;
            Switch();
        }
        protected abstract void Switch();
        protected bool GlobalLayoutIsValid()
        {
            if (GlobalLayout < currentLayoutMax)
                return true;
            return false;
        }

        #region static stuff
        private static int _GlobalLayout = -1;

        public static int GlobalLayout
        {
            get
            {
                if (_GlobalLayout < 0)
                {
                    _GlobalLayout = LoadDefaultGlobalLayout();
                    Debug.Log("Load default layout = " + _GlobalLayout);
                }
                return _GlobalLayout;
            }
            set
            {
                if (_GlobalLayout == value)
                    return;

                Debug.Log("GlobalLayout switch from " + _GlobalLayout + " to " + value);
                _GlobalLayout = value;
                PlayerPrefs.SetInt(DefaultLayoutPRCode, _GlobalLayout);
                PlayerPrefs.Save();
                StartEvent_OnSwitch(_GlobalLayout);
                Activate();
                StartEvent_AfterSwitch(_GlobalLayout);
            }
        }
        const string DefaultLayoutPRCode = "LayoutSwitcherDefaultLayout";
        private static int LoadDefaultGlobalLayout()
        {
            return PlayerPrefs.GetInt(DefaultLayoutPRCode);
        }

        private static LayoutSwitcherBase[] GetAllPosibleOtherLayoutSwitcherBase()
        {
            List<LayoutSwitcherBase> results = new List<LayoutSwitcherBase>();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (s.isLoaded)
                    {
                        var allGameObjects = s.GetRootGameObjects();
                        for (int j = 0; j < allGameObjects.Length; j++)
                        {
                            var go = allGameObjects[j];
                            LayoutSwitcherBase[] toAdd = go.GetComponentsInChildren<LayoutSwitcherBase>(true);
                            foreach (var item in toAdd)
                            {
                                if (item != null)
                                    results.Add(item);
                            }
                        }
                    }
                }
            }
#endif
            if (results.Count == 0)
            {
                return activeSwitchers.ToArray();
            }
            return results.ToArray();
        }
        private static List<LayoutSwitcherBase> activeSwitchers = new List<LayoutSwitcherBase>();
        private static void Activate()
        {
#if UNITY_EDITOR
            SwitcherLayoutManager.CheckToSet(GlobalLayout);
#endif
            LayoutSwitcherBase[] unSorted = GetAllPosibleOtherLayoutSwitcherBase();
            List<LayoutSwitcherBase> list = new List<LayoutSwitcherBase>();
            foreach (var switcher in unSorted)
            {
                switcher.GetParentIndex();
                list.Add(switcher);
            }
            list.Sort((x, y) => x.DepthLevel.CompareTo(y.DepthLevel));

            foreach (var switcher in list)
            {
                switcher.baseSwitch();
            }
        }

        #region event
        public static Action<int> onLayoutSwitched;

        private static void StartEvent_OnSwitch(int _layout)
        {
            if (onLayoutSwitched != null)
            {
                onLayoutSwitched(_layout);
            }
        }
        public static Action<int> afterLayoutSwitched;

        private static void StartEvent_AfterSwitch(int _layout)
        {
            if (afterLayoutSwitched != null)
            {
                afterLayoutSwitched(_layout);
            }
        }
        #endregion
        #endregion

#if UNITY_EDITOR
        internal class BaseInspector : Editor
        {
            protected LayoutSwitcherBase mainBase { private set; get; }

            public virtual void OnEnable()
            {
                mainBase = target as LayoutSwitcherBase;

                if (Application.isPlaying)
                    return;
                //RefreshLayoutSelector();
            }

            protected void DrawLayoutSelector()
            {
                GUILayout.BeginVertical();
                Rect mainRect = EditorGUILayout.GetControlRect();
                Rect titleRect = new Rect(mainRect.x, mainRect.y, mainRect.width * 0.4f, mainRect.height);
                Rect selectorRect = new Rect(mainRect.x + mainRect.width * 0.4f, mainRect.y, mainRect.width * 0.6f, mainRect.height);

                GUI.Label(titleRect, "Layout");


                List<GUIContent> list = new List<GUIContent>();
                for (int i = 0; i < mainBase.currentLayoutMax; i++)
                    list.Add(new GUIContent("Layout " + i));
                if (mainBase.GlobalLayoutIsValid())
                {
                    DrawActualSelector(selectorRect, list, mainBase.currentLayout);
                }
                else
                {
                    Color normalColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;
                    list.Add(new GUIContent("Error"));
                    DrawActualSelector(selectorRect, list, list.Count - 1);
                    GUI.backgroundColor = normalColor;
                    DrawWarning();
                }
                GUILayout.EndVertical();
            }
            private void DrawWarning()
            {
                EditorGUILayout.HelpBox(
                    "The CurrentLayoutIndex of this Switcher is " + mainBase.currentLayout + ", but The GlobalLayoutIndex is " + GlobalLayout + ", which doesn't supported in this Switcher",
                    MessageType.Error, true);
            }
            private void DrawActualSelector(Rect _rect, List<GUIContent> _list, int _selectedIndex)
            {
                int tmpLayout = EditorGUI.Popup(_rect, _selectedIndex, _list.ToArray());
                if (tmpLayout != mainBase.currentLayout)
                {
                    GlobalLayout = tmpLayout;
                }
            }
            protected float LayoutSelectorHeight
            {
                get
                {
                    return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }
        }
#endif
    }
}