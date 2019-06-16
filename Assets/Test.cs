using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RTool.Attribute;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Test : MonoBehaviour
{
    [SerializeField]
    private AudioSource source = null;
    [SerializeField]
    private AudioClip clip = null;
    [SerializeField]
    private int ofsetSamples;

    [SerializeField, ReadOnly]
    float[] samples;

    private void Zo()
    {
        if (clip == null)
            return;
        float[] newArray = new float[100];
        clip.GetData(newArray, ofsetSamples);
        for (int i = 0; i < newArray.Length; i++)
            Debug.LogFormat("[{0}]: {1}", i.ToString(), newArray[i].ToString());
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
                    handler.Zo();
                }
                
            }
        }
    #endif
}
