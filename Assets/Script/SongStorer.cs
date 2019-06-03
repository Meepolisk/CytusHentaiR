using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SongStorer : SingletonBase<SongStorer>
{
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
