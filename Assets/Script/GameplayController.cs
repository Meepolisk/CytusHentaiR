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
    
    private void Awake()
    {
        btnStart.onClick.AddListener(() =>
        {
            StartCoroutine(BtnStartPressed());
        });
        btnStop.onClick.AddListener(() =>
        {
            corePlayer.Stop();
            menuController.ReturnToPreviousMenu();
        });
    }
    IEnumerator BtnStartPressed()
    {
        if (btnPlayBubble.isOn)
        {
            corePlayer.RegistFollower(bubblePlayer);
            bubblePlayer.Setup();
        }
        else if (btnPlayCytus.isOn)
        {
            corePlayer.RegistFollower(bubblePlayer);
            bubblePlayer.Setup();
        }
        else if (btnRecordMode.isOn)
        {
            corePlayer.RegistFollower(noteRecorder);
        }
        panelGame.Show();
        CorePlayer.Setup(SongSelector.CurrentSong.AudioClip);
        yield return new WaitForSeconds(1);
        corePlayer.Play();
    }
}