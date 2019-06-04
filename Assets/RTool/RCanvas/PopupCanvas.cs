using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTool.EzCanvas
{
    class PopupCanvas : BaseCanvas
    {
        [SerializeField, HideInInspector]
        private PopupBackground PopupBackground = null;

        public override void Show(bool _isOn = true)
        {
            if (PopupBackground != null && _isOn == true)
            {
                PopupBackground.RegistPopup(this);
            }
            base.Show(_isOn);
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(PopupCanvas), true, isFallback = true)]
        private class PopUpInspector : BaseCanvasInspector
        {
            private new PopupCanvas handler { get; set; }
            protected override void OnEnable()
            {
                base.OnEnable();
                handler = base.handler as PopupCanvas;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
            }
            protected override void DrawControlBox()
            {
                handler.PopupBackground = EditorGUILayout.ObjectField(new GUIContent("Background",
                    "PopupBackground component"),
                    handler.PopupBackground,
                    typeof(PopupBackground), true) as PopupBackground;
                if (handler.PopupBackground == null)
                {
                    EditorGUILayout.HelpBox(
                       "Must have " + typeof(PopupBackground).Name + ". Otherwise this " + typeof(PopupCanvas).Name + " will work as a normal " + typeof(BaseCanvas).Name,
                       MessageType.Error, true);
                }
                base.DrawControlBox();
            }
        }
#endif
    }
}
