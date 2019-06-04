using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTool.EzCanvas
{
    public class MonoCanvas : BaseCanvas
    {
        [SerializeField, HideInInspector]
        internal MonoCanvasesController controller;
        public MonoCanvasesController Controller
        {
            get
            {
                return controller;
            }
            set
            {
                if (controller == value)
                    return;
                RefreshController(value);

                controller = value;
            }
        }
        public bool IsShowing
        {
            get
            {
                return (controller.CurrentActiveCanvas == this);
            }
        }
        [HideInInspector]
        public bool IgnoreStack = false;

        private void RefreshController(MonoCanvasesController _new)
        {
            if (controller != null)
                controller.UnRegistMono(this);
            if (_new != null)
                _new.RegistMono(this);
        }
        #region public call
        public override void Show(bool _isOn = true)
        {
            if (controller == null)
            {
                base.Show(_isOn);
                return;
            }

            if (controller.IsBusy == false)
            {
                if (_isOn == true)
                    controller.PagingNewMono(this);
                else
                    controller.PoppinOldMono();
            }
            else
                base.Show(_isOn);
        }
        #endregion

        internal void BaseShow(bool _isOn)
        {
            base.Show(_isOn);
        }

#if UNITY_EDITOR
        protected override void EditorOnDestroy()
        {
            base.EditorOnDestroy();
            Controller = null;
        }

        [CustomEditor(typeof(MonoCanvas), true, isFallback = true)]
        private class MonoCanvasInspector : BaseCanvasInspector
        {
            private new MonoCanvas handler;
            protected override void OnEnable()
            {
                base.OnEnable();
                handler = base.handler as MonoCanvas;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
            }
            protected override void DrawControlBox()
            {
                handler.Controller = EditorGUILayout.ObjectField(new GUIContent("Controler",
                    "The Controller that inherit from MonoCanvasController. You must assign this, otherwise, this system wont work"),
                    handler.Controller,
                    typeof(MonoCanvasesController), true) as MonoCanvasesController;
                if (handler.Controller == null)
                {
                    EditorGUILayout.HelpBox(
                       "Must have " + typeof(MonoCanvasesController).Name + ". Otherwise this " + typeof(MonoCanvas).Name + " will work as a normal " + typeof(BaseCanvas).Name,
                       MessageType.Error, true);
                }
                else
                {
                    if (Application.isPlaying)
                        base.DrawControlBox();
                    else
                        DrawToolKit();
                }
            }
            private void DrawToolKit()
            {
                GUILayout.BeginHorizontal();
                DrawPreviewButton(EditorStyles.miniButtonLeft);
                bool isActive = handler.controller.CurrentActiveCanvas == handler;
                bool enable = GUILayout.Toggle(isActive, new GUIContent("Set As Default", "Set this MonoCanvas to default canvas of controller." +
                    ((handler.controller.CurrentActiveCanvas != null)? (" Current  default MonoCanvas is " + handler.controller.CurrentActiveCanvas.gameObject.name + ".") : null)), EditorStyles.miniButtonRight);
                if (enable != isActive)
                {
                    if (enable)
                        handler.controller.EditorSetCurrentActiveCanvas(handler);
                    else
                        handler.controller.EditorSetCurrentActiveCanvas(null);
                }
                GUILayout.EndHorizontal();
            }

            protected override void PreviewButtonPressed(bool _value)
            {
                if (handler != handler.controller.CurrentActiveCanvas)
                {
                    if (_value == true)
                        handler.Controller.ForceVisible(handler);
                    else
                        handler.Controller.ResetAllPreview();
                }
                else
                {
                    base.PreviewButtonPressed(_value);
                }
            }
        }
#endif
    }
}
