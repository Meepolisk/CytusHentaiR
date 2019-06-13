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
    private Toggle btnRecordMode = null;
    [SerializeField]
    private Toggle btnPlayBubble = null;
    [SerializeField]
    private Toggle btnPlayCytus = null;

    [SerializeField]
    private MonoCanvas panelStartGame = null;
    [SerializeField]
    private Button btnStart = null;
    
    [SerializeField]
    private MonoCanvas panelGame = null;
    [SerializeField]
    private Button btnStop = null;
    [Header("Component Ref: CORE")]
    [SerializeField]
    private AudioBGMController corePlayer = null;
    public AudioBGMController CorePlayer => corePlayer;
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
    IEnumerator BtnStartPressed()
    {
        if (btnPlayBubble.isOn)
        {
            AllPlayers = new List<CytusPlayer> { corePlayer, bubblePlayer };
            bubblePlayer.gameObject.SetActive(true);
            cytusPlayer.gameObject.SetActive(false);
            noteRecorder.gameObject.SetActive(false);
        }
        else if (btnPlayCytus.isOn)
        {
            AllPlayers = new List<CytusPlayer> { corePlayer, cytusPlayer };
            bubblePlayer.gameObject.SetActive(false);
            cytusPlayer.gameObject.SetActive(true);
            noteRecorder.gameObject.SetActive(false);
        }
        else if (btnRecordMode.isOn)
        {
            AllPlayers = new List<CytusPlayer> { corePlayer, noteRecorder };
            bubblePlayer.gameObject.SetActive(false);
            cytusPlayer.gameObject.SetActive(false);
            noteRecorder.gameObject.SetActive(true);
        }
        panelGame.Show();
        if (AllPlayers.Contains(bubblePlayer) == true)
            bubblePlayer.Setup();
        if (AllPlayers.Contains(cytusPlayer) == true)
            cytusPlayer.Setup();
        CorePlayer.Setup(SongSelector.CurrentSong.AudioClip);
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