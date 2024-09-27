using System;
using System.Collections.Generic;

namespace PlayVibe
{
    public class EventWrapper<T> where T : AbstractBaseEvent
    {
        private List<Action<T>> _listeners = new List<Action<T>>();

        public void AddListener(Action<T> listener)
        {
            _listeners.Add(listener);
        }

        public void Invoke(T data)
        {
            foreach (var listener in _listeners.ToArray())
            {
                listener.Invoke(data);
            }
        }

        public void RemoveListener(Action<T> listener)
        {
            _listeners.Remove(listener);
        }
    }
}
