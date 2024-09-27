using System;

namespace PlayVibe
{
    [Serializable]
    public class ConfirmPopupData
    {
        public string Message;
        public Action<bool> Action;
    }
}