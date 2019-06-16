using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace RTool.AudioAnalyze
{
    public sealed class BandVisualizorInjector : BandVisualizorBaseInjector
    {
        [System.Serializable]
        private class FloatInjector : UnityEvent<float> { };
        [SerializeField]
        private FloatInjector injector = null;

        protected override void InjectValue(float value)
        {
            if (injector != null && injector.GetPersistentEventCount() > 0)
                injector.Invoke(value);
        }

        protected override void BeatUpdate() { }
    }
}
