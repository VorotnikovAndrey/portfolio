using UnityEngine;
using UnityEditor;

namespace ZeroVector.Common.Reorderable.Editor {
    [CustomEditor(typeof(SurrogateTest))]
    public class SurrogateTestEditor : UnityEditor.Editor {
        private ReorderableCollection collection;
        private SerializedProperty myClassArray;

        private void OnEnable() {
            //custom list with more complex surrogate functionality

            collection = new ReorderableCollection(serializedObject.FindProperty("objects")) {
                surrogate = new ReorderableCollection.Surrogate(typeof(GameObject), AppendObject)
            };

            //myClassArray uses an auto surrogate property on the "ReorderableAttribute"
            //it's limited to only setting a property field to the dragged object reference. Still handy!

            myClassArray = serializedObject.FindProperty("myClassList");
        }

        public override void OnInspectorGUI() {
            GUILayout.Label("Drag a GameObject onto the lists. Even though the list type is not a GameObject!");

            serializedObject.Update();

            collection.DoLayoutList();
            EditorGUILayout.PropertyField(myClassArray);

            serializedObject.ApplyModifiedProperties();
        }

        private void AppendObject(SerializedProperty element, Object objectReference,
            ReorderableCollection collection) {
            //we can do more with a custom surrogate delegate :)

            element.FindPropertyRelative("gameObject").objectReferenceValue = objectReference;
            element.FindPropertyRelative("name").stringValue = objectReference.name;
        }
    }
}