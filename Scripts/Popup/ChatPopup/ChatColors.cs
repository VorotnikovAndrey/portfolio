using System.Collections.Generic;
using UnityEngine;

namespace PlayVibe
{
    [CreateAssetMenu(fileName = "New ChatColors", menuName = "SO/ChatColors")]
    public class ChatColors : ScriptableObject
    {
        public List<Color> Data;
    }
}