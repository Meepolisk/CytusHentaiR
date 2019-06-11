using RTool.Attribute;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPeer : MonoBehaviour
{
    private const int channels = 0;

    [Header("Config")]
    [SerializeField, ReadOnlyWhenPlaying]
    private FFTWindow fftWindow = FFTWindow.Blackman;
    [SerializeField, ReadOnlyWhenPlaying]
    private int samplesCount = 1024;
    [SerializeField, Range(0.1f, 1f)]
    private float bfDownScale = 0.5f;
    public float BfDownScale => bfDownScale;
    [SerializeField, Range(1f, 2f)]
    private float bfSafeScale = 1.25f;
    public float BfSafeScale => bfSafeScale;

    [Header("Output Debug")]
    [SerializeField, ReadOnly]
    private float[] samples;

    [SerializeField, ReadOnly]
    private float songFrequency;
    [SerializeField, ReadOnly]
    private float freqPerSamples;

    public float[] GetSamples => samples;
    private AudioSource audioSource { get; set;}

    public static Dictionary<BandType, BandProfile> bandProfileDict => new Dictionary<BandType, BandProfile>
    {
        {BandType.SubBass, new BandProfile(20, 60) },
        {BandType.Bass, new BandProfile(60, 200) },
        {BandType.LowMid, new BandProfile(200, 1000) },
        {BandType.HighMid, new BandProfile(1000, 5000) },
        {BandType.Treble, new BandProfile(5000, 20000) }
    };

    private Dictionary<BandType, FreqBand> bandDict = new Dictionary<BandType, FreqBand>();
    public FreqBand GetBand(BandType _bandType) => bandDict[_bandType];
    public int GetBandCount => bandDict.Count;
    public Dictionary<BandType, FreqBand> BandsDict => bandDict;

    private void Awake()
    {
        var asd = bandDict.Values;
        audioSource = GetComponent<AudioSource>();
        samples = new float[samplesCount];
        songFrequency = audioSource.clip.frequency;
        freqPerSamples = songFrequency / samplesCount;
        foreach (var item in bandProfileDict)
        {
            bandDict.Add(item.Key, new FreqBand(this, freqPerSamples, item.Value));
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
        audioSource.GetSpectrumData(samples, channels, fftWindow);
    }
    private void UpdateFrequencyBands()
    {
        foreach (var item in bandDict)
        {
            item.Value.UpdateValue();
        }
    }
}

public enum BandType
{
    SubBass = 0,
    Bass = 1,
    LowMid = 2,
    HighMid =3,
    Treble = 4
}

[System.Serializable]
public struct BandProfile
{
    public float hertzMin { get; private set; }
    public float hertzMax { get; private set; }

    public BandProfile (float _min, float _max)
    {
        hertzMin = _min; hertzMax = _max;
    }
}

[System.Serializable]
public class FreqBand
{
    private float bfDownScale => Handler.BfDownScale;
    private float bfSafeScale => Handler.BfSafeScale;

    private int minSampleID { get; set; }
    private int maxSampleID { get; set; }
    private int deltaSample { get; set; }

    public AudioPeer Handler { get; private set; }
    
    public FreqBand (AudioPeer _handler, float _freqPerSample, BandProfile _bandProfile)
    {
        var hertzMin = _bandProfile.hertzMin;
        var hertzMax = _bandProfile.hertzMax;
        Handler = _handler;
        minSampleID = Mathf.Clamp((Mathf.CeilToInt(hertzMin / _freqPerSample) - 1), 0, (_handler.GetSamples.Length - 1));
        maxSampleID = Mathf.Clamp((Mathf.CeilToInt(hertzMax / _freqPerSample) - 1), minSampleID, (_handler.GetSamples.Length - 1));
        deltaSample = (maxSampleID - minSampleID);
    }
    
    public float BfValue { get; private set; }
    public float Value { get; private set; }
    public delegate void ValueUpdateDelegate(float value, float bufferedValue);
    public ValueUpdateDelegate onUpdateValue = null;


    public void UpdateValue()
    {
        Value = 0f;
        for (int id = minSampleID; id <= maxSampleID; id++)
        {
            Value += Handler.GetSamples[id];
        }
        Value /= deltaSample;
        float max = BfValue * bfSafeScale;
        BfValue = (Value >= max) ? Value : Mathf.Clamp((BfValue - (bfDownScale * Time.deltaTime)), Value, BfValue);

        if (onUpdateValue != null)
            onUpdateValue(Value, BfValue);
    }
}
