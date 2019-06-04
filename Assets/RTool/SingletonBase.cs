using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class SingletonBase<T> : MonoBehaviour where T : Component
{
    protected static T _instance;

    public static T Instance => _instance;

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Debug.LogWarning("Singleton got override", gameObject);
            Destroy(this);
        }
        _instance = this as T;
    }
}
[DisallowMultipleComponent]
public abstract class SingletonDontDestroy<T> : MonoBehaviour where T : Component
{
    protected static T _instance;

    public static T Instance => _instance;

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Debug.LogWarning("Singleton got override", this.gameObject);
            Destroy(gameObject);
            return;
        }
        _instance = this as T;
        DontDestroyOnLoad(_instance.gameObject);
    }
}
