using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BandVisualizor : MonoBehaviour
{
    [SerializeField, Range(50f,200f)]
    private float upScale = 50f;
    [SerializeField]
    private AudioPeer audioPeer;
    [SerializeField]
    private GameObject prefabs;

    private GameObject[] cubes { get; set; }
    const float normalScale = 10f;
    const float range = 2f;

    private void Start()
    {
        cubes = new GameObject[audioPeer.GetFreqBands.Length];
        for (int i = 0; i < cubes.Length; i++)
        {
            GameObject newGO = Instantiate(prefabs, transform);
            newGO.name = "Bands " + i;
            cubes[i] = newGO;
        }
    }

    private void Update()
    {
        for (int i = 0; i< cubes.Length; i++)
        {
            cubes[i].transform.localScale = new Vector3(normalScale, audioPeer.GetFreqBands[i].value * upScale, normalScale);
            cubes[i].transform.localPosition = new Vector3(((float)i * normalScale) + range, audioPeer.GetFreqBands[i].value * (upScale / 2f), 0);
        }
    }
}
