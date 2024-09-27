using System.Collections.Generic;
using Services.Gameplay.TimeDay;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Network
{
    public class SecurityObserver : MonoBehaviour
    {
        [SerializeField] private float distance;
        [SerializeField] private List<TimeDayState> enableIn;

        public float Distance => distance;
        public List<TimeDayState> EnableIn => enableIn;
        
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Selection.activeGameObject == gameObject)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, distance);
            }
#endif
        }
    }
}