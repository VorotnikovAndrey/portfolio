using System;
using UnityEngine;

namespace PlayVibe
{
    [Serializable]
    public class PopupCanvasModel
    {
        [SerializeField] private PopupGroup popupGroup;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Canvas canvas;

        public PopupGroup PopupGroup => popupGroup;
        public RectTransform RectTransform => rectTransform;
        public Canvas Canvas => canvas;
    }
}