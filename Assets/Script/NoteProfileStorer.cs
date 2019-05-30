using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class NoteProfileStorer : SingletonBase<NoteProfileStorer>
{
    [SerializeField]
    private NoteProfile noteList;

    public static void Save(NoteProfile data)
    {
        Instance.noteList = data;
    }
    public static NoteProfile Load(NoteProfile data)
    {
        return Instance.noteList;
    }
}
