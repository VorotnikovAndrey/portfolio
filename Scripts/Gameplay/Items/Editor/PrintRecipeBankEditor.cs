using System;
using System.Collections.Generic;
using Gameplay.Items;
using UnityEditor;
using UnityEngine;

namespace Services.Gameplay.Craft
{
    [CustomEditor(typeof(PrintRecipeBank))]
    public class PrintRecipeBankEditor : Editor
    {
        private PrintRecipeBank bank;
        private PrintRecipeItemDetailsWindow window;
        
        private readonly Vector2 iconSize = new(100, 100);
        private readonly Vector2 textSize = new(24, 100);
        private readonly HashSet<string> itemsKeys = new();
        
        private void OnEnable()
        {
            bank = target as PrintRecipeBank;

            if (bank == null || bank.ItemsSettings == null)
            {
                return;
            }
            
            itemsKeys.Clear();
            
            foreach (var element in bank.ItemsSettings.Data)
            {
                itemsKeys.Add(element.Key);
            }
        }

        private void OnDisable()
        {
            window?.CloseWindow();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (bank == null || bank.ItemsSettings == null)
            {
                return;
            }
            
            GUILayout.Space(10);

            var itemsArray = new string[itemsKeys.Count];
            itemsKeys.CopyTo(itemsArray);

            var index = 0;
            var centeredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            };
            
            var largerTextButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18
            };

            foreach (var pair in bank.Data)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label($"#{index}", centeredStyle, GUILayout.Width(textSize.x * 2f), GUILayout.Height(textSize.y));
                
                var icon = !string.IsNullOrEmpty(pair.ItemKey) && bank.ItemsSettings.Data.TryGetValue(pair.ItemKey, out var data) ? data.Icon : null;

                if (icon != null)
                {
                    if (GUILayout.Button(icon.texture, GUILayout.Width(iconSize.x), GUILayout.Height(iconSize.y)))
                    {
                        PrintRecipeItemDetailsWindow.ShowWindow(bank, pair, false, -1, out window);
                    }
                }
                else
                {
                    if (GUILayout.Button("none", largerTextButtonStyle, GUILayout.Width(iconSize.x), GUILayout.Height(iconSize.y)))
                    {
                        PrintRecipeItemDetailsWindow.ShowWindow(bank, pair, false, -1, out window);
                    }
                }

                GUILayout.Label(" = ", centeredStyle, GUILayout.Width(textSize.x), GUILayout.Height(textSize.y));

                for (var i = 0; i < pair.ComponentsKeys.Count; i++)
                {
                    var componentKey = pair.ComponentsKeys[i];
                    var componentIcon = !string.IsNullOrEmpty(componentKey) && bank.ItemsSettings.Data.TryGetValue(componentKey, out var componentData) ? componentData.Icon : null;

                    if (componentIcon != null)
                    {
                        if (GUILayout.Button(componentIcon.texture, GUILayout.Width(iconSize.x), GUILayout.Height(iconSize.y)))
                        {
                            PrintRecipeItemDetailsWindow.ShowWindow(bank, pair, true, i, out window);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("none", largerTextButtonStyle, GUILayout.Width(iconSize.x), GUILayout.Height(iconSize.y)))
                        {
                            PrintRecipeItemDetailsWindow.ShowWindow(bank, pair, true, i, out window);
                        }
                    }

                    if (i < pair.ComponentsKeys.Count - 1)
                    {
                        GUILayout.Label(" + ", centeredStyle, GUILayout.Width(textSize.x), GUILayout.Height(textSize.y));
                    }
                }
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Remove", largerTextButtonStyle, GUILayout.Width(iconSize.x * 1.2f), GUILayout.Height(iconSize.y)))
                {
                    window?.CloseWindow();
                    bank.Data.Remove(pair);
                    return;
                }

                index++;

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Add", largerTextButtonStyle, GUILayout.Height(40)))
            {
                window?.CloseWindow();
                
                bank.Data.Add(new PrintRecipeBank.PrintRecipePair()
                {
                    ItemKey = string.Empty,
                    ComponentsKeys = new List<string>
                    {
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty
                    }
                });
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Clear", largerTextButtonStyle, GUILayout.Height(40)))
            {
                window?.CloseWindow();
                bank.Data.Clear();
            }
        }
    }
}