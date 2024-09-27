using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gameplay
{
    [CustomEditor(typeof(DropPreset))]
    public class DropPresetEditor : Editor
    {
        private DropPreset dropPreset;
        
        private readonly List<string> possibleItems = new();

        private void OnEnable()
        {
            dropPreset = (DropPreset)target;
            possibleItems.Clear();
        }

        public override void OnInspectorGUI()
        {
            try
            {
                dropPreset.ItemsSettings = (ItemsSettings)EditorGUILayout.ObjectField(
                    "Items Settings",
                    dropPreset.ItemsSettings,
                    typeof(ItemsSettings),
                    false
                );
                    
                if (dropPreset == null || dropPreset.Data == null || dropPreset.ItemsSettings == null)
                {
                    return;
                }

                if (!possibleItems.Any())
                {
                    foreach (var item in dropPreset.ItemsSettings.Data)
                    {
                        possibleItems.Add(item.Key);
                    }
                }
                
                EditorGUILayout.Space(20);
                
                var count = 0;
                
                foreach (var element in dropPreset.Data)
                {
                    EditorGUILayout.BeginHorizontal();

                    if (dropPreset.Data.Count(item => item.ItemKey == element.ItemKey) > 1)
                    {
                        GUIStyle redLabelStyle = new GUIStyle(EditorStyles.label);
                        redLabelStyle.normal.textColor = Color.red;

                        EditorGUILayout.LabelField("Duplicate", redLabelStyle, GUILayout.Width(60));
                    }

                    EditorGUILayout.LabelField($"#{count}", GUILayout.Width(30));
                    
                    count++;

                    var item = 0;

                    for (int i = 0; i < possibleItems.Count; i++)
                    {
                        if (possibleItems[i] == element.ItemKey)
                        {
                            item = i;
                            break;
                        }
                    }

                    item = EditorGUILayout.Popup(item, possibleItems.ToArray(), GUILayout.Width(200));

                    element.ItemKey = possibleItems[item];
                    
                    var texture = dropPreset.ItemsSettings.Data.FirstOrDefault(x => x.Key == element.ItemKey).Value?.Icon?.texture;

                    if (texture != null)
                    {
                        EditorGUILayout.Space(10);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUI.DrawTexture(GUILayoutUtility.GetRect(100, 100), texture, ScaleMode.ScaleToFit, true);
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        dropPreset.Data.Remove(element);
                        break;
                    }
                    
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(20);

                    EditorGUILayout.LabelField("Chance");
                    element.Chance = Mathf.Clamp(EditorGUILayout.FloatField(element.Chance), 0f, 100f);
                    
                    EditorGUILayout.LabelField("From day");
                    element.FromDay = Mathf.Clamp(EditorGUILayout.IntField(element.FromDay), 0, 10);
                    
                    EditorGUILayout.LabelField("To day");
                    element.ToDay = Mathf.Clamp(EditorGUILayout.IntField(element.ToDay), 0, 10);

                    EditorGUILayout.Space(40);
                }
            }
            catch (Exception e)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Add"))
            {
                dropPreset.Data.Add(new DropChanceData());
            }

            EditorGUILayout.EndHorizontal();

            EditorUtility.SetDirty(dropPreset);
        }
    }
}