using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTool.AudioAnalyze;

public class BandVisualizorTuneController : BandVisualizorBase
{
    private class LightBehaviour
    {
        private Light light { get; set; }
        private BandVisualizorTuneController controller { get; set; }
        private bool isOn { get; set; }
        private bool active { get; set; }

        public LightBehaviour (BandVisualizorTuneController _controller, Light _light)
        {
            controller = _controller;
            light = _light;
        }
        public void Trigger()
        {
            active = true;
            isOn = true;
        }
        public void InternalUpdate()
        {
            if (active == false)
                return;

            if (isOn)
            {
                light.intensity += Time.deltaTime * controller.lightUpRate;
                if (light.intensity >= controller.maxValue)
                    isOn = false;
            }
            else
            {
                light.intensity -= Time.deltaTime * controller.lightOutRate;
                if (light.intensity <= controller.minValue)
                    active = false;
            }
        }
    }

    [SerializeField]
    private float beatMin = 0.2f;
    [SerializeField]
    private float beatMax = 0.3f;
    [SerializeField]
    private Cloth fogCloth = null;
    [SerializeField]
    private List<Light> lights = null;
    private List<LightBehaviour> lightPubs { get; set; }

    [SerializeField]
    private float minValue = 1f;
    [SerializeField]
    private float maxValue = 3f;
    [SerializeField]
    private float lightUpRate = 6f;
    [SerializeField]
    private float lightOutRate = 1f;

    protected override void Start()
    {
        base.Start();
        lightPubs = new List<LightBehaviour>();

        lights.ForEach(x => {
            lightPubs.Add(new LightBehaviour(this, x));
        });
    }
    private void Update()
    {
        lightPubs.ForEach(x => { x.InternalUpdate(); });
    }
    private void TriggerLight()
    {
        lightPubs[Random.Range(0, lightPubs.Count)].Trigger();
        Debug.Log("Light");
    }

    private bool active = false;
    protected override void ValueUpdate(float Value, float bufferedValue)
    {
        if (active == false && bufferedValue > beatMax)
        {
            TriggerLight();
            active = true;
        }
        else if (active == true && bufferedValue < beatMin)
        {
            active = false;
        }
    }
}
