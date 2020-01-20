using RTool;
using RTool.AudioAnalyze;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BandDrawerController : MonoBehaviour
{
    [SerializeField]
    private RectTransform content = null;
    [SerializeField, Range(10f, 100f), ReadOnly]
    private float contentSpeed = 30f;
    [SerializeField, ReadOnly]
    private float initWidth = 30f;

    private void Update()
    {
        Vector2 sizeDelta = content.sizeDelta;
        //sizeDelta.x = AudioPeer.Instance.AudioSource.time + initWidth;
        sizeDelta.x = (Time.time * contentSpeed) + initWidth;
        content.sizeDelta = sizeDelta;
    }
}
