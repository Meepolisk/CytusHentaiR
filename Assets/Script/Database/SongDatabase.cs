using UnityEngine;
using RTool.Database;

namespace Database
{
    [CreateAssetMenu(menuName = "GameData/SongData")]
    public class SongDatabase : ScriptableDatabase<SongData> { }
}
