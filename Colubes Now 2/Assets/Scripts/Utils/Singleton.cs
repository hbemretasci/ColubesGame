using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;
    public static T Instance
    {
        get { return _instance; }
    }

    protected virtual void Awake()
    {
        if (_instance != null) Debug.LogError("[Singleton] Create a second instance from the Singleton class!");
            else _instance = (T) this;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}