using System;
using NTC.Global.Cache;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI
{
    public class TargetInputRotate : MonoCache, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float rotationSpeed = 4f;
        [SerializeField] private float rotationForce = 0.3f;
        [SerializeField] private float rotationDump = 2f;

        public LocalEvents Events;

        private bool _isActive;
        private Vector3 _lastPosition = Vector3.zero;
        private float _shift;

        protected override void OnEnabled()
        {
            _shift = 0;
        }

        protected override void OnDisabled()
        {
            _shift = 0;
        }

        protected override void Run()
        {
            if (_isActive)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _lastPosition = Input.mousePosition;
                    _shift = 0;
                }

                if (Input.GetMouseButton(0))
                {
                    Vector3 position = Input.mousePosition;
                    if (position == _lastPosition)
                    {
                        _shift = Mathf.Lerp(_shift, 0f, rotationSpeed * Time.deltaTime * rotationDump);
                        return;
                    }
                    
                    _shift = (position.x - _lastPosition.x) * rotationForce;
                    _lastPosition = position;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _isActive = false;
                }
            }

            Events.EmitVelocity.Invoke(new Vector3(0, -_shift, 0));
            _shift = Mathf.Lerp(_shift, 0f, rotationSpeed * Time.deltaTime);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            _isActive = true;
#endif
        }

        public void OnPointerDown(PointerEventData eventData)
        {
#if UNITY_EDITOR
            _isActive = true;
#endif
        }

        public void OnPointerUp(PointerEventData eventData)
        {
#if UNITY_EDITOR
            _isActive = false;
#endif
        }

        [Serializable]
        public class LocalEvents
        {
            public UnityEvent<Vector3> EmitVelocity;
        }
    }
}
