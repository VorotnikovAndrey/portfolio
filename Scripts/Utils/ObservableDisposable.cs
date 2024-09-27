using System;

namespace Source
{
    public class ObservableDisposable : IDisposable
    {
        public event Action OnDispose;

        public void Dispose()
        {
            OnDispose?.Invoke();
            OnDispose = null;
        }
    }
}
