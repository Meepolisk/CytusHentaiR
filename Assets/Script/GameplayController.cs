using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(VideoPlayer))]
public class GameplayController : MonoBehaviour
{
    [SerializeField]
    private List<CytusPlayer> allCytusPlayer = null;

    private void Start()
    {
        StartCoroutine(_Start());
    }
    private IEnumerator _Start()
    {
        yield return new WaitForSeconds(2);
        foreach (var item in allCytusPlayer)
        {
            item.Play();
        }
    }
}