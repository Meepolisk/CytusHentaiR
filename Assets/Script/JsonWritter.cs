using UnityEngine;
using System.IO;

namespace RTool.FileIO
{
    public static class JsonFileWriter
    {
        public static void Write<T> (T data, string customName = null)
        {
            if (string.IsNullOrEmpty(customName))
                customName = typeof(T).FullName;
            string path = Path.Combine(Application.persistentDataPath, customName, "data.json");
            File.WriteAllText(path, JsonUtility.ToJson(data));
        }

        public static void Read<T> (string customName = null)
        {
            if (string.IsNullOrEmpty(customName))
                customName = typeof(T).FullName;
            

            //todo
        }
    }
}
