using UnityEngine;

namespace Gameplay
{
    public class MapCameraController : MonoBehaviour
    {
        [SerializeField] private Camera camera;

        public Camera Camera => camera;

    }
}