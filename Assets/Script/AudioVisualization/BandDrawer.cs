using RTool.Attribute;
using RTool.AudioAnalyze;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class BandDrawer : BandVisualizorBase
{
    [SerializeField]
    private RectTransform drawMark = null;
    [SerializeField]
    private RectTransform content = null;
    [SerializeField]
    private float xDelta = 0.5f;

    private new LineRenderer renderer { get; set; }

    private void Awake()
    {
        renderer = GetComponent<LineRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        lastPosX = GetX;
    }

    float lastPosX { get; set; }
    protected override void ValueUpdate(float Value, float bufferedValue)
    {
        if (lastPosX > GetX + xDelta)
            Draw(bufferedValue);
    }
    private void Draw(float _value)
    {
        lastPosX = GetX;
        renderer.positionCount++;
        renderer.SetPosition(renderer.positionCount - 1, new Vector3(lastPosX, _value * 100f, 0f));
    }
    private float GetX => drawMark.anchoredPosition.x - content.sizeDelta.x;
}
