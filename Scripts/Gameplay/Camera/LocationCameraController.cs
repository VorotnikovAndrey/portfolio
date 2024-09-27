using PlayVibe;
using UnityEngine;

namespace Gameplay
{
    public class LocationCameraController : PoolView
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float speed;
        [SerializeField] private Vector3 positionOffset;
        [SerializeField] private Vector3 rotationOffset;

        private Transform camTransform;
        private Transform target;

        public Camera Camera => cam;

        private void Start()
        {
            Camera.main?.gameObject.SetActive(false);
            
            camTransform = transform;
            camTransform.localEulerAngles = rotationOffset;
        }

        public void MoveTo(Vector3 position)
        {
            Stop();
            camTransform.position = position;
        }

        public void FollowTo(Transform target)
        {
            this.target = target;
        }

        public void Stop()
        {
            target = null;
        }

        public void LateUpdate()
        {
            if (target != null)
            {
                camTransform.position = Vector3.Lerp(camTransform.position, ValidatePosition(target.position), speed);
            }
        }

        private Vector3 ValidatePosition(Vector3 position)
        {
            return position + positionOffset;
        }
    }
}