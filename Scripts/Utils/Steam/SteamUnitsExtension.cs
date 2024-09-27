using Steamworks;
using UnityEngine;

namespace Utils.Steam
{
    public class SteamUnitsExtension
    {
        public static Texture2D GetSteamImageAsTexture2D(int image)
        {
            var isValid = SteamUtils.GetImageSize(image, out var width, out var height);

            if (!isValid)
            {
                return null;
            }
            
            var imageData = new byte[width * height * 4];
            isValid = SteamUtils.GetImageRGBA(image, imageData, (int)(width * height * 4));

            if (!isValid)
            {
                return null;
            }
                
            var texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(imageData);
            texture.Apply();
            
            return texture;
        }
    }
}