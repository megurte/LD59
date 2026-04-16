
using UnityEngine;

namespace Common.Utils
{
    public static class TextUtils
    {
        private static readonly Color DefaultColor = Color.white;
        private static readonly Color IncreasedColor = Color.green;
        private static readonly Color DecreasedColor = Color.red;
        
        public static string FormatGlitch(string s)
        {
            return $"<char>{s}</>";
        }
        
        public static Color GetChangedColorToLow(int current, int max)
        {
            if (current < max) return DecreasedColor;
            if (current > max) return IncreasedColor;
            return DefaultColor;
        }
        
        public static Color GetChangedColorToHigh(int current, int max)
        {
            if (current < max) return IncreasedColor;
            if (current > max) return DecreasedColor;
            return DefaultColor;
        }
        
        public static Color ParseHex(string hex, int alpha = 255)
        {
            if (!ColorUtility.TryParseHtmlString(hex, out var color))
            {
                Debug.LogError($"Invalid hex color: {hex}");
                return Color.magenta;
            }
            color.a = alpha / 255f;
            return color;
        }
    }
}