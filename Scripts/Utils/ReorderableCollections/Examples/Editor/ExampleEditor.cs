using UnityEditor;

namespace ZeroVector.Common.Reorderable.Editor {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Example))]
    public class ExampleEditor : UnityEditor.Editor {
        private ReorderableCollection list1;
        private SerializedProperty list2;
        private ReorderableCollection list3;

        void OnEnable() {
            list1 = new ReorderableCollection(serializedObject.FindProperty("list1")) {elementNameProperty = "myEnum"};

            list2 = serializedObject.FindProperty("list2");

            list3 = new ReorderableCollection(serializedObject.FindProperty("list3"));
            list3.GetElementNameCallback += GetList3ElementName;
        }

        private string GetList3ElementName(SerializedProperty element) {
            return element.propertyPath;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            //draw the list using GUILayout, you can of course specify your own position and label
            list1.DoLayoutList();

            //Caching the property is recommended
            EditorGUILayout.PropertyField(list2);

            //draw the final list, the element name is supplied through the callback defined above "GetList3ElementName"
            list3.DoLayoutList();

            //Draw without caching property
            EditorGUILayout.PropertyField(serializedObject.FindProperty("list4"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("list5"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}