using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Defong.Utils
{
    public static class CloneUtils
    {
        public static T CloneObjectSerializable<T>(this T obj) where T : class
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, obj);
            ms.Position = 0;
            object result = bf.Deserialize(ms);
            ms.Close();
            return (T) result;
        }
    }
}