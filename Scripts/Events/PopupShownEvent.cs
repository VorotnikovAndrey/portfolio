namespace PlayVibe
{
    public class PopupShownEvent : AbstractBaseEvent
    {
        public PopupGroup Group;
        public AbstractBasePopup Popup;
    }
}