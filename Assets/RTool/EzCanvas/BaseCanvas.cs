using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using REditor;
using UnityEditor;
#endif

namespace RTool.EzCanvas
{
    [RequireComponent(typeof(CanvasGroup))]
    [DisallowMultipleComponent]
    public class BaseCanvas : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        protected CanvasGroup canvasGroup;

        public delegate void BaseCanvasDelegate(BaseCanvas baseCanvas);
        public BaseCanvasDelegate onShowFinished;
        public BaseCanvasDelegate onHideFinished;

        public virtual bool Visible
        {
            get
            {
#if UNITY_EDITOR
                if (IsPreview)
                    return (storedAlpha > 0);
#endif
                return (canvasGroup.alpha > 0);
            }
            set
            {
#if UNITY_EDITOR
                if (IsPreview)
                {
                    storedAlpha = value ? 1f : 0f;
                    canvasGroup.alpha = value ? 0f : 1f;
                    return;
                }
#endif
                canvasGroup.alpha = value ? 1f : 0f;
            }
        }
        public virtual bool Interactable
        {
            get => canvasGroup.interactable;
            set => canvasGroup.interactable = value;
        }
        public virtual bool BlocksRaycasts
        {
            get => canvasGroup.blocksRaycasts;
            set => canvasGroup.blocksRaycasts = value;
        }
        public virtual bool Enable
        {
            set
            {
                Visible = value;
                Interactable = value;
                BlocksRaycasts = value;
            }
        }

        protected virtual void OnShowFinished()
        {
            if (onShowFinished != null)
                onShowFinished(this);

        }
        protected virtual void OnHideFinished()
        {
            if (onHideFinished != null)
                onHideFinished(this);
        }
        protected virtual void OnDestroy()
        {
            //if (onHideFinished != null)
            //    onHideFinished(this);
        }

        private void Start()
        {
            if (Visible == true)
                Show();
        }

        public virtual void Show(bool _isOn = true)
        {
            //Debug.Log(this.gameObject.name + " is " + (_isOn ? "showing" : "hiding"), gameObject);
            Enable = _isOn;
            if (_isOn)
                OnShowFinished();
            else
                OnHideFinished();
        }

        public void Hide()
        {
            Show(false);
        }

        #region editor
#if UNITY_EDITOR
        protected static Dictionary<int, int> idList = new Dictionary<int, int>();
        protected static List<BaseCanvas> activeBaseCanvases = new List<BaseCanvas>();
        protected virtual void OnValidate()
        {
            if (gameObject.scene.name != null && idList.ContainsKey(GetInstanceID()) == false)
            {
                EditorAwake();
                idList.Add(GetInstanceID(), gameObject.GetInstanceID());
                activeBaseCanvases.Add(this);
                IsPreview = false;
                EditorApplication.hierarchyChanged += EditorApplication_hierarchyChanged;
            }
        }
        protected virtual void Reset()
        {
            EditorAwake();
        }
        protected virtual void EditorAwake()
        {
            AssignCanvas();
        }
        private void EditorApplication_hierarchyChanged()
        {
            if (this == null && idList.ContainsKey(GetInstanceID()))
            {
                EditorOnDestroy();
                EditorApplication.hierarchyChanged -= EditorApplication_hierarchyChanged;
            }
        }
        protected virtual void EditorOnDestroy()
        {
            GameObject go = EditorUtility.InstanceIDToObject(idList[GetInstanceID()]) as GameObject;
            if (go != null) //vẫn còn gameobject
            {
                go.GetComponent<CanvasGroup>().hideFlags = HideFlags.None;
                EditorUtility.SetDirty(go);
            }
            idList.Remove(GetInstanceID());
            activeBaseCanvases.Remove(this);
        }
        private void AssignCanvas()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        [SerializeField, HideInInspector]
        private float storedAlpha = -1f;
        internal bool IsPreview
        {
            get => (storedAlpha != -1f);
            set
            {
                if (IsPreview == value)
                    return;

                if (value == true)
                {
                    float curAlpha = canvasGroup.alpha;
                    canvasGroup.alpha = Visible ? 0f : 1f;
                    storedAlpha = curAlpha;
                    if (Application.isPlaying == false)
                        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
                }
                else
                {
                    canvasGroup.alpha = storedAlpha;
                    storedAlpha = -1f;
                    if (Application.isPlaying == false)
                        EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
                }
            }
        }
        internal bool EditorVisible {
            get => (canvasGroup.alpha > 0);
            set => IsPreview = value ^ Visible;
        }

        private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.ExitingEditMode)
            {
                IsPreview = false;
            }
        }

        [CustomEditor(typeof(BaseCanvas), true, isFallback = true)]
        internal class BaseCanvasInspector : RInspector<BaseCanvas>
        {
            protected override void OnEnable()
            {
                base.OnEnable();
                if (!Application.isPlaying)
                    handler.canvasGroup.hideFlags = HideFlags.HideInInspector;
                else
                    handler.canvasGroup.hideFlags = HideFlags.None;
            }

            protected override void DrawGUI()
            {
                DrawControlBox();
                DrawDefaultInspector();
            }
            protected virtual void DrawControlBox()
            {
                GUILayout.BeginHorizontal();
                DrawPreviewButton(EditorStyles.miniButton);

                GUILayout.Label(new GUIContent("⋮⋮"), EditorStyles.centeredGreyMiniLabel);
                
                bool visible = GUILayout.Toggle(handler.Visible, new GUIContent("Visible", "Visibility of canvas group"), EditorStyles.miniButtonLeft);
                if (visible != handler.Visible)
                {
                    Undo.RecordObject(handler.canvasGroup, "Canvas group visible");
                    handler.Visible = visible;
                }
                bool interactable = GUILayout.Toggle(handler.Interactable, 
                    new GUIContent("Interactable", "Is the group interactable (are the elements beneath the group enabled"), EditorStyles.miniButtonMid);
                if (interactable != handler.Interactable)
                {
                    Undo.RecordObject(handler.canvasGroup, "Canvas group interactable");
                    handler.Interactable = interactable;
                }

                bool blockRaycast = GUILayout.Toggle(handler.BlocksRaycasts,
                    new GUIContent("Blocks Raycasts", "Does this group block raycasting (allow collision)"), EditorStyles.miniButtonRight);
                if (blockRaycast != handler.BlocksRaycasts)
                {
                    Undo.RecordObject(handler.canvasGroup, "Canvas group block raycasts");
                    handler.BlocksRaycasts = blockRaycast;
                }
                GUILayout.EndHorizontal();
            }
            protected virtual void DrawPreviewButton(GUIStyle _guiStyle)
            {
                string stage = handler.Visible ? "Hide" : "Show";
                var value = GUILayout.Toggle(handler.IsPreview, new GUIContent(stage, "Toggle this to temporary " + stage + " this canvas." +
                    " This preview stage will end right after user enter play mode"), _guiStyle);
                if (value != handler.IsPreview)
                    PreviewButtonPressed(value);
            }
            protected virtual void PreviewButtonPressed(bool _value)
            {
                handler.IsPreview = _value;
            }
        }

        #region static menu call
        const string nameSpace = "EzCanvas";
        const string hideFlagName = RTool.rootNameSpace + " / " + nameSpace + " / " + "Hide all BaseCanvas";
        [MenuItem(hideFlagName)]
        private static void HideAllBaseCanvas()
        {
            foreach (var item in activeBaseCanvases)
            {
                if (item != null)
                {
                    item.EditorVisible = false;
                }
            }
        }
        [MenuItem(hideFlagName, true)]
        private static bool HideAllBaseCanvasCondition()
        {
            foreach (var item in activeBaseCanvases)
            {
                if (item.EditorVisible == true)
                    return true;
            }
            return false;
        }
        const string previewFlagName = RTool.rootNameSpace + " / " + nameSpace + " / " + "Reset all PreviewFlag";
        [MenuItem(previewFlagName)]
        private static void ResetAllPreviewFlag()
        {
            foreach (var item in activeBaseCanvases)
            {
                if (item != null)
                    item.IsPreview = false;
            }
        }
        [MenuItem(previewFlagName, true)]
        private static bool ResetAllPreviewFlagCondition()
        {
            foreach (var item in activeBaseCanvases)
            {
                if (item.IsPreview == true)
                    return true;
            }
            return false;
        }
        [MenuItem(RTool.rootNameSpace + "/" + nameSpace + "/Check Instance IDs")]
        private static void CheckID()
        {
            Debug.Log("Current idList count: " + idList.Count);
            foreach (var item in idList)
            {
                string str = "id: " + item.Key;
                str += ", in object: " + item.Value;
                GameObject go = EditorUtility.InstanceIDToObject(item.Value) as GameObject;
                if (go != null && go.scene.name != null)
                {
                    Debug.Log(str, go);
                }
                else
                {
                    Debug.Log(str);
                }
            }
            Debug.Log("Current activeBaseCanvases count: " + activeBaseCanvases.Count);
            foreach (var item in activeBaseCanvases)
            {
                Debug.Log(item.name, item);
            }
        }
        private static T SafeCreateObject<T>(string _undoMsg, params Type[] _additionComponent) where T : Component
        {
            return SafeCreateObject<T>(_undoMsg, null, _additionComponent);
        }

        private static T SafeCreateObject<T>(string _undoMsg, Action<T> onCreate = null, params Type[] _additionComponent) where T : Component
        {
            if (string.IsNullOrEmpty(_undoMsg))
                _undoMsg = "Create " + typeof(T).Name;

            T newObject = FindObjectOfType<T>();
            if (newObject == null)
            {
                List<Type> alltype = new List<Type> {typeof(T)};
                if (_additionComponent != null && _additionComponent.Length > 0)
                    alltype.AddRange(_additionComponent);

                newObject = new GameObject(typeof(T).Name, alltype.ToArray()).GetComponent<T>();
                if (onCreate != null)
                    onCreate(newObject);
                Undo.RegisterCreatedObjectUndo(newObject.gameObject, _undoMsg);
            }
            return newObject;
        }
        private static Canvas SafeFindCanvas(string _undoMsg)
        {
            Canvas currentCanvas = SafeCreateObject<Canvas>(_undoMsg, typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            currentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            SafeCreateObject<EventSystem>(_undoMsg, typeof(StandaloneInputModule));
            return currentCanvas;
        }

        [MenuItem("GameObject/" + nameSpace + "/BaseCanvas", false, 10)]
        //[MenuItem("GameObject/BaseCanvas", false, priority = -1)]
        private static void CreateBaseCanvas()
        {
            string smg = "Create " + typeof(BaseCanvas).Name;
            Canvas currentCanvas = SafeFindCanvas(smg);
            
            BaseCanvas baseCanvas = new GameObject(typeof(BaseCanvas).Name).AddComponent<BaseCanvas>();
            Undo.RegisterCreatedObjectUndo(baseCanvas.gameObject, smg);
            baseCanvas.EditorSetupCreatedViaContextMenu(currentCanvas.transform);
            Selection.SetActiveObjectWithContext(baseCanvas.gameObject, baseCanvas);
        }
        [MenuItem("GameObject/" + nameSpace + "/MonoCanvas", false, 10)]
        private static void CreateMonoCanvas()
        {
            string smg = "Create " + typeof(MonoCanvas).Name;
            Canvas currentCanvas = SafeFindCanvas(smg);
            MonoCanvasesController currentController = SafeCreateObject<MonoCanvasesController>(smg,
            onCreate: (controller =>
            {
                controller.EditorSetupCreatedViaContextMenu(currentCanvas.transform);
                //check if there is popup bg, set sibling below that
                PopupBackground popupBG = FindObjectOfType<PopupBackground>();
                if (popupBG != null)
                {
                    controller.transform.SetSiblingIndex(popupBG.transform.GetSiblingIndex());
                }
            }), typeof(RectTransform));

            MonoCanvas monoCanvas = new GameObject(typeof(MonoCanvas).Name).AddComponent<MonoCanvas>();
            Undo.RegisterCreatedObjectUndo(monoCanvas.gameObject, smg);
            //setup the controller
            monoCanvas.Controller = currentController;
            if (currentController.CurrentActiveCanvas == null)
            {
                currentController.EditorSetCurrentActiveCanvas(monoCanvas);
            }
            else
            {
                monoCanvas.Enable = false;
            }
            monoCanvas.EditorSetupCreatedViaContextMenu(currentController.transform);
            Selection.SetActiveObjectWithContext(monoCanvas.gameObject, monoCanvas);
        }
        
        [MenuItem("GameObject/" + nameSpace + "/Popup", false, 10)]
        private static void CreatePopup()
        {
            string smg = "Create " + typeof(PopupCanvas).Name;
            Canvas currentCanvas = SafeFindCanvas(smg);
            PopupBackground currentPopupBG = SafeCreateObject<PopupBackground>(smg,
            onCreate : (popup =>
            {
                popup.EditorSetupCreatedViaContextMenu(currentCanvas.transform);
                popup.transform.SetAsLastSibling();
            }), typeof(RectTransform));

            PopupCanvas newPopup = new GameObject(typeof(PopupCanvas).Name).AddComponent<PopupCanvas>();
            newPopup.EditorSetupCreatedViaContextMenu(currentPopupBG.transform);
            Selection.SetActiveObjectWithContext(newPopup.gameObject, newPopup);
        }
        #endregion
#endif
        #endregion
    }

#if UNITY_EDITOR
    internal static class ExtensionMethods
    {
        internal static void EditorSetupCreatedViaContextMenu(this Component component, Transform rootCanvas)
        {
            RectTransform rect = component.GetComponent<RectTransform>();
            if (rect == null)
                rect = component.gameObject.AddComponent<RectTransform>();

            rect.SetParent(rootCanvas);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero; rect.anchoredPosition = Vector2.zero;
        }
    }
#endif
}

