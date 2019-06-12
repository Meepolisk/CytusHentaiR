using UnityEngine;
using RTool.Database;

namespace Database
{
    [CreateAssetMenu(menuName = "GameData/NoteData")]
    public class NoteDatabase : ScriptableDatabase<NoteDataset> { }
}
