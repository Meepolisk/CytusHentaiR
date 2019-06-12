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

    }

    public abstract partial class ScriptableDatabase<T> : ScriptableDatabase where T : IdenticalDataBase
    {
        [SerializeField]
        private List<T> data = new List<T>();

        private Dictionary<string, T>

        private Dictionary<string, T> Data
        {
            get
            {
                Dictionary<string, T> result = new Dictionary<string, T>();
                foreach (var item in data)
                {
                    result.Add(item.Key, item);
                }
                return result;
            }
        }
    }
}