using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTool.EzCanvas
{
    public class PopupCanvas : BaseCanvas
    {
        public override void Show(bool _isOn = true)
        {
            if (_isOn == true)
            {
                if (PopupBackground.AutoInstance == null)
                    Debug.LogWarning("PopupCanvas trying to show up, but cannot find PopupBackground...");
                else
                    PopupBackground.AutoInstance.RegistPopup(this);
            }
            base.Show(_isOn);
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(PopupCanvas), true, isFallback = true)]
        private class PopUpInspector : BaseCanvasInspector
        {
            //private new PopupCanvas handler { get; set; }
            //protected override void OnEnable()
            //{
            //    base.OnEnable();
            //    handler = base.handler as PopupCanvas;
            //}

            //public override void OnInspectorGUI()
            //{
            //    base.OnInspectorGUI();
            //}
            protected override void DrawControlBox()
            {
                var bg = FindObjectOfType<PopupBackground>();
                if (bg == null)
                {
                    EditorGUILayout.HelpBox(
                       "Could found any instance of " + typeof(PopupBackground).Name + ". This " + typeof(PopupCanvas).Name + " will behave as normal "
                       + typeof(BaseCanvas).Name,
                       MessageType.Warning, true);
                }
                base.DrawControlBox();
            }
        }
#endif
    }
}
