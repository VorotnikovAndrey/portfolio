using UnityEngine;
using Zenject;

namespace PlayVibe
{
    public class FaceUI : MonoBehaviour
    {
        [SerializeField] private Vector3 offset;
        
        [Inject] private GameplayStage gameplayStage;

        private Camera mainCamera;

        private void Start()
        {
            Apply();
        }
        
        private void LateUpdate()
        {
            Apply();
        }

        private void Apply()
        {
            if (mainCamera == null)
            {
                mainCamera = gameplayStage.LocalGameplayData?.LocationCamera?.Camera;
                return;
            }
            
            transform.LookAt(mainCamera.transform);
            
            var rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position).eulerAngles;

            rotation.x += offset.x;
            rotation.y = offset.y;
            rotation.z = offset.z;
            
            transform.localEulerAngles = rotation;
        }
    }
}