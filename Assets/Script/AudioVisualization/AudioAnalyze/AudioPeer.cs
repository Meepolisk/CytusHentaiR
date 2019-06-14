using RTool.Attribute;
using System.Collections.Generic;
using UnityEngine;

namespace RTool.AudioAnalyze
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioPeer : SingletonBase<AudioPeer>
    {
        private const int channels = 0;

        [Header("Component Ref")]
        [SerializeField]
        private AudioSource audioSource = null;
        public AudioSource AudioSource => audioSource;
        [SerializeField]
        private RhymSetting rhymSetting = null;

        public float BfDownScale => rhymSetting.BfDownScale;
        public float BfSafeScale => rhymSetting.BfSafeScale;
        public float TotalBoost => rhymSetting.TotalBoost;

        [Header("Config")]
        [SerializeField, ReadOnlyWhenPlaying]
        private FFTWindow fftWindow = FFTWindow.Blackman;
        [SerializeField, Range(8, 12)]
        private int sampleRate = 10;
        public int SamplesCount => (int)Mathf.Pow(2, sampleRate);

        [Header("Output Debug")]
        [SerializeField, ReadOnly]
        private float songFrequency;
        [SerializeField, ReadOnly]
        private float freqPerSamples;

        public float[] Samples { get; private set; }
        
        private Dictionary<BandType, FreqBand> bandDict = new Dictionary<BandType, FreqBand>();
        public FreqBand GetBand(BandType _bandType) => bandDict[_bandType];
        public int GetBandCount => bandDict.Count;
        public Dictionary<BandType, FreqBand> BandsDict => bandDict;

        protected override void Awake()
        {
            base.Awake();
            Setup();
            if (audioSource.clip != null)
                RefreshSetting();
        }
        public void Setup()
        {
            Samples = new float[SamplesCount];
            foreach (var item in rhymSetting.GetBands)
            {
                bandDict.Add(item.Key, new FreqBand(this, item.Value));
            }
        }
        public void RefreshSetting()
        {
            songFrequency = audioSource.clip.frequency;
            freqPerSamples = songFrequency / SamplesCount;
            foreach (var item in rhymSetting.GetBands)
            {
                bandDict[item.Key].Setup(freqPerSamples);
            }
        }
        private void Update()
        {
            if (audioSource.isPlaying)
            {
                GetSpectrumData();
                UpdateFrequencyBands();
            }
        }

        private void GetSpectrumData()
        {
            audioSource.GetSpectrumData(Samples, channels, fftWindow);
        }
        private void UpdateFrequencyBands()
        {
            foreach (var item in bandDict)
            {
                item.Value.UpdateValue();
            }
        }
        public static void TryRefreshSetting()
        {
            if (Instance != null)
                Instance.RefreshSetting();
        }
    }
}
