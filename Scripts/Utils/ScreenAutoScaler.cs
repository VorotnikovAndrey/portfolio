using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe
{
    [RequireComponent(typeof(CanvasScaler))]
    public sealed class ScreenAutoScaler : MonoBehaviour
    {
        [HideInInspector] [SerializeField] private CanvasScaler canvasScaler;

        [SerializeField] private Vector2Int defaultResolution = new(1920, 1080);
        [SerializeField] private bool enable = true;

        private float currentScreenWidth;
        private float currentScreenHeight;
        
        #region RefreshDPI

        private static readonly int dpi = 350;

        private static Vector2Int screenSize;

        #endregion

       private void OnValidate()
        {
            canvasScaler = GetComponent<CanvasScaler>();
        }

        private void Start()
        {
            if (!enable)
            {
                return;
            }

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            
            UpdateResolution();
            
            currentScreenWidth = Screen.width;
            currentScreenHeight = Screen.height;
        }

        private void Update()
        {
            if (Screen.width == currentScreenWidth && Screen.height == currentScreenHeight)
            {
                return;
            }
            
            currentScreenWidth = Screen.width;
            currentScreenHeight = Screen.height;
                
            UpdateResolution();
        }

        private void UpdateResolution()
        {
            RefreshDPI();

            var factorX = (float) screenSize.x / (float) defaultResolution.x;
            var factorY = (float) screenSize.y / (float) defaultResolution.y;
            var coef = Mathf.Clamp(factorY - factorX, 0, float.MaxValue);

            canvasScaler.scaleFactor = factorY - coef;
        }
        
        private static void RefreshDPI()
        {
            if (Screen.dpi > dpi)
            {
                var value = Screen.dpi / dpi;
                screenSize = new Vector2Int((int) (Screen.width / value), (int) (Screen.height / value));
                Screen.SetResolution(screenSize.x, screenSize.y, true);
            }
            else
            {
                screenSize = new Vector2Int((int) (Screen.width), (int) (Screen.height));
            }
        }

#if UNITY_EDITOR
        public void UpdateResolutionForScreenShot()
        {
            var screen = GetMainGameViewSize();
            Screen.SetResolution((int)screen.x, (int)screen.y, true);

            var factorX = (float)screen.x / (float)defaultResolution.x;
            var factorY = (float)screen.y / (float)defaultResolution.y;
            var coef = Mathf.Clamp(factorY - factorX, 0, float.MaxValue);

            canvasScaler.scaleFactor = factorY - coef;
        }

        public static Vector2 GetMainGameViewSize()
        {
            var T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            var GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var Res = GetSizeOfMainGameView.Invoke(null, null);
            return (Vector2)Res;
        }
#endif
    }
}