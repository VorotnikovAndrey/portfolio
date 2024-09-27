using UnityEngine;

namespace Services.ExtensionsClasses
{
    public static class PhotonVector3Extensions
    {
        public static Vector3 ToVector3(this CustomVector3 customVector)
        {
            return new Vector3(customVector.X, customVector.Y, customVector.Z);
        }
        
        public static CustomVector3 ToCustomVector3(this Vector3 vector)
        {
            return new CustomVector3(vector.x, vector.y, vector.z);
        }
    }
}