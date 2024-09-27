using System;
using PlayVibe.RolePopup;

namespace Popup
{
    [Serializable]
    public class ChatMessageEventData
    {
        public string Nickname { get; }
        public int Index { get; }
        public string Message { get; }
        public RoleType RoleType { get; }

        public ChatMessageEventData(string nickname, int index, string message, RoleType roleType)
        {
            Nickname = nickname;
            Index = index;
            Message = message;
            RoleType = roleType;
        }
    }
}