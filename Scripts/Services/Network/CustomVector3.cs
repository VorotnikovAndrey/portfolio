using System;

namespace Services
{
    [Serializable]
    public class CustomVector3
    {
        public float X;
        public float Y;
        public float Z;
        
        public CustomVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}