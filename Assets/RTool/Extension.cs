using System.Collections.Generic;
using UnityEngine;

namespace RTool.Extension
{
    public static class ExtensionMethods
    {
        public static void Push<T>(this List<T> _list, T _object)
        {
            _list.Add(_object);
        }
        public static T PeekLast<T>(this List<T> _list) where T : class
        {
            if (_list.Count == 0)
                return null;

            return _list[_list.Count - 1];
        }
        public static T PeekFirst<T>(this List<T> _list) where T : class
        {
            if (_list.Count == 0)
                return null;

            return _list[0];
        }
        public static T Pop<T>(this List<T> _list) where T : class
        {
            if (_list.Count == 0)
                return null;
            var index = _list.Count - 1;
            var removedOne = _list[index];
            _list.RemoveAt(index);

            return removedOne;
        }
    }
}