using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTool.PagingCanvas;

public class GameplayController : MonoBehaviour
{
    [Header("Component Ref")]
    [SerializeField]
    private MonoCanvasesController menuController = null;
    [SerializeField]
    private MonoCanvas panelModeSelector = null;
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
    [SerializeField]
    private Button btnBack2 = null;
    [SerializeField]
    private CytusPlayer videoPlayer = null;
    public CytusPlayer VideoPlayer => videoPlayer;
    [SerializeField]
    private CytusPlayer notePlayer = null;
    public CytusPlayer NotePlayer => notePlayer;
    [SerializeField]
    private CytusPlayer noteRecorder = null;
    public CytusPlayer NoteRecorder => noteRecorder;

    public List<CytusPlayer> AllPlayers { get; private set; }

    private void Awake()
    {
        btnPlayMode.onClick.AddListener( () =>
        {
            AllPlayers = new List<CytusPlayer> { videoPlayer, notePlayer };
            OpenStartMenu();
        });
        btnRecordMode.onClick.AddListener( () =>
        {
            AllPlayers = new List<CytusPlayer> { videoPlayer, noteRecorder };
            OpenStartMenu();
        });
        btnBack.onClick.AddListener(() =>
        {
            menuController.ReturnToPreviousMenu();
        });
        btnBack2.onClick.AddListener(() =>
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
        });
    }
    private void OpenStartMenu()
    {
        panelStartGame.Show();
    }
    IEnumerator BtnStartPressed()
    {
        panelGame.Show();
        yield return new WaitForSeconds(2);
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