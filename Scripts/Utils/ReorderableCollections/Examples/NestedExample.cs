using UnityEngine;

namespace ZeroVector.Common.Reorderable {
    public class NestedExample : MonoBehaviour {
        [Reorderable] public ExampleChildList list;

        [System.Serializable]
        public class ExampleChild {
            [Reorderable(singleLine = true)] public NestedChildList nested;
        }

        [System.Serializable]
        public class NestedChild {
            public float myValue;
        }

        [System.Serializable]
        public class NestedChildCustomDrawer {
            public bool myBool;
            public GameObject myGameObject;
        }

        [System.Serializable]
        public class ExampleChildList : ReorderableList<ExampleChild> { }

        [System.Serializable]
        public class NestedChildList : ReorderableList<NestedChildCustomDrawer> { }
    }
}