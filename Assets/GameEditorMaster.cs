using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEditorMaster : MonoBehaviour
{
    [SerializeField, Range(0f, 10f)]
    private float timeScale = 1f;
    private float TimeScale
    {
        set
        {
            timeScale = value;
            Time.timeScale = timeScale;
        }
    }

    private void OnValidate()
    {
        TimeScale = timeScale;
    }
}
