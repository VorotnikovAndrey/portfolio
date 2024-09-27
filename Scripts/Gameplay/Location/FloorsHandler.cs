using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Network
{
    public class FloorsHandler : MonoBehaviour
    {
        [SerializeField] private int prisonerHomeFloor;
        [SerializeField] private int securityHomeFloor;
        [SerializeField] private int solitaryFloor;
        [Space]
        [SerializeField] private List<FloorsData> floors = new();

        public int ActiveFloor { get; private set; }
        public int Count => floors.Count;
        public bool IsAllActive => floors.All(element =>
        {
            if (element.Parent != null)
            {
                return element.Parent.activeSelf;
            }

            return false;

        });

        public int PrisonerHomeFloor => prisonerHomeFloor;
        public int SecurityHomeFloor => securityHomeFloor;
        public int SolitaryFloor => solitaryFloor;
        
        public void SetFloor(int index)
        {
            ActiveFloor = index;
                
            foreach (var element in floors)
            {
                if (element.Parent != null)
                {
                    element.Parent.SetActive(element.Index == index);
                }
            }
        }

        public void SetForAll(bool value)
        {
            ActiveFloor = -1;
            
            floors.ForEach(element =>
            {
                if (element.Parent != null)
                {
                    element.Parent.SetActive(value);
                }
            });
        }

        public GameObject GetFloorParent(int index)
        {
            return floors.FirstOrDefault(x => x.Index == index).Parent;
        }
    }

    [Serializable]
    public class FloorsData
    {
        public int Index;
        public GameObject Parent;
    }
}