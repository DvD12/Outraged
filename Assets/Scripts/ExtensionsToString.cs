using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Outraged
{
    public static class ExtensionsToString
    {
        public static void Restrict(this TextMeshProUGUI Text, int additionalRestriction = 0)
        {
            var text = Text.text;
            if (text == null) { return; }
            var textWidth = text.GetWidth(fontSize: (int)Text.fontSize);
            var txtRect = Text.GetComponent<RectTransform>();
            var textObjWidth = txtRect.sizeDelta.x;
            int minLength = 3 + additionalRestriction;
            if (text.Length > minLength && textWidth > textObjWidth)
            {
                int charWidth = textWidth / text.Length;
                int maxCharsFit = Math.Min(text.Length, (int)(textObjWidth / charWidth));
                Text.text = text.Substring(0, Math.Max(1, maxCharsFit - minLength)) + "...";
            }
        }
        public static string GetString<T>(this IEnumerable<T> list)
        {
            string result = "";
            int i = 0;
            foreach (var val in list)
            {
                result += val.ToString();
                if (i < list.Count() - 1)
                {
                    result += ",";
                }
                i++;
            }
            return result;
        }
        public static string GetString(this Language value)
        {
            switch (value)
            {
                case Language.German: return "Deutsch";
                case Language.English: return "English";
                case Language.French: return "Français";
                case Language.Italian: return "Italiano";
                default: return value.ToString();
            }
        }
        public static bool IsValid(this string value) => !String.IsNullOrEmpty(value);
        public static string Left(this string str, int length)
        {
            return str.Substring(0, Math.Min(length, str.Length));
        }
    }
}