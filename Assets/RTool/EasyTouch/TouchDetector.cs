using System.Collections.Generic;
using UnityEngine;

public class TouchDetector : MonoBehaviour
{
    [Header("Component Ref")]
    [SerializeField]
    private Camera uiCamera = null;
    [SerializeField]
    private ParticleSystem tapEffect = null;
    [SerializeField]
    private ParticleSystem holdEffect = null;

    [Header("Config")]
    [SerializeField]
    private bool haveTapeEffect = true;
    [SerializeField]
    private bool haveHoldEffect = true;

    private float xScale, yScale;
    Queue<ParticleSystem> tapEffectQueue = new Queue<ParticleSystem>();

    private void Start()
    {
        //tapEffectTime = tapEffect.main.duration + 0.1f;
        float hwRatio = (float)Screen.height / Screen.width;
        yScale = uiCamera.orthographicSize;
        xScale = uiCamera.orthographicSize / hwRatio;

        uiCamera.transform.localPosition = new Vector3(xScale, yScale, 0f);
    }

    private void OnEnable()
    {
        EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;
        EasyTouch.On_TouchDown += EasyTouch_On_TouchDown;
        EasyTouch.On_TouchUp += EasyTouch_On_TouchUp;
    }

    private void OnDisable()
    {
        EasyTouch.On_TouchStart -= EasyTouch_On_TouchStart;
        EasyTouch.On_TouchDown -= EasyTouch_On_TouchDown;
        EasyTouch.On_TouchUp -= EasyTouch_On_TouchUp;
    }

    private void EasyTouch_On_TouchStart(Gesture gesture)
    {

        if (haveHoldEffect == true)
        {
            if (gesture.fingerIndex == 0)
            {
                ParticleSystem.EmissionModule emission = holdEffect.emission;
                emission.enabled = true;
                //holdEffect.gameObject.SetActive(true);
            }
        }

        if (haveTapeEffect == false)
            return;
        //try to pop queue
        ParticleSystem poppedParticle = null;
        if (tapEffectQueue.Count > 0)
            poppedParticle = tapEffectQueue.Peek();
        if (poppedParticle == null || poppedParticle.gameObject.activeSelf == true)
            poppedParticle = Instantiate(tapEffect, transform);
        else
            poppedParticle = tapEffectQueue.Dequeue();

        poppedParticle.gameObject.SetActive(true);
        tapEffectQueue.Enqueue(poppedParticle);
        Vector2 scaledPos = gesture.NormalizedPosition();
        poppedParticle.transform.localPosition = new Vector3(scaledPos.x * xScale, scaledPos.y * yScale, 10f) * 2f;
    }

    private void EasyTouch_On_TouchDown(Gesture gesture)
    {
        if (haveHoldEffect == false)
            return;
        if (gesture.fingerIndex != 0)
            return;

        Vector2 scaledPos = gesture.NormalizedPosition();
        holdEffect.transform.localPosition = new Vector3(scaledPos.x * xScale, scaledPos.y * yScale, 10f) * 2f;
    }

    private void EasyTouch_On_TouchUp(Gesture gesture)
    {
        if (haveHoldEffect == false)
            return;
        if (gesture.fingerIndex == 0)
        {
            ParticleSystem.EmissionModule emission = holdEffect.emission;
            emission.enabled = false;
            //holdEffect.gameObject.SetActive(false);
        }
    }
}