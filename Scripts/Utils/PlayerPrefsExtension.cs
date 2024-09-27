#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace PlayVibe
{
    public static class PlayerPrefsExtension
    {
        [MenuItem("User/Clear PlayerPrefs")]
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            
            Debug.Log("PlayerPrefs cleared".AddColorTag(Color.cyan));
        }
    }
}
#endif