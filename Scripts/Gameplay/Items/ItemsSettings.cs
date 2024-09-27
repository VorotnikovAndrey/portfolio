using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Player.Effects;
using PlayVibe.RolePopup;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Utils.Extensions;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "ItemsSettings", menuName = "SO/ItemsSettings")]
    public class ItemsSettings : ScriptableObject
    {
        [SerializeField] private List<ItemData> data;

        private Dictionary<string, ItemData> items;

        public Dictionary<string, ItemData> Data
        {
            get
            {
                items = new Dictionary<string, ItemData>();

                foreach (var element in data)
                {
                    items.TryAdd(element.Key, element);
                }

                return items;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Setup")]
        public void Setup()
        {
            data.Clear();

            var iconPaths = AssetDatabase.FindAssets("t:Texture", new[] { "Assets/Textures/Icons/Items" });
            
            foreach (var path in iconPaths)
            {
                var newItem = new ItemData
                {
                    Icon = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(path)),
                    Key = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(path))
                };

                data.Add(newItem);
            }
        }
#endif
        public string GetRandom()
        {
            return items.Keys.ToList().GetRandom();
        }
    }

    [Serializable]
    public class ItemData
    {
        public string Key;
        public ItemClassification Classification;
        public bool IsConsumable;
        [ShowIf("IsConsumable")] public List<RoleType> AvailableFor;
        [ShowIf("IsConsumable")] public List<EffectType> Effects;
        [PreviewField]
        public Sprite Icon;
    }
}