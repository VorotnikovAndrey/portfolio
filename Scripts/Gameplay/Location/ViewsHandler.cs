using System.Collections.Generic;
using PlayVibe;
using UnityEngine;

namespace Gameplay.Network
{
    public class ViewsHandler : MonoBehaviour
    {
        public List<MapItemboxInteractiveObject> MapItemBoxes;
        public List<RecyclerInteractiveObject> Recyclers;
        public List<SecurityObserver> SecurityObservers;
    }
}