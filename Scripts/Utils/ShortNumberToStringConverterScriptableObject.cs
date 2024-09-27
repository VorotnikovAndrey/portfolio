using UnityEngine;

namespace PlayVibe
{
    [CreateAssetMenu(fileName = "ShortConverter", menuName = "Converters/ShortNumberToString")]
    public class ShortNumberToStringConverterScriptableObject : AbstractNumberToStringConverterScriptableObject
    {
        [SerializeField] private long MinimumNumberToStop = 1000;
        [SerializeField] private int MaxFractionalDigits = 2;
        [SerializeField] private string[] ClassNames = {"", "k", "M"};

        public override string Convert(long value)
        {
            return Convert((double) value);
        }

        public override string Convert(double value)
        {
            return StringUtils.NumberToShortString(value, MinimumNumberToStop, MaxFractionalDigits, customClassNames: ClassNames);
        }
    }
}