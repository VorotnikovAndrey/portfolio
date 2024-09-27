using System.Collections.Generic;
using Photon.Pun;
using PlayVibe;
using UnityEngine;

namespace Utils
{
    public static class PhotonRegionExtensions
    {
        public const string DefaultRegion = "Europe";
        
        public static Dictionary<string, string> RegionCodes { get; } = new()
        {
            { "Asia", "asia" },
            { "Australia", "au" },
            { "China", "cn" },
            { "Japan", "jp" },
            { "India", "in" },
            { "South Korea", "kr" },
            { "Singapore", "sg" },
            { "Europe", "eu" },
            { "USA East", "us" },
            { "USA West", "usw" },
            { "South America", "sa" },
            { "Middle East", "ae" },
            { "South Africa", "af" }
        };

        public static string CurrentRegion => PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion;

        public static bool ConnectToRegion(string region)
        {
            RegionCodes.TryGetValue(region, out var key);
            
            if (!string.IsNullOrEmpty(key))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefs.User.PreRegion, region);
                
                PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = key;

                if (!PhotonNetwork.ConnectToRegion(key))
                {
                    Debug.Log($"Failed to connect to region: {key.AddColorTag(Color.yellow)}".AddColorTag(Color.red));
                    return false;
                }

                Debug.Log($"Successfully connected to region: {key.AddColorTag(Color.yellow)}".AddColorTag(Color.cyan));
                
                return true;
            }

            return false;
        }
    }
}