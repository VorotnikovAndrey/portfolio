using UnityEngine;

namespace Utils
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance;

        public virtual void Awake()
        {
            if (Instance)
            {
                Debug.LogWarning("Duplicate subclass of type " + typeof(T) + "! eliminating " + name + " while preserving " + Instance.name);
                Destroy(gameObject);
            }
            else
            {
                Instance = this as T;
            }
        }

        public virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }  
        }
    }
}