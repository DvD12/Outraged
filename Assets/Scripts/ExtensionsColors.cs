using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Outraged
{
    public enum ColorType
    {
        Gold,
        Blackened,
        Grey,
        Cyan,
        Black,
        White,
        Red,
        Crimson
    }
    public static class ExtensionsColors
    {
        public static string ToHex(this ColorType value)
        {
            string result = "";
            Color color = value.ToColor();
            result += ((byte)(color.r * 255)).ToString("X2") + ((byte)(color.g * 255)).ToString("X2") + ((byte)(color.b * 255)).ToString("X2") + ((byte)(color.a * 255)).ToString("X2");
            return result;
        }
        public static Color ToColor(this ColorType value)
        {
            switch (value)
            {
                case ColorType.Gold: return new Color(.937f, .819f, .157f);
                case ColorType.Blackened: return new Color(.1f, .1f, .1f);
                case ColorType.Grey: return new Color(.6f, .6f, .6f);
                case ColorType.Cyan: return new Color(.781f, .98f, .9375f);
                case ColorType.Black: return new Color(0, 0, 0);
                case ColorType.White: return new Color(1, 1, 1);
                case ColorType.Red: return new Color(1, 0, 0);
                case ColorType.Crimson: return new Color(.62f, .79f, .79f);
                default: return new Color(0, 0, 0);
            }
        }

        public static void Color(this Button value, ColorType colorType, bool keepAlpha = false)
        {
            value.gameObject.Color(colorType, keepAlpha);
        }

        public static void Color(this GameObject value, ColorType colorType, bool keepAlpha = false, ColorType outlineColorType = ColorType.Gold)
        {
            var valueImage = value.GetComponent<Image>();
            if (valueImage != null)
            {
                Color color = colorType.ToColor();
                if (keepAlpha)
                {
                    var alpha = valueImage.color.a;
                    valueImage.color = new Color(color.r, color.g, color.b, alpha);
                }
                else
                {
                    valueImage.color = color;
                }
            }
            var valueTxt = value.GetComponent<TextMeshProUGUI>();
            if (valueTxt != null)
            {
                Color color = colorType.ToColor();
                Color outlineColor = outlineColorType.ToColor();
                if (keepAlpha)
                {
                    var alpha = valueTxt.color.a;
                    valueTxt.color = new Color(color.r, color.g, color.b, alpha);
                    valueTxt.outlineColor = new Color(outlineColor.r, outlineColor.g, outlineColor.b, alpha);
                }
                else
                {
                    valueTxt.color = color;
                    valueTxt.outlineColor = outlineColor;
                }
            }
        }
        public static void Color(this Image value, ColorType colorType, bool keepAlpha = false)
        {
            value.gameObject.Color(colorType, keepAlpha);
        }

        public static void Color(this TextMeshProUGUI value, ColorType colorType, bool keepAlpha = false, bool keepTags = false, ColorType outlineColorType = ColorType.Gold)
        {
            value.color = colorType.ToColor();
        }
        
        public static void Color(this TMP_InputField value, ColorType colorType)
        {
            value.textComponent.gameObject.Color(colorType);
            value.placeholder.gameObject.Color(colorType);
        }
        public static void Color(this UICircle value, ColorType colorType)
        {
            value.color = colorType.ToColor();
        }
        public static string WrapColor(this string value, ColorType color)
        {
            value = "<color=#" + color.ToHex() + ">" + value + "</color>";
            return value;
        }

        public static void ColorAlpha(this Image value, float a) { if (value != null) { value.color = new Color(value.color.r, value.color.g, value.color.b, a); } }
    }
}
