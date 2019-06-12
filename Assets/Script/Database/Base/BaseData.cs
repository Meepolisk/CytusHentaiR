using System;
using System.Collections.Generic;
using UnityEngine;

namespace RTool.Database
{
    [System.Serializable]
    public abstract class IdenticalDataBase
    {
        [SerializeField]
        protected string key = "";
        [SerializeField]
        protected string name = "";
        public string Name { get => name; set => name = value; }

        public abstract string Key { get; }
    }
    [System.Serializable]
    public abstract class IdenticalData : IdenticalDataBase
    {
        public sealed override string Key => key;
        
    }
    [System.Serializable]
    public abstract partial class IdenticalData<T> : IdenticalDataBase where T : IdenticalData
    {
        protected abstract T parentData { get; }

        public string DataKey => key;
        public string ParentKey => parentData.Key;
        public sealed override string Key => parentData.Key + "." + key;
    }
    
    public abstract partial class ScriptableDatabase: ScriptableObject
    {
        [SerializeField, HideInInspector]
        protected ScriptableDatabase parentDatabase = null;

        public abstract void DeserializeToDict();
        public abstract void SerializeToList();
        internal abstract void CheckDeserialize();
    }

    public abstract partial class ScriptableDatabase<T> : ScriptableDatabase where T : IdenticalDataBase, new()
    {
        [SerializeField, HideInInspector]
        private List<T> dataList = new List<T>();

        private Dictionary<string, T> dataDict { get; set; }

        public sealed override void DeserializeToDict()
        {
            dataDict = new Dictionary<string, T>();
            foreach (var item in dataList)
            {
                dataDict.Add(item.Key, item);
            }
        }
        public sealed override void SerializeToList()
        {
            if (dataList == null || dataList.Count == 0)
                return;

            dataList = new List<T>(dataDict.Values);
        }
        internal sealed override void CheckDeserialize()
        {
            if (dataDict == null)
                DeserializeToDict();
        }
        public T Get(string _key)
        {
            CheckDeserialize();

            return dataDict[_key];
        }
        public void Add (string _key, T _data)
        {
            CheckDeserialize();

            dataDict.Add(_key, _data);
        }
    }
}