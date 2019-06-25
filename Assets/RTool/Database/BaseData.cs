using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RTool.Database
{
    public abstract partial class ScriptableDatabase : ScriptableObject
    {
        private static bool ValidKeySyntax(string key) => (!string.IsNullOrEmpty(key) && new Regex(@"^[a-zA-Z0-9_]+$").IsMatch(key));
        private static bool ValidKeyUnique(string selectedKey, string newKey, IEnumerable<string> keyList)
        {
            List<string> listIDs = new List<string>(keyList);
            if (!string.IsNullOrEmpty(selectedKey))
                listIDs.Remove(selectedKey);

            return listIDs.Contains(newKey) == false;
        }
        private static bool CheckValidKey(string _old, string _new, IEnumerable<string> _checkList)
        {
            return ValidKeySyntax(_new) && ValidKeyUnique(_old, _new, _checkList);
        }
        private static string UniqueID(string _id, IEnumerable<string> _checkList)
        {
            List<string> checkList = new List<string>(_checkList);
            if (checkList.Contains(_id) == false)
                return _id;

            ushort count = 1;
            while (true)
            {
                string result = _id + count.ToString();
                if (checkList.Contains(result) == false)
                    return result;
                count++;
            }
        }
    }

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