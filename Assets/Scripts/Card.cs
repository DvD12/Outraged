using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Outraged
{
    public class Card : MonoBehaviour
    {
        public const int MAX_CHARS = 150;
        public const int MAX_PICKS = 2;
        public const int MAX_AUTHOR_CHARS = 25;
        public enum CardType
        {
            Question,
            Answer
        }
        public CardData Data;
        public int CardID;

        public Image CardImg;
        public DynamicText CardTxt;
        public DynamicText CardNumberPickTxt;
        public TextMeshProUGUI CardAuthorTxt;

        private void Awake()
        {
            Initialize();
        }
        private bool isInitialized = false;
        public void Initialize()
        {
            if (!isInitialized)
            {
                CardImg = this.GetComponent<Image>();
                CardTxt = this.gameObject.GetChildFromName(nameof(CardTxt)).GetComponent<DynamicText>();
                CardNumberPickTxt = this.gameObject.GetChildFromName(nameof(CardNumberPickTxt)).GetComponent<DynamicText>();
                CardAuthorTxt = this.gameObject.GetChildFromName(nameof(CardAuthorTxt)).GetComponent<TextMeshProUGUI>();
                isInitialized = true;
            }
        }

        public bool IsQuestion => Data.Type == CardType.Question;
        public override string ToString()
        {
            return (this.Data.Type == CardType.Question ? "Q" + Data.Picks + ":" : "A: ") + Data.Text;
        }

        public void SetCard(int id, int charRevealMs = 0)
        {
            if (!DataCardSet.AllCards.ContainsKey(id)) { return; }
            CardID = id;
            Data = DataCardSet.AllCards[CardID];
            SetUI(charRevealMs: charRevealMs);
        }
        public void SetUI(TextTag customPickTxt = TextTag.Empty, int charRevealMs = 0)
        {
            Initialize();
            CardTxt.SetText(Data.Text, charRevealMs: charRevealMs);
            CardNumberPickTxt.SetText(customPickTxt != TextTag.Empty ? customPickTxt : Data.Type == CardType.Answer ? TextTag.Empty : TextTag.PickCards, (s) => s.Replace("#VALUE", Data.Picks.ToString()));
            CardImg.SetImage(Data.Type == CardType.Answer ? "CardGreen" : "CardRed");
            CardTxt.Color(Data.Type == CardType.Answer ? ColorType.Gold : ColorType.White);
            CardNumberPickTxt.Color(ColorType.White);
            CardAuthorTxt.SetText(Data.Author.IsValid() ? Data.Author.ToString().Left(MAX_AUTHOR_CHARS) : "");
            CardAuthorTxt.Color(Data.Type == CardType.Answer ? ColorType.Gold : ColorType.White);
        }
        public void ColorAlpha(float alpha) => CardImg.ColorAlpha(alpha);
        public void SetOnClickCallback(Action action)
        {
            this.GetComponent<TouchBtn>().AddOnClickListener(() => action());
        }
        public void SetActive(bool active) => this.gameObject.SetActive(active);
    }
}
