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
public abstract class SemitonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T lastUsedInstance = null;
    public static T LastInstance
    {
        get
        {
            return lastUsedInstance;
        }
    }
    protected virtual void OnEnable()
    {
        if (lastUsedInstance == null)
        {
            Debug.LogFormat(this, "{0} Semiton applied", typeof(T).Name, name);
            lastUsedInstance = (T)(ScriptableObject)this;
        }
    }
    //public static T Instance
    //{
    //    get
    //    {
    //        if (!_instance)
    //            _instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
    //        return _instance;
    //    }
    //}
}
