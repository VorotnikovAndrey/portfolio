using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayVibe
{
    public class EventAggregator
    {
        private readonly Dictionary<Type, object> _eventWrappersDict = new Dictionary<Type, object>();

        public void Add<T>(Action<T> action) where T : AbstractBaseEvent
        {
            var wrapper = GetWrapper<T>();
            wrapper.AddListener(action);
        }

        public void Remove<T>(Action<T> action) where T : AbstractBaseEvent
        {
            var wrapper = GetWrapper<T>();
            wrapper?.RemoveListener(action);
        }

        public void SendEvent<T>(T eventData) where T : AbstractBaseEvent
        {
            var wrapper = GetWrapper<T>();
            wrapper?.Invoke(eventData);
        }
        
        public void SendEvent<T>() where T : AbstractBaseEvent
        {
            var wrapper = GetWrapper<T>();
            wrapper?.Invoke(default(T));
        }
        
        private EventWrapper<T> GetWrapper<T>() where T : AbstractBaseEvent
        {
            Type type = typeof(T);

            if (_eventWrappersDict.ContainsKey(type))
            {
                return _eventWrappersDict[type] as EventWrapper<T>;
            }

            var wrapper = new EventWrapper<T>();
            _eventWrappersDict.Add(type, wrapper);

            return wrapper;
        }
    }
}
