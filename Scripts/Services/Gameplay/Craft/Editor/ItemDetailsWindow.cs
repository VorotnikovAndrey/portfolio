using Gameplay;
using UnityEditor;
using UnityEngine;

namespace Services.Gameplay.Craft
{
    public class ItemDetailsWindow : EditorWindow
    {
        private static CraftBank craftBank;
        private static CraftBank.CraftBankPair pair;
        private static bool isComponent;
        private static int index;
        
        private readonly Vector2 iconSize = new(100, 100);
        private readonly float iconSpacing = 10f;
        private readonly int maxColumns = 6;
        
        private Vector2 scrollPosition;
        private float windowWidth;

        public static void ShowWindow(CraftBank craftBank, CraftBank.CraftBankPair pair, bool isComponent, int index, out ItemDetailsWindow windowRef)
        {
            ItemDetailsWindow.craftBank = craftBank;
            ItemDetailsWindow.pair = pair;
            ItemDetailsWindow.isComponent = isComponent;
            ItemDetailsWindow.index = index;

            var window = GetWindow<ItemDetailsWindow>();
            window.titleContent = new GUIContent("Items");
            window.minSize = new Vector2(400, 300);
            window.maxSize = new Vector2(600, 800);
            window.Show();

            windowRef = window;
        }

        public void CloseWindow()
        {
            Close();
        }

        private void OnGUI()
        {
            CalculateWindowWidth();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (craftBank.ItemsSettings != null)
            {
                GUILayout.BeginVertical();

                var columnCount = 0;
                var i = 0;

                foreach (var element in craftBank.ItemsSettings.Data)
                {
                    var icon = craftBank.ItemsSettings.Data[element.Key].Icon;

                    if (icon == null)
                    {
                        continue;
                    }
                    
                    if (columnCount == 0)
                    {
                        GUILayout.BeginHorizontal();
                    }

                    if (i == 0 && columnCount == 0)
                    {
                        var largerTextButtonStyle = new GUIStyle(GUI.skin.button)
                        {
                            fontSize = 18
                            
                        };
                        if (GUILayout.Button("None", largerTextButtonStyle, GUILayout.Width(iconSize.x), GUILayout.Height(iconSize.y)))
                        {
                            if (!isComponent)
                            {
                                pair.ItemKey = null;
                            }
                            else
                            {
                                pair.ComponentsKeys[index] = null;
                            }
                    
                            EditorUtility.SetDirty(craftBank);

                            CloseWindow();
                        }
                        
                        GUILayout.Space(iconSpacing);

                        columnCount++;
                    }

                    i++;

                    if (GUILayout.Button(icon.texture, GUILayout.Width(iconSize.x), GUILayout.Height(iconSize.y)))
                    {
                        if (!isComponent)
                        {
                            pair.ItemKey = element.Key;
                        }
                        else
                        {
                            pair.ComponentsKeys[index] = element.Key;
                        }
                        
                        EditorUtility.SetDirty(craftBank);

                        CloseWindow();
                    }

                    columnCount++;

                    if (columnCount >= maxColumns)
                    {
                        GUILayout.EndHorizontal();
                        columnCount = 0;
                    }
                    else
                    {
                        GUILayout.Space(iconSpacing);
                    }
                }

                if (columnCount > 0)
                {
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("No items to display.");
            }

            EditorGUILayout.EndScrollView();
        }

        private void CalculateWindowWidth()
        {
            windowWidth = maxColumns * (iconSize.x + iconSpacing) + iconSpacing * 2;
            minSize = new Vector2(windowWidth, minSize.y);
            maxSize = new Vector2(windowWidth, maxSize.y);
        }
    }
}
