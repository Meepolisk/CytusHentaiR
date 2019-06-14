using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTool.AudioAnalyze;

public class BandVisualizor : BandVisualizorBase
{
    [Header("Component Ref")]
    [SerializeField]
    BandVisualizorController controller = null;
    [SerializeField]
    private Image mesh = null;
    [SerializeField]
    private Image bufferedMesh = null;
    [SerializeField]
    private new Text name = null;
    [SerializeField]
    private Text value = null;
    private float boost => controller.UpScale;

    protected override void Start()
    {
        base.Start();
        name.text = bandType.ToString();
    }

    private const float minY = 5f;
    protected override void ValueUpdate(float Value, float bufferedValue)
    {
        Vector2 meshSD = mesh.rectTransform.sizeDelta;
        meshSD.x = Mathf.Clamp(Value * boost, minY, 999999999);
        mesh.rectTransform.sizeDelta = meshSD;

        Vector2 bufferedMeshSD = bufferedMesh.rectTransform.sizeDelta;
        bufferedMeshSD.x = Mathf.Clamp(bufferedValue * boost, minY, 999999999);
        bufferedMesh.rectTransform.sizeDelta = bufferedMeshSD;

        value.text = bufferedValue.ToString();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        if (name)
        {
            name.text = bandType.ToString();
        }
    }
}
