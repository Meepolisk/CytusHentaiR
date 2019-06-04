using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoBGMController : CytusPlayer
{
    [System.Serializable]
    private struct VRes
    {
        public uint width;
        public uint height;

        public VRes(uint _width, uint _height)
        {
            width = _width; height = _height;
        }
    }

    [Header("Component Ref")]
    [SerializeField]
    private RectTransform rootRect = null;
    [SerializeField]
    private RawImage broadCaster = null;
    [SerializeField]
    private VideoPlayer Player = null;

    private RectTransform bcRectTrans => broadCaster.rectTransform;

    public override bool IsPlaying => Player.isPlaying;
    public override float CurrentTime => (float)Player.time;
    public bool isPause => Player.isPaused;
    public bool isPrepared => Player.isPrepared;
    public override double Duration => Player.time;

    private Dictionary<VRes, RenderTexture> textureDict { get; set; }
    private void Awake()
    {
        textureDict = new Dictionary<VRes, RenderTexture>();
    }

    public void Setup(VideoClip _video)
    {
        RenderTexture newRT;
        VRes vRes = new VRes(_video.width, _video.height);
        if (textureDict.ContainsKey(vRes))
            newRT = textureDict[vRes];
        else
        {
            newRT = new RenderTexture((int)_video.width, (int)_video.height, 30);
            textureDict.Add(vRes, newRT);
        }
        broadCaster.texture = newRT;
        Player.targetTexture = newRT;
        Player.clip = _video;

        //refresh texture ui
        float horizontal, vertical;
        GetPreferSize((float)vRes.width, (float)vRes.height, out horizontal, out vertical);
        bcRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, horizontal);
        bcRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vertical);
    }
    private void GetPreferSize(float vidW, float vidH, out float horizon, out float vertical)
    {
        Vector3[] corners = new Vector3[4];
        rootRect.GetLocalCorners(corners);
        float dtW = Vector3.Distance(corners[1], corners[2]);
        float dtH = Vector3.Distance(corners[0], corners[1]);
        float scrRatio = dtW / dtH;
        float vidRatio = vidW / vidH;

        horizon = dtW; vertical = dtH;
        if (scrRatio < vidRatio)
            vertical = dtW / vidRatio;
        else if (scrRatio > vidRatio)
            horizon = dtH * vidRatio;
        Debug.Log("Horizontal: " + horizon);
        Debug.Log("Vertical: " + vertical);
    }

    public override void Pause()
    {
        Player.Pause();
    }

    public override void Play()
    {
        Player.Play();
    }

    public override void Stop()
    {
        Player.Stop();
    }
}