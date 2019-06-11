using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BandVisualizor : MonoBehaviour
{
    [Header("Component Ref")]
    [SerializeField]
    private BandVisualizorController controller = null;
    [SerializeField]
    private AudioPeer audioPeer = null;
    [SerializeField]
    private BandType bandType = BandType.SubBass;
    [SerializeField]
    private Image mesh = null;
    [SerializeField]
    private Image bufferedMesh = null;
    [SerializeField]
    private new Text name = null;
    [SerializeField]
    private Text value = null;

    private float UpScale => controller.UpScale;

    private void Start()
    {
        audioPeer.BandsDict[bandType].onUpdateValue += ValueUpdate;
        name.text = bandType.ToString();
    }
    private void OnDestroy()
    {
        audioPeer.BandsDict[bandType].onUpdateValue -= ValueUpdate;
    }

    private const float minY = 5f;
    private void ValueUpdate(float Value, float bufferedValue)
    {
        Vector2 meshSD = mesh.rectTransform.sizeDelta;
        meshSD.y = Mathf.Clamp(Value * UpScale, minY, 999999999);
        mesh.rectTransform.sizeDelta = meshSD;

        Vector2 bufferedMeshSD = bufferedMesh.rectTransform.sizeDelta;
        bufferedMeshSD.y = Mathf.Clamp(bufferedValue * UpScale, minY, 999999999);
        bufferedMesh.rectTransform.sizeDelta = bufferedMeshSD;

        value.text = bufferedValue.ToString();
    }
}
