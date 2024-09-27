//************************************************************************************************
// http://wiki.unity3d.com/index.php?title=Singleton#Generic_Based_Singleton_for_MonoBehaviours
//************************************************************************************************  

using System;
using UnityEngine;

namespace Source
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        protected static T _instance = null;
        private static object lockObject = new object();

        public static T Instance
        {
            get
            {
                // Instance requiered for the first time, we look for it
                if (_instance == null)
                {
                    lock (lockObject)
                    {
                        //Search for existing objects
                        UnityEngine.Object[] result = FindObjectsOfType(typeof(T));

                        if (result.Length > 0)
                        {
                            _instance = result[0] as T;
                            if (result.Length > 1)
                                Debug.LogError(String.Format("[MonoSingleton] Something went really wrong - there should never be more than 1 {0}! Reopening the scene might fix it. ", typeof(T).ToString()));
                        }
                        // Object not found, inform that we are trying to aquire not created instance
                        else
                        {
#if UNITY_EDITOR
                            Debug.LogWarning("[MonoSingleton] You are trying to get not created instance of " + typeof(T).ToString());
#endif
                        }
                    }
                }
                return _instance;
            }
        }
        
        public static bool AwakeDone { get; private set; }

        public static bool HasInstance => _instance != null;

        public static event Action<T> OnInstance;

        public static T CreateInstance(bool dontDestroyOnLoad = false)
        {
            if (_instance == null)
            {
                lock (lockObject)
                {
                    //Search for existing objects
                    UnityEngine.Object[] result = FindObjectsOfType(typeof(T));

                    if (result.Length > 0)
                    {
                        _instance = result[0] as T;
                        Debug.LogWarning(String.Format("[MonoSingleton]: You trying to create instance of {0}, but it is already existing!", typeof(T)), _instance.gameObject);

                        if (result.Length > 1)
                            Debug.LogError(String.Format("[MonoSingleton]: Something went really wrong - there should never be more than 1 {0}! Reopening the scene might fix it. ", typeof(T)));
                    }
                    else
                    {
                        GameObject parent = new GameObject(typeof(T).Name);
                        _instance = parent.AddComponent<T>();
                        if (dontDestroyOnLoad)
                            DontDestroyOnLoad(parent);
                    }
                }
            }
            return _instance;
        }

        public static void SubscribeOnInstance(Action<T> action)
        {
            if (HasInstance)
                action?.Invoke(Instance);
            else
                OnInstance += action;
        }

        // This function is called when the instance is used the first time
        // Put all the initializations you need here, as you would do in Awake
        protected virtual void Init()
        {
        }


        // If no other monobehaviour request the instance in an awake function
        // executing before this one, no need to search the object.
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (_instance != null)
                {
                    _instance.Init();
                    OnInstance?.Invoke(_instance);
                    OnInstance = null;
                }
            }
            else if (_instance == this as T)
            {
                _instance.Init();
                OnInstance?.Invoke(_instance);
                OnInstance = null;
            }
            else
            {
                Destroy(gameObject);
            }
            AwakeDone = true;
        }

        // Make sure the instance isn't referenced anymore when the object is destroyed, just in case.
        protected virtual void OnDestroy()
        {
            if (_instance == this as T)
            {
                _instance = null;
            }
        }

        // Make sure the instance isn't referenced anymore when the user quit, just in case.
        private void OnApplicationQuit()
        {
            _instance = null;
        }
    }
}
