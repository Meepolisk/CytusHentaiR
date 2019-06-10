using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RTool.UTask
{
    internal class UTaskHandler : MonoBehaviour
    {
        private static UTaskHandler _instance;
        internal static UTaskHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject(typeof(UTaskHandler).Name).AddComponent<UTaskHandler>();
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }
    }
}
