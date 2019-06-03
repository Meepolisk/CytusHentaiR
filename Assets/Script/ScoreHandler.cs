using RTool.Attribute;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreHandler : MonoBehaviour
{
    [Header("Component Ref")]
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private NotePlayer notePlayer;



    public void Setup(NotePlayer _notePlayer)
    {
        notePlayer = _notePlayer;
    }


}
