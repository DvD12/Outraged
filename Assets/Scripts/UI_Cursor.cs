using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Outraged
{
    public class UI_Cursor
    {
        public static bool IsPressed = false;
        public static Vector2 GetCurrentCursorSize()
        {
            return Application.isMobilePlatform ? Vector2.zero : new Vector2(20, 20);
        }

    }
}
