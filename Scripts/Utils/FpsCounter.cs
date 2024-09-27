using System;
using UnityEngine;

namespace Utils
{
    public class FpsCounter : MonoBehaviour
    {
        public static event Action<float> FpsUpdated;

        public static float Fps { get; private set; }
        public static float MillisecondsPerFrame { get; private set; }

        private static float deltaTime;

        private GUIStyle Style;
        private bool IsActive = true;

        private void Awake()
        {
            Style = new GUIStyle
            {
                normal =
                {
                    textColor = Color.white
                },
                fontSize = 25,
                fontStyle = FontStyle.Bold
            };
        }

        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

            MillisecondsPerFrame = deltaTime * 1000.0f;
            Fps = 1.0f / deltaTime;

            FpsUpdated?.Invoke(Fps);

            if (Input.GetKeyDown(KeyCode.F12))
            {
                IsActive = !IsActive;
            }
        }

        private void OnGUI()
        {
            if (!IsActive)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            
            float x = safeArea.x + 10;
            float y = safeArea.y + 10;

            GUI.Label(new Rect(x, y, 100, 20), ((int)Fps).ToString(), Style);
        }
    }
}
