using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BandVisualizorController : MonoBehaviour
{
    [SerializeField, Range(100f,1000f)]
    private float upScale = 200f;
    public float UpScale => upScale;

}
