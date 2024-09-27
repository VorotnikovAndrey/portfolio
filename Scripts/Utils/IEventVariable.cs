using System;
using System.Collections.Generic;

namespace Source
{
    public interface IEventVariable<out T> : IDisposable
    {
        T Value { get; }
        T PreviousValue { get; }
        void AddListener(Action<T> action, bool invoke = true);
        void AddListener(ICollection<IDisposable> subs, Action<T> action, bool invoke = true);
        void RemoveListener(Action<T> action);

    }
}
