using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class NoteProfileStorer : SingletonBase<NoteProfileStorer>
{
    [SerializeField]
    private List<NoteProfile> noteList;

    public static void Save(List<NoteProfile> data)
    {
        Instance.noteList = data;
    }
    public static List<NoteProfile> Load()
    {
        return Instance.noteList;
    }
}
