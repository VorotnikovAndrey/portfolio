using PlayVibe.RolePopup;
using TMPro;
using UnityEngine;

namespace PlayVibe
{
    public class ChatMessageContainer : PoolView
    {
        [SerializeField] private TextMeshProUGUI messageText;

        public void Set(string value)
        {
            messageText.text = value;
        }
    }
}