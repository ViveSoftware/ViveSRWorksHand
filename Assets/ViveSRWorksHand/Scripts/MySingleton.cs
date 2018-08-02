using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// http://wiki.unity3d.com/index.php/Singleton
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class MySingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static object _lock = new object();

    //private static List<string> instanceRec = new List<string>();

    private static int _findNullCount = 0;

    public static T Instance
    {
        get
        {
            if (_findNullCount > 1)
            {
                //Debug.LogError("[MySingleton] not found > 1 time: " + typeof(T).ToString());
                return null;
            }

            //if (applicationIsQuitting)
            //{
            //    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
            //        "' already destroyed on application quit." +
            //        " Won't create again - returning null.");
            //    return null;
            //}

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));
                    
                    if (_instance == null)
                    {
                        _findNullCount++;

                        //讓warning只印一次就好
                        //if (instanceRec.IndexOf(typeof(T).ToString()) == -1)
                        if (_findNullCount == 1)
                        {
                            Debug.LogWarning("[Singleton] FindObjectOfType not found :" + typeof(T));
                            //instanceRec.Add(typeof(T).ToString());
                        }

                        //GameObject singleton = new GameObject();
                        //_instance = singleton.AddComponent<T>();
                        //singleton.name = "(singleton) " + typeof(T).ToString();

                        //DontDestroyOnLoad(singleton);

                        //Debug.Log("[Singleton] An instance of " + typeof(T) +
                        //    " is needed in the scene, so '" + singleton +
                        //    "' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        Debug.Log("[Singleton] Using instance already created: " + typeof(T));
                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError("[Singleton] Something went really wrong " +
                                " - there should never be more than 1 singleton!" +
                                " Reopening the scene might fix it.");
                            return _instance;
                        }
                    }
                }

                return _instance;
            }
        }
    }

    //private static bool applicationIsQuitting = false;
    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    //public void OnDestroy()
    //{
    //    applicationIsQuitting = true;
    //}
    /*
    public static void CreateSingleton(string name)
    {
        if (_instance != null)
            return;
        GameObject singleton = new GameObject();
        _instance = singleton.AddComponent<T>();
        singleton.name = name;// "(singleton) " + typeof(T).ToString();
        Debug.LogWarning("[Singleton] : " + name + " , create...");
    }
    */
}