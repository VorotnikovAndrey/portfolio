using TMPro;
using UnityEngine.EventSystems;

namespace Popup
{
    public class CustomTMPInputField : TMP_InputField
    {
        public override void OnPointerEnter(PointerEventData eventData)
        {
            
        }
    
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                base.OnPointerClick(eventData);
            }
        }
    }
}