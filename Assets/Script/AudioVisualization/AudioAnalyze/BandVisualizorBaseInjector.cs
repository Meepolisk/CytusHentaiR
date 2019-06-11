using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace RTool.AudioAnalyze
{
    public abstract class BandVisualizorBaseInjector : BandVisualizorBase
    {
        [Header("Config")]
        [SerializeField]
        private float activeThreadhold = 0.25f;
        [SerializeField]
        private float baseValue = 1f;
        [SerializeField]
        private float multiflier = 1f;

        protected sealed override void ValueUpdate(float Value, float bufferedValue)
        {
            if (bufferedValue > activeThreadhold)
                InjectValue(baseValue + (bufferedValue * multiflier));
            else
                InjectValue(baseValue);
        }

        protected abstract void InjectValue(float value);
    }
}
