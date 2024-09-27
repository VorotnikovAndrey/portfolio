#if UNITY_EDITOR
using PlayVibe;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Utils
{
    public class PropsNameValidator : MonoBehaviour
    {
        [Button("Rename")]
        public void Rename()
        {
            var array = transform.GetComponentsInChildren<Transform>();

            foreach (var element in array)
            {
                if (element == transform)
                {
                    continue;
                }
                
                var originalName = element.name;
                var spaceIndex = originalName.IndexOf(' ');

                if (spaceIndex <= 0)
                {
                    continue;
                }
                    
                var newName = originalName[..spaceIndex];

                if (newName == originalName)
                {
                    continue;
                }
                    
                Undo.RecordObject(element.gameObject, "Rename Object");

                var oldName = element.name;
                element.name = newName;
                            
                Debug.Log($"Change name {oldName.AddColorTag(Color.yellow)} to {newName.AddColorTag(Color.yellow)}".AddColorTag(Color.cyan));
                            
                EditorUtility.SetDirty(element.gameObject);
            }
        }
    }
}
#endif