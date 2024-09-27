using System;
using UnityEngine;
using Zenject;

namespace Utils
{
    public class TimeTicker : ITickable
    {
        public event Action OnTick;
        public event Action OnSecondTick;
        public event Action OnHalfSecondTick;

        private float _secondTimer = 0;
        private float _halfSecondTimer = 0;
        
        public void Tick()
        {
            OnTick?.Invoke();

            _secondTimer += Time.deltaTime;
            if (_secondTimer >= 1)
            {
                _secondTimer = 0;
                OnSecondTick?.Invoke();
            }
            
            _halfSecondTimer += Time.deltaTime;
            if (_halfSecondTimer >= 0.5f)
            {
                _halfSecondTimer = 0;
                OnHalfSecondTick?.Invoke();
            }
        }
    }
}