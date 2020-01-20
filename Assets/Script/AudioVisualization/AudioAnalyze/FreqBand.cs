using UnityEngine;
using UnityEditor;
using RTool;

namespace RTool.AudioAnalyze
{
    public enum BandType
    {
        SubBass = 0,
        Bass = 1,
        LowMid = 2,
        HighMid = 3,
        Treble = 4
    }
    [System.Serializable]
    public class BandProfile
    {
        [SerializeField, ReadOnly]
        private float hertzMin = 20f;
        public float HertzMin => hertzMin;
        [SerializeField, ReadOnly]
        private float hertzMax = 60f;
        public float HertzMax => hertzMax;
        [SerializeField, Range(1f, 20f)]
        private float boost = 1f;
        public float Boost => boost;

        public BandProfile(float _min, float _max, float _boost)
        {
            hertzMin = _min; hertzMax = _max;
            boost = _boost;
        }
    }
    [System.Serializable]

    public class FreqBand
    {
        private float boost => Handler.TotalBoost * Profile.Boost;
        private float bfDownScale => Handler.BfDownScale * Handler.TotalBoost;
        private float bfSafeScale => Handler.BfSafeScale;

        private int minSampleID { get; set; }
        private int maxSampleID { get; set; }
        private int deltaSample { get; set; }

        public BandProfile Profile { get; private set; }
        public AudioPeer Handler { get; private set; }

        public FreqBand(AudioPeer _handler, BandProfile _bandProfile)
        {
            Profile = _bandProfile;
            Handler = _handler;
        }
        public void Setup(float _freqPerSample)
        {
            minSampleID = Mathf.Clamp((Mathf.CeilToInt(Profile.HertzMin / _freqPerSample) - 1), 0, (Handler.Samples.Length - 1));
            maxSampleID = Mathf.Clamp((Mathf.CeilToInt(Profile.HertzMax / _freqPerSample) - 1), minSampleID, (Handler.Samples.Length - 1));
            deltaSample = (maxSampleID - minSampleID);
        }

        public float BfValue { get; private set; }
        public float Value { get; private set; }
        public delegate void ValueUpdateDelegate(float value, float bufferedValue);
        public ValueUpdateDelegate onUpdateValue = null;
        public delegate void BeatUpdateDelegate();
        public BeatUpdateDelegate onBeat = null;

        private float lastBeatFrame { get; set; }
        private float lowestValue { get; set; }
        private float lowestFrame { get; set; }
        const float lowestFrameRange = 0.1f;
        const float beatCooldown = 0.1f;
        public void UpdateValue()
        {
            Value = 0f;
            for (int id = minSampleID; id <= maxSampleID; id++)
            {
                Value += Handler.Samples[id];
            }
            Value *= boost;
            Value /= deltaSample;
            float max = BfValue * bfSafeScale;
            BfValue = (Value >= max) ? Value : Mathf.Clamp((BfValue - (bfDownScale * Time.deltaTime)), Value, BfValue);
            if (onUpdateValue != null)
                onUpdateValue(Value, BfValue);
            if (BfValue - lowestValue > Handler.BeatThreehold && Time.time - lastBeatFrame > beatCooldown)
            {
                lowestValue = BfValue;
                lowestFrame = Time.time;
                lastBeatFrame = Time.time;
                if (onBeat != null)
                    onBeat();
            }


            //update lowestRecord
            if (lowestValue < BfValue)
            {
                lowestValue = BfValue;
                lowestFrame = Time.time;
            }
            else
            {
                if (Time.time - lowestFrame > lowestFrameRange)
                {
                    lowestValue = BfValue;
                    lowestFrame = Time.time;
                }
            }
        }

        private void DebugLog()
        {
            Debug.LogFormat("lowest: {0} at {1}", lowestValue, lowestFrame);
        }
    }
}