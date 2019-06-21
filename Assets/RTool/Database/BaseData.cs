using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTool.Database
{
    public abstract partial class ScriptableDatabase : ScriptableObject { }

    public abstract partial class ScriptableDatabase<T> : ScriptableDatabase where T : class, new()
    {
        protected class IdenticalData
        {
            public string key;
            public T value;
            
            public IdenticalData(string _key, T _value)
            {
                key = _key; value = _value;
            }
        }
        [SerializeField]
        private List<IdenticalData> data = null;

        public T this[string key]
        {
            get => data.Single(x => x.key == key).value;
            set => data.Single(x => x.key == key).value = value;
        }

        public void Add(string key, T value)
        {
            if (ContainsKey(key))
                throw new Exception("Key already exist");

            data.Add(new IdenticalData(key, value));
        }

        public void Clear()
        {
            data.Clear();
        }
        public bool ContainsKey(string key) => data.FirstOrDefault(x => x.key == key) != null;
        
        public bool Remove(string key)
        {
            try
            { 
                data.Remove(data.Single(x => x.key == key));
                return true;
            }
            catch { return false; }
        }

        public bool TryGetValue(string key, out T value)
        {
            try
            {
                value = data.Single(x => x.key == key).value;
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }
    }
    public abstract partial class LinkedScriptableDatabase<T> : ScriptableDatabase<T> where T : class, new()
    {
       
    }
}