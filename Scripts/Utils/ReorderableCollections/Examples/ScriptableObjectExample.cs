using System;
using UnityEngine;

// ReSharper disable StringLiteralTypo, IdentifierTypo, InconsistentNaming
namespace ZeroVector.Common.Reorderable {
    [CreateAssetMenu(fileName = "New ScriptableObject Example", menuName = "ScriptableObject Example")]
    public class ScriptableObjectExample : ScriptableObject {

        // This will naturally not work, which is the whole point.
        [Reorderable] public Vector3 t;

        [SerializeField]
        [Reorderable(paginate = false)]
        private MyList list;

        [Space]
        [Reorderable(paginate = true, pageSize = 0, elementNameProperty = "Value")]
        public MyDict dict;

        [Serializable]
        private struct MyObject {
            #pragma warning disable 649
            public bool myBool;
            public float myValue;
            public string myString;
            #pragma warning restore 649 
        }

        [Serializable]
        private class MyList : ReorderableList<MyObject> { }

        [Serializable]
        public class MyDict : ReorderableDictionary<float, string, MyDict.KeyValuePair> {
            public override float DeduplicateKey(float duplicateKey) {
                return duplicateKey + 0.1f;
            }
            
            // ReSharper disable once RedundantNameQualifier
            [Serializable]
            public new class KeyValuePair : ReorderableDictionary<float, string, KeyValuePair>.KeyValuePair { }
        }

    }
}