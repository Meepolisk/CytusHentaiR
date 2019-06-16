using RTool.Attribute;
using UnityEngine;

namespace RTool.AudioAnalyze
{
    public abstract class BandVisualizorBase : MonoBehaviour
    {
        [SerializeField, ReadOnlyWhenPlaying]
        protected BandType bandType = BandType.SubBass;

        public AudioPeer AudioPeer => AudioPeer.Instance;

        protected virtual void Start()
        {
            AudioPeer.BandsDict[bandType].onUpdateValue += ValueUpdate;
            AudioPeer.BandsDict[bandType].onBeat += BeatUpdate;
        }
        protected virtual void OnDestroy()
        {
            AudioPeer.BandsDict[bandType].onUpdateValue -= ValueUpdate;
            AudioPeer.BandsDict[bandType].onBeat -= BeatUpdate;
        }

        protected abstract void ValueUpdate(float Value, float bufferedValue);
        protected abstract void BeatUpdate();

        protected virtual void OnValidate()
        {
            gameObject.name = bandType.ToString();
        }
    }
}