using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Outraged
{
    public class TouchBtn : Button // redacted
    {
        public void AddOnClickListener(Action clickAction, bool clearPastCallbacks = true, bool preventThisFromClear = false)
        {
            if (clearPastCallbacks) { RemoveAllOnClickListeners(); }
            if (clickAction != null) { this.onClick.AddListener(new UnityAction(() => clickAction())); }
        }
        public void RemoveAllOnClickListeners()
        {
            this.onClick.RemoveAllListeners();
        }
    }
}
