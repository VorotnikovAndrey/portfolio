using UnityEngine;

public class CameraResizer : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;
    
    private float defaultOrthoSize;
    private float defaultAspectRatio;

    [SerializeField] private float minAspectRatio = 1.33f;
    [SerializeField] private float maxAspectRatio = 2.0f;

    private void Start()
    {
        defaultOrthoSize = gameCamera.orthographicSize;
        defaultAspectRatio = (float)Screen.width / Screen.height;

        var minWidth = Mathf.RoundToInt(Screen.height * minAspectRatio);
        var maxWidth = Mathf.RoundToInt(Screen.height * maxAspectRatio);
        
        Screen.SetResolution(Mathf.Clamp(Screen.width, minWidth, maxWidth), Screen.height, false);
    }

    private void Update()
    {
        var currentAspectRatio = (float)Screen.width / Screen.height;

        currentAspectRatio = Mathf.Clamp(currentAspectRatio, minAspectRatio, maxAspectRatio);

        gameCamera.orthographicSize = defaultOrthoSize * (currentAspectRatio / defaultAspectRatio);

        if (currentAspectRatio > defaultAspectRatio)
        {
            var viewportWidth = defaultAspectRatio / currentAspectRatio;
            gameCamera.rect = new Rect((1f - viewportWidth) / 2f, 0f, viewportWidth, 1f);
        }
        else
        {
            gameCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }
    }
}