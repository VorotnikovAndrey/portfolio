using PlayVibe;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Popup
{
    public class ChatActivatorArea : MonoBehaviour, IPointerEnterHandler 
    {
        [SerializeField] private ChatPopup chatPopup;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            chatPopup.ShowChat();
        }
    }
}