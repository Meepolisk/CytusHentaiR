using System;
using System.Collections.Generic;
using UnityEngine;

namespace RTool.Database
{
    [System.Serializable]
    public abstract class IdenticalDataBase
    {
        //change these value as we change variable name for later reflection
        public const string KeyFieldName = "key";
        public const string NameFieldName = "name";

        [SerializeField]
        internal string key = "";
        public string Key => key;

        [SerializeField]
        internal string name = "";
        public string Name { get => name; set => name = value; }

        public abstract string FullKey { get; }
    }
    [System.Serializable]
    public abstract class IdenticalData : IdenticalDataBase
    {
        public sealed override string FullKey => key;
        
    }
    [System.Serializable]
    public abstract partial class IdenticalData<T> : IdenticalDataBase where T : IdenticalData
    {
        protected abstract T parentData { get; }
        
        public string ParentKey => parentData.Key;
        public sealed override string FullKey => parentData.Key + "." + key;
    }
    
    public abstract partial class ScriptableDatabase : ScriptableObject
    {
        [SerializeField, HideInInspector]
        protected ScriptableDatabase parentDatabase = null;

        public abstract void DeserializeToDict();
        public abstract void SerializeToList();
        protected abstract bool IsDeserialized { get; }
        internal abstract void CheckDeserialize();
    }

    public abstract partial class ScriptableDatabase<T> : ScriptableDatabase where T : IdenticalDataBase, new()
    {
        [SerializeField]
        private List<T> dataList = new List<T>();

        private Dictionary<string, T> dataDict { get; set; }

        public sealed override void DeserializeToDict()
        {
            dataDict = new Dictionary<string, T>();
            dataList.ForEach(item => { dataDict.Add(item.Key, item); });
        }
        public sealed override void SerializeToList()
        {
            if (dataDict == null)
                return;

            dataList = new List<T>(dataDict.Values);
        }
        internal sealed override void CheckDeserialize()
        {
            if (!IsDeserialized)
                DeserializeToDict();
        }
        protected sealed override bool IsDeserialized => (dataDict != null);
        public T Get(string _key)
        {
            CheckDeserialize();

            return dataDict[_key];
        }
        public void Add (string _key, T _data)
        {
            CheckDeserialize();
            if (_data.key == null)

            dataDict.Add(_key, _data);
        }

        protected virtual void Reset()
        {
            if (dataDict != null)
                dataDict = null;
            if (dataList != null)
                dataList.Clear();
        }
    }
}