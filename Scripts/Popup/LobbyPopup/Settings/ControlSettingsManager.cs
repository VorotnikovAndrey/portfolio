using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace PlayVibe
{
    public class ControlSettingsManager
    {
        [Inject] private ControlSettings controlSettings;
        
        public void SaveControlSettings()
        {
            foreach (var element in controlSettings.Data)
            {
                PlayerPrefs.SetString($"{Constants.PlayerPrefs.User.ControlSettingsKey}_{element.Key}", element.Value.ToString());
            }
            
            PlayerPrefs.Save();
            
            Debug.Log("Control settings saved.".AddColorTag(Color.cyan));
        }

        public void LoadControlSettings()
        {
            var keys = controlSettings.Data.Keys.ToList();
            
            foreach (var type in keys)
            {
                var key = $"{Constants.PlayerPrefs.User.ControlSettingsKey}_{type}";
                
                if (!PlayerPrefs.HasKey(key))
                {
                    continue;
                }
                
                if (TryConvertToKeyCode(PlayerPrefs.GetString(key), out var result))
                {
                    controlSettings.Data[type] = result;
                }
            }
            
            Debug.Log("Control settings loaded.".AddColorTag(Color.cyan));
        }
        
        private bool TryConvertToKeyCode(string value, out KeyCode keyCode)
        {
            return Enum.TryParse(value.ToUpper(), out keyCode);
        }
    }
}