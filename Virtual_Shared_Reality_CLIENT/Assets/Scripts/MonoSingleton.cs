using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/* When I use it I can't call the specific method of a singleton*/
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static readonly object ThreadLock = new object();

    private static bool isInitialized;
    private static bool isApplicationQuitting;
   
    public static T Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                Debug.LogWarning("Instance '" + typeof(T) + "' already destroyed on application quit." +
                                 " Won't create again - returning null.");
                return null;
            }

            lock (ThreadLock)
            {
                if (_instance != null)
                {
                    return _instance;
                }

                T[] objects = FindObjectsOfType<T>();

                // Throws a error if there is more than 1 monobehaviour component attached in the scene.
                if (objects.Length > 1)
                {
                    Debug.LogError("Something went really wrong - there should never be more than 1 singleton!" +
                                   " Reopening the scene might fix it.");
                    return _instance;
                }

                _instance = objects.Length > 0 ? objects[0] : FindObjectOfType<T>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(T).Name + " (Singleton)");
                    _instance = singleton.AddComponent<T>();
                    DontDestroyOnLoad(singleton);

                    isInitialized = false;

                    Debug.Log("An instance of " + typeof(T) + " is needed in the scene, so '" + singleton +
                              "' was created with DontDestroyOnLoad.");
                }

                if (isInitialized)
                {
                    return _instance;
                }

                // _instance.Initialize();
                isInitialized = true;

                return _instance;
            }
        }
        set
        {
            if(value == null)
            {
                if (isApplicationQuitting)
                {
                    Destroy(_instance);
                }
            }
        }
    }

    protected void Awake()
    {
        if (Instance) { }
        Init();
    }

    public virtual void Init() { }


    private void OnDestroy()
    {
        Debug.LogWarning("(Singleton) OnDestroy");

        _instance = null;
        isInitialized = false;
    }

    private void OnApplicationQuit()
    {
        Debug.LogWarning("(Singleton) OnApplicationQuit");

        isApplicationQuitting = true;
        _instance = null;
    }
}
