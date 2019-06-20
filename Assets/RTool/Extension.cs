using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UObject = UnityEngine.Object;
using Object = System.Object;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace RTool.Extension
{
    public static class ExtensionMethods
    {
        #region Queue/Stack simulator
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
        #endregion

        #region DeepClone
        /// <summary>
        /// Deep clone an object using System.Reflection, to return a new object which have the same value. Recommend not using this in runtime
        /// </summary>
        /// <typeparam name="T">Any type that is not primitive and is serializable</typeparam>
        /// <param name="source">The source object that will get data. Cannot be null</param>
        public static T DeepClone<T>(this T source)
        {
            if (source == null)
                throw new ArgumentNullException("Object cannot be null");
            return (T)RecursiveDeepCopy(source);
        }

        static object RecursiveDeepCopy(object source)
        {
            if (source == null)
                return null;
            Type type = source.GetType();
            if (type.IsValueType || type == typeof(string))
            {
                return source;
            }
            else if (type.IsArray)
            {
                //Type elementType = Type.GetType(
                //     type.FullName.Replace("[]", string.Empty));

                Type elementType = type.GetElementType();
                var array = source as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    copied.SetValue(RecursiveDeepCopy(array.GetValue(i)), i);
                }
                return Convert.ChangeType(copied, source.GetType());
            }
            else if (typeof(UObject).IsAssignableFrom(type))
            {
                return source;
            }
            else if (type.IsClass)
            {
                object toret = Activator.CreateInstance(source.GetType());
                FieldInfo[] fields = type.GetFields(BindingFlags.Public |
                            BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    object fieldValue = field.GetValue(source);
                    if (fieldValue == null)
                        continue;
                    field.SetValue(toret, RecursiveDeepCopy(fieldValue));
                }
                return toret;
            }
            else
                throw new ArgumentException("Unknown type");
        }
        #endregion
    }
}