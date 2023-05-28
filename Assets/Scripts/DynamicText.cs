using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Outraged
{
    public class DynamicText : MonoBehaviour
    {
        public TextMeshProUGUI Text => this.GetComponent<TextMeshProUGUI>();
        public float fontSize
        {
            get { return Text.fontSize; }
            set { Text.fontSize = value; }
        }
        public FontStyles fontStyle
        {
            get { return Text.fontStyle; }
            set { Text.fontStyle = value; }
        }
        public TextAlignmentOptions alignment
        {
            get { return Text.alignment; }
            set { Text.alignment = value; }
        }
        public int TextLength => CurrentText != null ? CurrentText.Length : 0;

        private TextTag? Tag;
        private Func<string, string> OnTextUpdate;
        private Language CurrentLanguage = DataLanguages.FallbackLanguage;
        private int CharRevealMs = 0;
        private string CurrentText = null;
        private int CurrentCharRevealed = 0;
        private DateTime LastCharReveal;

        private void Update()
        {
            var newLang = Profile.GetAppLanguage();
            if (newLang != CurrentLanguage)
            {
                CurrentLanguage = newLang;
                UpdateText();
            }
            UpdateTextReveal();
        }
        private void UpdateText()
        {
            if (!Tag.HasValue) { return; }
            string txt = Tag.Value.GetText();
            if (OnTextUpdate != null) { txt = OnTextUpdate(txt); }
            SetText(txt);
        }
        private void UpdateTextReveal()
        {
            if (CurrentText == null)
            {
                return;
            }
            if (CharRevealMs == 0)
            {
                Text.text = CurrentText;
            }
            else if (CurrentCharRevealed <= CurrentText.Length && LastCharReveal.AddMilliseconds(CharRevealMs) < DateTime.UtcNow)
            {
                Text.text = $"{CurrentText.Substring(0, CurrentCharRevealed)}<color=#ffffff00>{CurrentText.Substring(CurrentCharRevealed, CurrentText.Length - CurrentCharRevealed)}</color>";
                SetLastCharReveal(DateTime.UtcNow);
                CurrentCharRevealed++;
            }
        }
        public void SetLastCharReveal(DateTime time)
        {
            LastCharReveal = time;
        }
        public void SetText(TextTag tag, Func<string, string> onTextUpdate = null, int charRevealMs = 0)
        {
            Tag = tag;
            OnTextUpdate = onTextUpdate;
            CharRevealMs = charRevealMs;
            UpdateText();
        }
        public void SetText(string text, int charRevealMs = 0)
        {
            if (CurrentText != text)
            {
                CurrentCharRevealed = 0; // Reset counter if text has updated
                CharRevealMs = charRevealMs;
                CurrentText = text;
                Text.text = "";
                UpdateTextReveal();
            }
        }
        public void Color(ColorType color)
        {
            Text.Color(color);
        }
        public void Restrict(int val = 0)
        {
            Text.Restrict(val);
        }
    }
}
