namespace PlayVibe.Photon
{
    public class TryCreateRoomEvent : AbstractBaseEvent
    {
        public string OwnerName;
        public string RoomName;
        public string RoomPassword;
        public string Region;
        public bool EnableAdminPopup;
        public bool AutoRoleBalanceEnabled;
        public string Location;
    }
}