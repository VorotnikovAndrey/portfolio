using System;
using UnityEngine;

namespace PlayVibe.AnyKeyPressPopup
{
    [Serializable]
    public class AnyKeyPressPopupData
    {
        public Action<KeyCode> Action;
    }
}