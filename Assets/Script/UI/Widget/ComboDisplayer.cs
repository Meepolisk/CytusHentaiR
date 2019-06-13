using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboDisplayer : ScoreDisplayer
{
    [SerializeField]
    private Animator animator = null;

    public override void Refresh(int _score)
    {
        uiText.text = _score.ToString() + " combo";
        if (_score > 5)
            animator.SetTrigger("Refresh");
    }
}
