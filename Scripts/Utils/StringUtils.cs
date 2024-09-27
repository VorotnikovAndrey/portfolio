using System;
using System.Globalization;
using UnityEngine;

namespace PlayVibe
{
    public static class StringUtils
    {
        public static string LeftAlignFormat = "<align=left>{0}</align>";
        public static string RightAlignFormat = "<align=right>{0}</align>";

        private static readonly string[] NumberClassName = { "", "k", "M" };

        /// <summary>
        /// Used for HTML colors. (#C52806)
        /// https://htmlcolorcodes.com
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string AddColorTag(this object message, string color)
        {
            string result = $"<color=#{color.Replace("#", string.Empty)}>{message}</color>";
            return result;
        }

        public static string AddColorTag(Color color, string tag)
        {
            string result = $"<color=#{GetColorHexString(color)}>{tag}</color>";
            return result;
        }

        public static string AddColorTag(this string message, Color color)
        {
            string result = $"<color=#{GetColorHexString(color)}>{message}</color>";
            return result;
        }

        public static string AddColorTag(this object message, Color color)
        {
            string result = $"<color=#{GetColorHexString(color)}>{message}</color>";
            return result;
        }

        private static string GetColorHexString(Color color)
        {
            string colorString = string.Empty;
            colorString += ((int)(color.r * 255)).ToString("X02");
            colorString += ((int)(color.g * 255)).ToString("X02");
            colorString += ((int)(color.b * 255)).ToString("X02");
            return colorString;
        }

        public static string NumberToShortString(double digit, long min = 10000, int maxFractionalDigits = 2, int iteration = 0, string[] customClassNames = null)
        {
            while (true)
            {
                if (iteration == 0 && Math.Abs(digit) < min) return NumberToString(digit);

                bool isNegative = digit < 0;
                digit = Math.Truncate(digit * 100) / 100;
                bool isRound = digit - Math.Round(digit) == 0;

                int fractionalDigits = isRound ? 0 : maxFractionalDigits;

                string[] digitParts = digit.ToString(CultureInfo.InvariantCulture).Split('.');
                if (digitParts.Length > 1)
                {
                    fractionalDigits = Mathf.Min(maxFractionalDigits, digitParts[1].Length);
                }

                string[] suffixes = customClassNames ?? NumberClassName;
                if (isNegative && digit > -1000 || !isNegative && digit < 1000 || iteration == suffixes.Length - 1)
                {
                    if (iteration > suffixes.Length - 1) iteration = suffixes.Length - 1;
                    return NumberToString(digit, fractionalDigits) + suffixes[iteration];
                }

                digit /= 1000;
                iteration += 1;
            }
        }

        public static string NumberToString(double digit, int fractionalDigits = 0)
        {
            string fractDig = fractionalDigits == -1 ? "" : fractionalDigits.ToString();
            return digit.ToString("N" + fractDig, CultureInfo.InvariantCulture);
        }
    }
}
