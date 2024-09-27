using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace PlayVibe
{
    public class StateSwitcher : MonoBehaviour
    {
        [HideInInspector] public bool UseStateOnAwake;
        [HideInInspector] public string DefaultKey = "Please enter key";
        
        [SerializeField] protected List<Item> items;
        
        private void Awake()
        {
            if (UseStateOnAwake)
            {
                Set(DefaultKey);
            }
        }

        public void Set<T>(T value) where T : Enum
        {
            if (typeof(T).IsEnum)
            {
                var stringValue = value.ToString();
                Set(stringValue);
            }
            else
            {
                throw new ArgumentException("T must be an enum type.");
            }
        }
        
        public void Set(string value)
        {
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(value) && item.Key == value)
                {
                    item.Activate.Invoke();
                }
                else
                {
                    item.Deactivate.Invoke();
                }
            }
        }

        public bool HasKey(string key)
        {
            return items.Any(x => x.Key == key);
        }

        #region ConvertExtensions

        public void ConvertAndSetByString(float value) 
        {
            Set(value.ToString(CultureInfo.InvariantCulture));
        }
        
        public void ConvertAndSetByString(long value) 
        {
            Set(value.ToString(CultureInfo.InvariantCulture));
        }
        
        public void ConvertAndSetByString(double value) 
        {
            Set(value.ToString(CultureInfo.InvariantCulture));
        }
        
        public void ConvertAndSetByString(byte value) 
        {
            Set(value.ToString(CultureInfo.InvariantCulture));
        }
        
        public void ConvertAndSetByString(object value) 
        {
            Set(value.ToString());
        }

        #endregion
        
        [Serializable]
        public class Item
        {
            public string Key;
            [Space]
            public UnityEvent Activate;
            public UnityEvent Deactivate;
        }
    }
}
