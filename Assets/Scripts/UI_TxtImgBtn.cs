using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Outraged
{
    public class UI_TxtImgBtn : MonoBehaviour
    {
        protected Image HiddenRaycaster;
        protected DynamicText Text;
        protected Image InnerImg;
        protected TouchBtn Button;
        protected LayoutElement layoutElement;
        protected bool RestrictText = true;
        public const int w = 400, h = 120;

        public static UI_TxtImgBtn Create(Transform parent, int fontSize = 24)
        {
            var result = Instantiate(Resources.Load<GameObject>("Prefabs/TxtImgBtn"));
            result.transform.SetParent(parent);
            result.transform.localPosition = new Vector3(0, 0, 0);
            var txtImgBtn = result.GetComponent<UI_TxtImgBtn>();
            txtImgBtn.SetFontSize(fontSize);
            var rect = txtImgBtn.GetComponent<RectTransform>();
            rect.localScale = new Vector3(1, 1, 1);
            return result.GetComponent<UI_TxtImgBtn>();
        }

        private void Awake()
        {
            Initialize();
        }
        private void Start()
        {
            Initialize();
        }

        public void SetActive(bool active) { this.gameObject.SetActive(active); }

        private bool IsInitialized = false;
        public void Initialize()
        {
            if (IsInitialized) { return; }
            IsInitialized = true;
            this.HiddenRaycaster = this.gameObject.GetChildFromName(nameof(HiddenRaycaster)).GetComponent<Image>();
            this.Text = this.gameObject.GetChildFromName(nameof(Text)).GetComponent<DynamicText>();
            this.Button = this.GetComponent<TouchBtn>();
            this.InnerImg = this.gameObject.GetChildFromName(nameof(InnerImg)).GetComponent<Image>();
        }
        public void SetTextRestriction(bool r)
        {
            RestrictText = r;
        }
        public TouchBtn GetBtn() { Initialize(); return this.Button; }
        public void Color(ColorType textColor = ColorType.Blackened, ColorType backgroundColor = ColorType.Cyan)
        {
            Initialize();
            Text.Color(textColor);
            ColorBackground(backgroundColor);
        }
        public void ColorBackground(ColorType backgroundColor = ColorType.Cyan) => InnerImg.Color(backgroundColor);
        public void SetFontSize(int fontSize)
        {
            Initialize();
            this.Text.fontSize = fontSize;
        }
        public void Resize(int x, int y)
        {
            this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(x, y);
            this.InnerImg.GetComponent<RectTransform>().sizeDelta = new Vector2(x, y);
            this.Text.GetComponent<RectTransform>().sizeDelta = new Vector2(x - 10, y);
            this.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        public void SetTextAlignment(TextAlignmentOptions alignment)
        {
            Initialize();
            this.Text.alignment = alignment;
        }
        private void SetTextInternal(int fontSize = 0, bool? bold = null)
        {
            if (bold.HasValue) { this.Text.fontStyle = bold.Value ? FontStyles.Bold : FontStyles.Normal; }
            if (fontSize > 0) { this.SetFontSize(fontSize); }
            if (RestrictText) { this.Text.Restrict(2); }
        }
        public void SetText(TextTag text, Func<string, string> onTextUpdate = null, int fontSize = 0, bool? bold = null)
        {
            Initialize();
            this.Text.SetText(text, onTextUpdate);
            SetTextInternal(fontSize, bold);
        }
        public void SetText(string text, int fontSize = 0, bool? bold = null)
        {
            Initialize();
            this.Text.Text.text = text;
            SetTextInternal(fontSize, bold);
        }
    }
}
