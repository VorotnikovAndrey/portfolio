using System;

namespace Services
{
    [Serializable]
    public class PhotonPeerData
    {
        public PhotonPeerEvents Code;
        public object CustomData;
        public int Sender;
    }
}