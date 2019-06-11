using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgRhymer1 : MonoBehaviour
{
    [Header("Component Ref")]
    [SerializeField]
    private AudioPeer audioPeer;
    [SerializeField]
    private ParticleSystem Bass_Universal = null;
    [SerializeField]
    private ParticleSystem LowMidRange_Universal = null;

    [Header("Band Config")]
    [SerializeField, Range(0f, 100f)]
    private float Bass_Multiplier = 30f;
    [SerializeField, Range(0f, 100f)]
    private float LowMidRange_Multiplier = 30f;

    private void Update()
    {
        //Bass
        var BassUniversalModule = Bass_Universal.main;
        BassUniversalModule.simulationSpeed = (audioPeer.GetBand(BandType.SubBass).BfValue * Bass_Multiplier) + 1f;

        //LowMidRange
        var LowMidRangeUniversalModule = LowMidRange_Universal.main;
        LowMidRangeUniversalModule.simulationSpeed = (audioPeer.GetBand(BandType.Bass).BfValue * LowMidRange_Multiplier) + 1f;
    }
}
