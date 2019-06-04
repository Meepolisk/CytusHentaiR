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

        public bool IsEqual(uint _width, uint _height)
        {
            return (width == _width) && (height == _height);
        }
    }

    [Header("Component Ref")]
    [SerializeField]
    private RawImage broadcaster = null;
    [SerializeField]
    private VideoPlayer Player = null;

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
        VRes vRes = new VRes(_video.width, _video.width);
        if (textureDict.ContainsKey(vRes))
            newRT = textureDict[vRes];
        else
        {
            newRT = new RenderTexture((int)_video.width, (int)_video.height, 30);
            textureDict.Add(vRes, newRT);
        }
        broadcaster.texture = newRT;
        Player.targetTexture = newRT;
        Player.clip = _video;
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