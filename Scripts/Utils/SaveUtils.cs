using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Formatting = Newtonsoft.Json.Formatting;

namespace Utils
{   
    public static class SaveUtils
    {
        public static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented
        };
        
        public static string UserModelPath => Path.Combine(Application.persistentDataPath, _userModelFileName);
        private const string _userModelFileName = "userData.data";
    }
}