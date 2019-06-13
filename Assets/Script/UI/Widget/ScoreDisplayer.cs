using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplayer : MonoBehaviour
{
    [SerializeField]
    protected Text uiText = null;

    public virtual void Refresh(int _score)
    {
        uiText.text = _score.ToString();
    }
}
