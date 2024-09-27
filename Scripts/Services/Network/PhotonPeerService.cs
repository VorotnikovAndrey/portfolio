using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using UnityEngine;

namespace Services
{
    public sealed class PhotonPeerService
    {
        public const byte UniversalEventCode = 0;
        
        public PhotonPeerService()
        {
            RegisterType(typeof(PhotonPeerData), UniversalEventCode);
            
            PhotonPeer.RegisterType(typeof(ulong), (byte)'U', SerializeUInt64, DeserializeUInt64);
            PhotonPeer.RegisterType(typeof(Vector3), (byte)'V', SerializeVector3, DeserializeVector3);
        }
        
        private void RegisterType(Type type, byte code) => PhotonPeer.RegisterType(type, code, SerializationUtils.SerializeObject, SerializationUtils.DeserializeObject);

        public static void RaiseUniversalEvent(PhotonPeerEvents eventCode, object eventData, RaiseEventOptions eventOptions, SendOptions sendOptions)
        {
            PhotonNetwork.RaiseEvent(UniversalEventCode, new PhotonPeerData
            {
                Code = eventCode,
                CustomData = eventData,
                Sender = PhotonNetwork.LocalPlayer.ActorNumber
            }, eventOptions, sendOptions);
        }
        
        private static byte[] SerializeUInt64(object obj)
        {
            var value = (ulong)obj;
            var bytes = BitConverter.GetBytes(value);
            
            if (bytes.Length != 8)
            {
                throw new IndexOutOfRangeException($"Expected 8 bytes from BitConverter, but got {bytes.Length} bytes.");
            }
            
            return bytes;
        }

        private static object DeserializeUInt64(byte[] data)
        {
            if (data.Length != 8)
            {
                throw new IndexOutOfRangeException($"Expected 8 bytes to deserialize a UInt64, but got {data.Length} bytes.");
            }
            
            return BitConverter.ToUInt64(data, 0);
        }
        
        private static short SerializeVector3(StreamBuffer outStream, object data)
        {
            var v = (Vector3)data;
            var bytes = new byte[3 * 4];
            var index = 0;

            Protocol.Serialize(v.x, bytes, ref index);
            Protocol.Serialize(v.y, bytes, ref index);
            Protocol.Serialize(v.z, bytes, ref index);

            outStream.Write(bytes, 0, 3 * 4);
            
            return 3 * 4;
        }

        private static object DeserializeVector3(StreamBuffer inStream, short length)
        {
            var v = new Vector3();
            var bytes = new byte[3 * 4];
            inStream.Read(bytes, 0, 3 * 4);

            var index = 0;
            Protocol.Deserialize(out v.x, bytes, ref index);
            Protocol.Deserialize(out v.y, bytes, ref index);
            Protocol.Deserialize(out v.z, bytes, ref index);

            return v;
        }
    }
}