using System.Collections.Generic;
using UnityEngine;
using RTool.Database;


[System.Serializable]
public class SongData : IdenticalData
{
    [SerializeField]
    private string gerne = "";
    public string Gerne => gerne;

    [SerializeField]
    private string composer = "";
    public string Composer => composer;

    [SerializeField]
    private float bpm = 120;
    public float BPM => bpm;
    
    [SerializeField]
    private Texture2D previewImage = null;
    public Texture2D PreviewImage => previewImage;

    [SerializeField]
    private AudioClip audioClip = null;
    public AudioClip AudioClip => audioClip;

    [SerializeField]
    private float audioDemo = 15f;
    public float AudioDemo => audioDemo;
}

[System.Serializable]
public class NoteDataset : IdenticalData<SongData>
{
    [SerializeField]
    private List<NoteProfile> noteList = new List<NoteProfile>();
    public List<NoteProfile> NoteList => new List<NoteProfile>(noteList);
    public Queue<NoteProfile> NoteQueue => new Queue<NoteProfile>(noteList);

    public int NoteCount => noteList.Count;

    protected override SongData parentData => throw new System.NotImplementedException();

    public void UpdateNote(List<NoteProfile> newNoteList)
    {
        noteList = newNoteList;
    }
}