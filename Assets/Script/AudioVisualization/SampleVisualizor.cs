using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleVisualizor : MonoBehaviour
{
    [SerializeField, Range(50f,400f)]
    private float upScale = 50f;
    [SerializeField]
    private AudioPeer audioPeer;
    [SerializeField]
    private GameObject prefabs;

    private GameObject[] cubes { get; set; }
    const float normalScale = 0.1f;

    private void Start()
    {
        cubes = new GameObject[audioPeer.GetSamples.Length];
        for (int i = 0; i < cubes.Length; i++)
        {
            GameObject newGO = Instantiate(prefabs, transform);
            newGO.name = "Sample " + i;
            cubes[i] = newGO;
        }
    }

    private void Update()
    {
        for (int i = 0; i< cubes.Length; i++)
        {
            cubes[i].transform.localScale = new Vector3(normalScale, audioPeer.GetSamples[i] * upScale, normalScale);
            cubes[i].transform.localPosition = new Vector3((float)i * normalScale, audioPeer.GetSamples[i] * (upScale / 2f), 0);
        }
    }
}
