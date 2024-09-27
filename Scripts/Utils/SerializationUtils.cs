using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PlayVibe
{
    public static class SerializationUtils
    {
        public static byte[] SerializeObject(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            BinaryFormatter bf = new BinaryFormatter();
            using MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        public static object DeserializeObject(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            BinaryFormatter bf = new BinaryFormatter();
            using MemoryStream ms = new MemoryStream(data);
            return bf.Deserialize(ms);
        }
    }
}