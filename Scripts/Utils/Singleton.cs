using UnityEngine;

namespace Utils
{
    public class Singleton<T> : Object where T : Object
    {
        public static T Instance;

        public Singleton()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
        }

        ~Singleton()
        {
            if (Instance == this)
            {
                Instance = null;
            }  
        }
    }
}