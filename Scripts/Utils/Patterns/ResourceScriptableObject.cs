using UnityEngine;

namespace Source
{
    public abstract class ResourceScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        public static string FileName => typeof(T).Name;

        private static T _asset;

        public static T Asset
        {
            get
            {
                if (!_asset)
                    _asset = Resources.Load<T>(FileName);
                return _asset;
            }
        }
    }
}