using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTool.AudioAnalyze;

public class BandVisualizorParticleSpeed : BandVisualizorBaseInjector
{
    [SerializeField]
    private ParticleSystem particle = null;

    private ParticleSystem.MainModule mainModule;

    private void Awake()
    {
        mainModule = particle.main;
    }

    protected override void InjectValue(float value)
    {
        mainModule.simulationSpeed = value;
    }
}
