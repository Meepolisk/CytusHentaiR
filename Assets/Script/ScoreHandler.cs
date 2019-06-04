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
    private BubbleNotePlayer notePlayer;



    public void Setup(BubbleNotePlayer _notePlayer)
    {
        notePlayer = _notePlayer;
    }


}
