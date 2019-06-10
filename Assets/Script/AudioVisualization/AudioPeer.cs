using RTool.Attribute;
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
    [SerializeField, ReadOnlyWhenPlaying]
    private float[] targetFreqBandSection = new float[]
    {
        60f,    //Sub-bass
        250f,   //Bass
        500f,   //LowMidrange
        2000f,  //Midrange
        4000,   //UpperMidrange
        6000f,  //Presence
        20000f  //Brilliance
                //OverBriliance
    };
    [Header("Output Debug")]
    [SerializeField, ReadOnly]
    private float[] samples;
    [SerializeField, ReadOnly]
    private FreqBand[] freqBands;

    [SerializeField, ReadOnly]
    private float songFrequency;
    [SerializeField, ReadOnly]
    private float freqPerSamples;

    public FreqBand[] GetFreqBands => freqBands;
    public float[] GetSamples => samples;
    private AudioSource audioSource { get; set;}

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        samples = new float[samplesCount];
        freqBands = new FreqBand[targetFreqBandSection.Length + 1];
        songFrequency = audioSource.clip.frequency;
        freqPerSamples = songFrequency / samplesCount;

        for (int i = 0; i < freqBands.Length; i++)
        {
            freqBands[i] = new FreqBand();
            freqBands[i].sampleIDs = new List<int>();
        }

        int bandTick = 0;
        int maxBandTick = targetFreqBandSection.Length -1;
        for (int i = 0; i < samplesCount; i++)
        {
            freqBands[bandTick].sampleIDs.Add(i);
            var curFreq = i * freqPerSamples;
            Debug.Log(bandTick);
            if (bandTick <= maxBandTick && curFreq > targetFreqBandSection[bandTick])
            {
                bandTick++;
            }
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
        foreach (var item in freqBands)
        {
            float average = 0f;
            foreach (var id in item.sampleIDs)
            {
                average += samples[id];
            }
            average /= item.sampleIDs.Count;
            item.value = average;
        }
    }
}
[System.Serializable]
public class FreqBand
{
    public List<int> sampleIDs = new List<int>();

    public FreqBand() { sampleIDs = new List<int>(); }

    public float value = 0;
}
