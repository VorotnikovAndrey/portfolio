using UnityEngine;

namespace PlayVibe
{
    public abstract class AbstractNumberToStringConverterScriptableObject : ScriptableObject
    {
        public abstract string Convert(long value);

        public abstract string Convert(double value);
    }
}
