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
    private Button btnPlayMode = null;
    
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
    private NotePlayer notePlayer = null;
    public NotePlayer NotePlayer => notePlayer;
    [SerializeField]
    private NoteRecorder noteRecorder = null;
    public NoteRecorder NoteRecorder => noteRecorder;

    public List<CytusPlayer> AllPlayers;// { get; private set; }

    private void Awake()
    {
        btnPlayMode.onClick.AddListener( () =>
        {
            AllPlayers = new List<CytusPlayer> { videoPlayer, notePlayer };
            notePlayer.gameObject.SetActive(true);
            noteRecorder.gameObject.SetActive(false);
            OpenStartMenu();
        });
        btnRecordMode.onClick.AddListener( () =>
        {
            AllPlayers = new List<CytusPlayer> { videoPlayer, noteRecorder };
            notePlayer.gameObject.SetActive(false);
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
        if (AllPlayers.Contains(notePlayer))
            notePlayer.Setup();
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