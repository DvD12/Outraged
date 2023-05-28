using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Outraged
{
    public class Checkbox : MonoBehaviour
    {
        private bool IsChecked = false;
        private Action<bool> OnCheckCallback;

        private void Start()
        {
            this.GetComponent<TouchBtn>().AddOnClickListener(() => Flip());
            UpdateUI();
        }

        private void Flip()
        {
            SetValue();
            OnCheckCallback?.Invoke(IsChecked);
        }

        public bool Value() => IsChecked;
        public void SetValue(bool? check = null)
        {
            IsChecked = check.HasValue ? check.Value : !IsChecked;
            UpdateUI();
        }
        public void OnCheck(Action<bool> callback) => OnCheckCallback = callback;
        private void UpdateUI() => this.GetComponent<Image>().SetImage(IsChecked ? "CheckTrueBtn" : "CheckFalseBtn");
    }
}
