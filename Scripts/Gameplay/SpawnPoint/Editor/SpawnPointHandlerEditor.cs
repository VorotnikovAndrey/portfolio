using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Player.SpawnPoint.Editor
{
    [CustomEditor(typeof(SpawnPointHandler))]
    public class SpawnPointHandlerEditor : UnityEditor.Editor
    {
        private SpawnPointHandler spawnPointHandler;

        private void OnEnable()
        {
            spawnPointHandler = target as SpawnPointHandler;
        }

        public override void OnInspectorGUI()
        {
            if (spawnPointHandler != null)
            {
                if (GUILayout.Button("Find and validate all SpawnPoint", GUILayout.Height(35f)))
                {
                    var array = FindObjectsOfType<SpawnPoint>().ToList();

                    spawnPointHandler.SetSpawnPoints(array);

                    for (var i = 0; i < array.Count; i++)
                    {
                        var element = array[i];
                        
                        element.SetId(i);
                        element.UpdatePosition();
                        element.UpdateEditorText();
                        element.UpdateMaterial();
                        
                        EditorUtility.SetDirty(array[i]);
                    }
                    
                    EditorUtility.SetDirty(spawnPointHandler);
                }
            }
            
            base.OnInspectorGUI();
        }
    }
}