using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Outraged
{
    public static class IOExtensions
    {
        private static Dictionary<string, Sprite> SpritesCache = new Dictionary<string, Sprite>();
        /// <summary>
        /// Enables an image and assigns it the sprite given if it can be found. Else, it disables it.
        /// </summary>
        public static bool SetImage(this Image image, string sprite_name, bool disable = false, string placeholderOnFail = "", bool resize = false)
        {
            if (disable || (string.IsNullOrEmpty(sprite_name) && string.IsNullOrEmpty(placeholderOnFail)))
            {
                image.enabled = false;
                return false;
            }
            if (image != null)
            {
                Sprite sprite = null;
                if (SpritesCache.ContainsKey(sprite_name))
                    sprite = SpritesCache[sprite_name];
                else
                    sprite = Resources.Load<Sprite>(sprite_name);
                if (sprite)
                {
                    if (!SpritesCache.ContainsKey(sprite_name))
                        SpritesCache.Add(sprite_name, sprite);
                    image.enabled = true;
                    image.sprite = sprite;
                    if (resize)
                    {
                        var x = sprite.rect.width;
                        var y = sprite.rect.height;
                        image.GetComponent<RectTransform>().sizeDelta = new Vector2(x, y);
                    }
                }
            }
            return true;
        }
        public static bool SetCountryImage(this Image image, string cID, string cName)
        {
            bool result = image.SetImage(Path.Combine("CountryFlags", cID));
            if (!result)
            {
                image.SetImage(Path.Combine("CountryFlags", "unknown"));
            }
            return result;
        }
    }
}
