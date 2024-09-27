using UnityEngine;

namespace Utils
{
    public class NicknameHolder : MonoBehaviour
    {
        [SerializeField] private Vector3 globalRotation;

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
            // Преобразуем глобальные углы в кватернион
            Quaternion targetRotation = Quaternion.Euler(globalRotation);

            // Устанавливаем глобальный поворот
            transform.rotation = targetRotation;
        }
    }
}