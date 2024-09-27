using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace PlayVibe
{
    public sealed class PopupsCanvas : MonoBehaviour, IInitializable, HColor
    {
        [HideInInspector] [SerializeField] private HColorData hColorData;
        [SerializeField] private ScreenAutoScaler screenAutoScaler;
        [Space]
        [SerializeField] private List<PopupCanvasModel> models;
        
        [Inject] private DiContainer diContainer;
        
        public HColorData HColorData => hColorData;

        private void OnValidate()
        {
            hColorData.TextColor = ColorUtility.HexToColor("#8cfc03");
        }
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize()
        {
            diContainer.BindInstance(this);
        }

        public RectTransform GetCanvasTransform(PopupGroup group)
        {
            return models.FirstOrDefault(x => x.PopupGroup == group)?.RectTransform;
        }
        
        public Canvas GetCanvas(PopupGroup group)
        {
            return models.FirstOrDefault(x => x.PopupGroup == group)?.Canvas;
        }
    }
}