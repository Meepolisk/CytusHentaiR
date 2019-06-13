using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class SongSelector : SingletonBase<SongSelector>
{
    [Header("Component Ref")]
    [SerializeField]
    private Image songPreview = null;
    [SerializeField]
    private Text songTittle = null;
    [SerializeField]
    private Text songBPM = null;
    [SerializeField]
    private Text songSinger = null;
    [SerializeField]
    private Text songComposer = null;
    [SerializeField]
    private Text songStepCount = null;
    
    private void Start()
    {
        SelectSong(songIndex);
    }
    public void SelectSong(int index)
    {
        songIndex = index;
        songPreview.sprite = CurrentSong.PreviewImage;
        songTittle.text = CurrentSong.Name;
        songBPM.text = CurrentSong.BPM.ToString() + "BPM";
        if (string.IsNullOrEmpty(CurrentSong.Singer) == false)
            songSinger.text = "Singer: " + CurrentSong.Singer;
        else
            songSinger.text = "";
        if (string.IsNullOrEmpty(CurrentSong.Composer) == false)
            songComposer.text = "Composer: " + CurrentSong.Composer;
        else
            songComposer.text = "";
        songStepCount.text = "Steps count: " + CurrentSong.NoteCount.ToString();
    }

    [SerializeField]
    private List<SongNoteProfile> songList = null;
    [SerializeField]
    private int songIndex = 0;
    public static int SongIndex => Instance.songIndex;
    public static SongNoteProfile CurrentSong => Instance.songList[SongIndex];

    public static void Save(List<NoteProfile> noteList)
    {
        Instance.songList[SongIndex].UpdateNote(noteList);
    }
    public static Queue<NoteProfile> LoadQueue()
    {
        return Instance.songList[SongIndex].NoteQueue;
    }
    public static List<NoteProfile> LoadList()
    {
        return Instance.songList[SongIndex].NoteList;
    }
}
