using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Test : MonoBehaviour
{
#if UNITY_EDITOR
    [CustomEditor(typeof(Test))]
    private class editor : Editor
    {
        RectTransform rect;
        RectTransform rootRect;
        Test handler;

        private void OnEnable()
        {
            handler = target as Test;
            rect = handler.GetComponent<RectTransform>();
            rootRect = handler.transform.parent.GetComponent<RectTransform>();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("640x480"))
            {
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 640);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 480);
            }
            if (GUILayout.Button("Reset"))
            {
                Vector3[] corners = new Vector3[4];
                rootRect.GetLocalCorners(corners);
                var dtH = Vector3.Distance(corners[1], corners[2]);
                var dtV = Vector3.Distance(corners[0], corners[1]);
                Debug.Log(dtH);
                Debug.Log(dtV);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, dtH);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dtV);
            }
        }
    }
#endif
}
