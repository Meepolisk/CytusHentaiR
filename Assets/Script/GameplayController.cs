using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTool.EzCanvas;
using RTool.Attribute;

public class GameplayController : MonoBehaviour
{
    [Header("Component Ref: UI")]
    [SerializeField]
    private MonoCanvasesController menuController = null;
    [SerializeField]
    private Button btnRecordMode = null;
    [SerializeField]
    private Button btnPlayBubble = null;
    [SerializeField]
    private Button btnPlayCytus = null;

    [SerializeField]
    private MonoCanvas panelStartGame = null;
    [SerializeField]
    private Button btnStart = null;
    [SerializeField]
    private Button btnBack = null;
    
    [SerializeField]
    private MonoCanvas panelGame = null;
    [SerializeField]
    private Button btnStop = null;
    [Header("Component Ref: CORE")]
    [SerializeField]
    private VideoBGMController videoPlayer = null;
    public VideoBGMController VideoPlayer => videoPlayer;
    [SerializeField]
    private BubbleNotePlayer bubblePlayer = null;
    public BubbleNotePlayer BubblePlayer => bubblePlayer;
    [SerializeField]
    private CytusNotePlayer cytusPlayer = null;
    public CytusNotePlayer CytusPlayer => cytusPlayer;
    [SerializeField]
    private NoteRecorder noteRecorder = null;
    public NoteRecorder NoteRecorder => noteRecorder;

    public List<CytusPlayer> AllPlayers;// { get; private set; }

    private void Awake()
    {
        btnPlayBubble.onClick.AddListener( () =>
        {
            AllPlayers = new List<CytusPlayer> { videoPlayer, bubblePlayer };
            bubblePlayer.gameObject.SetActive(true);
            cytusPlayer.gameObject.SetActive(false);
            noteRecorder.gameObject.SetActive(false);
            OpenStartMenu();
        });
        btnPlayCytus.onClick.AddListener(() =>
        {
            AllPlayers = new List<CytusPlayer> { videoPlayer, cytusPlayer };
            bubblePlayer.gameObject.SetActive(false);
            cytusPlayer.gameObject.SetActive(true);
            noteRecorder.gameObject.SetActive(false);
            OpenStartMenu();
        });
        btnRecordMode.onClick.AddListener( () =>
        {
            AllPlayers = new List<CytusPlayer> { videoPlayer, noteRecorder };
            bubblePlayer.gameObject.SetActive(false);
            cytusPlayer.gameObject.SetActive(false);
            noteRecorder.gameObject.SetActive(true);
            OpenStartMenu();
        });
        btnBack.onClick.AddListener(() =>
        {
            menuController.ReturnToPreviousMenu();
        });
        btnStart.onClick.AddListener(() =>
        {
            StartCoroutine(BtnStartPressed());
        });
        btnStop.onClick.AddListener(() =>
        {
            StopAllCytus();
            menuController.ReturnToPreviousMenu();
        });
    }
    private void OpenStartMenu()
    {
        panelStartGame.Show();
    }
    IEnumerator BtnStartPressed()
    {
        panelGame.Show();
        if (AllPlayers.Contains(bubblePlayer) == true)
            bubblePlayer.Setup();
        if (AllPlayers.Contains(cytusPlayer) == true)
            cytusPlayer.Setup();
        VideoPlayer.Setup(SongSelector.CurrentSong.VideoClip);
        yield return new WaitForSeconds(1);
        PlayAllCytus();
    }
    
    private void PlayAllCytus()
    {
        foreach (var item in AllPlayers)
        {
            item.Play();
        }
    }
    private void StopAllCytus()
    {
        foreach (var item in AllPlayers)
        {
            item.Stop();
        }
    }
}