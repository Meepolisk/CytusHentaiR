using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using REditor;
#endif

namespace RTool.LayoutSwitcher
{
    public abstract class DataSwitcherInjector : LayoutSwitcherBase
    {
        [HideInInspector]
        [SerializeField]
        protected bool autoInject = true;

        [HideInInspector]
        [SerializeField]
        protected Component injectComponent;
        protected abstract bool ComponentIsValid();
        protected abstract void ApplyData(int _layerIndex);
        protected abstract bool CanAutoInject { get; }

#if UNITY_EDITOR

        protected override void Reset()
        {
            base.Reset();

            //apply auto search
            if (CanAutoInject)
            {
                if (injectComponent == null)
                    injectComponent = EditorSearch();
            }
        }

        protected abstract void EditorShowValidComponent();
        protected abstract Component EditorSearch();

        [CustomEditor(typeof(DataSwitcherInjector), true)]
        private class Inspector : BaseInspector
        {
            private DataSwitcherInjector main;
            private ReorderableList reorderableList;

            public override void OnEnable()
            {
                base.OnEnable();
                main = mainBase as DataSwitcherInjector;

                reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("dataList"), true, true, true, true);
                reorderableList.onCanRemoveCallback = (reoderableList) =>
                {
                    return reorderableList.count > 2;
                };
                reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;

                    Rect pointerRect = new Rect(rect.x, rect.y, pointerSize, rect.height);
                    Rect dataRect = new Rect(rect.x + pointerSize, rect.y, rect.width - pointerSize, EditorGUIUtility.singleLineHeight);
                    if (main.currentLayout == index)
                    {
                        EditorGUI.LabelField(pointerRect, "♫", REditorUtils.redFont);
                    }
                    EditorGUI.PropertyField(dataRect, element, GUIContent.none);
                };
                reorderableList.drawHeaderCallback = (rect) =>
                {
                    GUI.Label(rect, "Data Collections");
                };
            }
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                base.DrawLayoutSelector();

                EditorGUILayout.BeginVertical();

                GUILayout.Label("Data to inject", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawInjectDatas();
                EditorGUILayout.EndVertical();
                
                if (main.CanAutoInject == true)
                {
                    GUILayout.Label("Inject method", EditorStyles.centeredGreyMiniLabel);
                    main.autoInject = GUILayout.Toggle(main.autoInject, "Auto Inject", EditorStyles.miniButton);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    if (main.autoInject)
                    {
                        EditorGUILayout.BeginHorizontal();
                        string errorTxt = "";
                        GUI.enabled = !ValidateComponent(ref errorTxt);
                        if (GUILayout.Button(new GUIContent("Component", "Search valid component in this game object"), EditorStyles.miniButton))
                        {
                            var foundComponent = main.EditorSearch();
                            if (foundComponent != null)
                            {
                                main.injectComponent = foundComponent;
                            }
                        }
                        GUI.enabled = true;
                        main.injectComponent = EditorGUILayout.ObjectField(main.injectComponent, typeof(Component), true) as Component;
                        EditorGUILayout.EndHorizontal();
                        DrawHelpBox(errorTxt);
                    }
                    else
                    {
                        DrawPropertiesExcluding(serializedObject, "m_Script");
                    }
                    serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    DrawPropertiesExcluding(serializedObject, "m_Script");
                }
                

                EditorGUILayout.EndVertical();
            }
            const float pointerSize = 14f;
            private void DrawInjectDatas()
            {
                serializedObject.Update();
                reorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
            private bool ValidateComponent(ref string _errorText)
            {
                if (main.injectComponent == null)
                {
                    _errorText = "Please assign the component!";
                    return false;
                }
                else if (!main.ComponentIsValid())
                {
                    _errorText = "Invalid component type!";
                    return false;
                }
                return true;
            }
            private void DrawHelpBox(string errorText)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (!string.IsNullOrEmpty(errorText))
                {
                    GUIStyle fontColorStyle = new GUIStyle();
                    fontColorStyle.alignment = TextAnchor.MiddleCenter;
                    fontColorStyle.normal.textColor = Color.red;

                    GUILayout.Label(errorText, fontColorStyle);
                    GUILayout.Label("It should be one of the following component", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(12));
                    main.EditorShowValidComponent();
                }
                else
                {
                    GUILayout.Label("Injector will search the valid property to inject", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(12));
                }
                EditorGUILayout.EndVertical();
            }

            private bool DrawSwitchToggleButton(Rect rect, bool _value, string _onTitle, string _offTitle)
            {
                Rect leftRect, rightRect;
                if (_value == true)
                {
                    leftRect = new Rect(rect.x, rect.y, rect.width * 0.6f, rect.height);
                    rightRect = new Rect(rect.x + rect.width * 0.6f, rect.y, rect.width * 0.4f, rect.height);
                }
                else
                {
                    leftRect = new Rect(rect.x, rect.y, rect.width * 0.4f, rect.height);
                    rightRect = new Rect(rect.x + rect.width * 0.4f, rect.y, rect.width * 0.6f, rect.height);
                }
                bool res = _value;
                res = GUI.Toggle(leftRect, res, _offTitle, EditorStyles.miniButtonLeft);
                res = GUI.Toggle(rightRect, !res, _onTitle, EditorStyles.miniButtonRight);
                if (res != _value)
                    return res;
                return _value;
            }
        }
#endif
    }

    public abstract class DataSwitcherInjector<T> : DataSwitcherInjector
    {
        [SerializeField, HideInInspector]
        protected List<T> dataList;
        protected sealed override int currentLayoutMax
        {
            get
            {
                return dataList.Count;
            }
        }

        [HideInInspector]
        protected virtual Dictionary<Type, Action<Component, T>> ComponentDictionary { get { return null; }}
        protected sealed override bool CanAutoInject
        {
            get
            {
                return (ComponentDictionary != null && ComponentDictionary.Count > 0);
            }
        }

        protected abstract void OnManualSwitch(T _data);

        private void AutoApplyData(T _data)
        {
            if (injectComponent == null)
                return;
            
            Type selectedKey = null;

            foreach (var item in ComponentDictionary)
            {
                Type type = item.Key;
                if (injectComponent.GetType().IsSubclassOf(type) || injectComponent.GetType() == type)
                {
                    selectedKey = type;
                }
            }
            if (selectedKey != null)
            {
                ComponentDictionary[selectedKey](injectComponent, _data);
            }
        }

        protected sealed override void Switch()
        {
            ApplyData(GlobalLayout);
        }
        protected sealed override void ApplyData(int _layoutIndex)
        {
            if (_layoutIndex != currentLayout)
                return;

            if (_layoutIndex < 0 || _layoutIndex > currentLayoutMax - 1)
                return;
            
            T toInject = dataList[_layoutIndex];

            if (autoInject)
            {
                AutoApplyData(toInject);
            }
            else
            {
                OnManualSwitch(toInject);
            }
        }

        protected sealed override bool ComponentIsValid()
        {
            if (injectComponent == null)
                return false;
            return componentIsValid(injectComponent);
        }
        private bool componentIsValid(Component _comp)
        {
            foreach (var item in ComponentDictionary)
            {
                Type type = item.Key;
                if (_comp.GetType().IsSubclassOf(type) || _comp.GetType() == type)
                    return true;
            }
            return false;
        }
#if UNITY_EDITOR

        protected override void Reset()
        {
            //Debug.Log("Reset");
            dataList = new List<T>(new T[2]);
            base.Reset();
        }

        protected sealed override void EditorShowValidComponent()
        {
            foreach (var item in ComponentDictionary)
            {
                GUILayout.Label("-" + item.Key.Name, EditorStyles.centeredGreyMiniLabel, GUILayout.Height(12));
            }
        }
        protected sealed override Component EditorSearch()
        {
            Component[] components = gameObject.GetComponents<Component>();
            foreach (var component in components)
            {
                if (componentIsValid(component))
                    return component;
            }
            return null;
        }
#endif
    }
}