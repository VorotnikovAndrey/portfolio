    using System;
using System.Collections.Generic;

namespace Source
{
    public class EventVariable<T> : IEventVariable<T>
    {
        private event Action<T> _onValueChanged;

        public static implicit operator T(EventVariable<T> v) => v.Value;

        public EventVariable(T defaultValue = default(T), Action<T> onValueChanged = null)
        {
            _value = defaultValue;
            PreviousValue = defaultValue;
            if (onValueChanged != null)
                AddListener(onValueChanged, false);
        }

        private T _value;
        public T Value
        {
            get { return _value; }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(value, _value))
                {
                    PreviousValue = _value;
                    _value = value;
                    if (_onValueChanged != null)
                        _onValueChanged(_value);
                }
            }
        }

        public T PreviousValue { get; private set; }

        public void AddListener(Action<T> action, bool execute = true)
        {
            _onValueChanged += action;
            if (execute)
                action(_value);
        }

        public void AddListener(ICollection<IDisposable> subs, Action<T> action, bool execute = true)
        {
            AddListener(action, execute);

            if (subs != null)
            {
                var sub = new ObservableDisposable();
                subs.Add(sub);
                sub.OnDispose += () => _onValueChanged -= action;
            }
        }

        public void RemoveListener(Action<T> action)
        {
            _onValueChanged -= action;
        }

        public void ForceEvent()
        {
            if (_onValueChanged != null)
                _onValueChanged(_value);
        }

        public void Dispose()
        {
            _onValueChanged = null;
        }
    }
}
