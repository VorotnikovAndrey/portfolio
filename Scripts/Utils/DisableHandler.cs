using System;
using UnityEngine;

namespace PlayVibe
{
    public class DisableHandler
    {
        protected Disposable _disposable;
        protected int _counter;

        public bool IsDisabled => _counter != 0;

        public virtual IDisposable Disable()
        {
            if (_counter == 0)
            {
                _disposable = new Disposable(Dispose);
            }

            _counter++;

            return _disposable;
        }

        public virtual void Clear()
        {
            _counter = 0;
        }

        protected virtual void Dispose()
        {
            if (_disposable == null)
            {
                return;
            }

            _counter--;

            if (_counter == 0)
            {
                _disposable = null;
            }
        }

        protected sealed class Disposable : IDisposable
        {
            private Action _disposeAction;

            public Disposable(Action action)
            {
                _disposeAction = action;
            }

            public void Dispose()
            {
                _disposeAction?.Invoke();
            }
        }
    }

    public sealed class DisableHandler<T> : DisableHandler where T : MonoBehaviour
    {
        private T _component;

        public DisableHandler(T value)
        {
            _component = value;
        }

        public override IDisposable Disable()
        {
            if (_counter == 0)
            {
                _disposable = new Disposable(Dispose);
                _component.enabled = false;
            }

            _counter++;

            return _disposable;
        }
        
        public override void Clear()
        {
            _counter = 0;
            _component.enabled = true;
            _disposable = null;
        }

        protected override void Dispose()
        {
            if (_disposable == null)
            {
                return;
            }

            _counter--;

            if (_counter == 0)
            {
                _component.enabled = true;
                _disposable = null;
            }
        }
    }
}