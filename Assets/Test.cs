using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RTool.Attribute;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Test : MonoBehaviour
{

    public void TraceMessage(string message,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
    {
        Debug.Log("message: " + message);
        Debug.Log("member name: " + memberName);
        Debug.Log("source file path: " + sourceFilePath);
        Debug.Log("source line number: " + sourceLineNumber);
    }

    private void BtnHit()
    {
        TraceMessage("oh no");
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Test))]
    private class editor : Editor
    {
        Test handler;

        private void OnEnable()
        {
            handler = target as Test;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("YEAH"))
            {
                Type newType = Type.GetType(typeof(NoteProfile[]).FullName.Replace("[]", string.Empty));
                Debug.Log(newType.Name);
            }
        }
    }
#endif
}
