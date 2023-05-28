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
    public class CardSetsMenu : MonoBehaviour
    {
        public static CardSetsMenu Instance;

        public DynamicText CardSetsMenuTitleTxt;
        public GameObject CardSetsElemsList;
        public Scrollbar CardSetsBoxScrollbar;
        public Image CardSetsBoxSlidingArrowDown;
        public Image CardSetsBoxSlidingArrowUp;
        public Image CardSetsBoxScrollbarHandleImg;
        public Image CardSetsFormBackgroundImg;
        
        public DynamicText CardSetsCardTextTxt;
        public TMP_InputField CardSetsCardTextValueTxt;
        public DynamicText CardSetsCardTextCharCountTxt;
        public DynamicText CardSetsQuestionCardTxt;
        public Checkbox CardSetsQuestionCardBtn;
        public GameObject CardSetsQuestionPicksContainer;
        public DynamicText CardSetsQuestionPicksTxt;
        public DynamicText CardSetsQuestionPicksValueTxt;
        public TouchBtn CardSetsQuestionPicksPlusBtn;
        public TouchBtn CardSetsQuestionPicksMinusBtn;
        public DynamicText CardSetsLanguageTxt;
        public TouchBtn CardSetsLanguageValueTxt;
        public DynamicText CardSetsAuthorTxt;
        public TMP_InputField CardSetsAuthorValueTxt;

        public UI_TxtImgBtn CardSetsSendBtn;
        public UI_TxtImgBtn CardSetsMenuBackBtn;

        private int Picks = 1;
        private string Language = "Any";

        private bool isInitialized = false;
        public void Initialize()
        {
            if (!isInitialized)
            {
                Instance = this;
                CardSetsMenuTitleTxt = Helpers.Find<DynamicText>(nameof(CardSetsMenuTitleTxt));
                CardSetsElemsList = Helpers.Find<GameObject>(nameof(CardSetsElemsList));
                CardSetsBoxScrollbar = Helpers.Find<Scrollbar>(nameof(CardSetsBoxScrollbar));
                CardSetsBoxSlidingArrowDown = Helpers.Find<Image>(nameof(CardSetsBoxSlidingArrowDown));
                CardSetsBoxSlidingArrowUp = Helpers.Find<Image>(nameof(CardSetsBoxSlidingArrowUp));
                CardSetsBoxScrollbarHandleImg = Helpers.Find<Image>(nameof(CardSetsBoxScrollbarHandleImg));
                CardSetsFormBackgroundImg = Helpers.Find<Image>(nameof(CardSetsFormBackgroundImg));

                CardSetsCardTextTxt = Helpers.Find<DynamicText>(nameof(CardSetsCardTextTxt));
                CardSetsCardTextValueTxt = Helpers.Find<TMP_InputField>(nameof(CardSetsCardTextValueTxt));
                CardSetsCardTextCharCountTxt = Helpers.Find<DynamicText>(nameof(CardSetsCardTextCharCountTxt));
                CardSetsQuestionCardTxt = Helpers.Find<DynamicText>(nameof(CardSetsQuestionCardTxt));
                CardSetsQuestionCardBtn = Helpers.Find<Checkbox>(nameof(CardSetsQuestionCardBtn));
                CardSetsQuestionPicksContainer = Helpers.Find<GameObject>(nameof(CardSetsQuestionPicksContainer));
                CardSetsQuestionPicksTxt = Helpers.Find<DynamicText>(nameof(CardSetsQuestionPicksTxt));
                CardSetsQuestionPicksValueTxt = Helpers.Find<DynamicText>(nameof(CardSetsQuestionPicksValueTxt));
                CardSetsQuestionPicksPlusBtn = Helpers.Find<TouchBtn>(nameof(CardSetsQuestionPicksPlusBtn));
                CardSetsQuestionPicksMinusBtn = Helpers.Find<TouchBtn>(nameof(CardSetsQuestionPicksMinusBtn));
                CardSetsLanguageTxt = Helpers.Find<DynamicText>(nameof(CardSetsLanguageTxt));
                CardSetsLanguageValueTxt = Helpers.Find<TouchBtn>(nameof(CardSetsLanguageValueTxt));
                CardSetsAuthorTxt = Helpers.Find<DynamicText>(nameof(CardSetsAuthorTxt));
                CardSetsAuthorValueTxt = Helpers.Find<TMP_InputField>(nameof(CardSetsAuthorValueTxt));

                CardSetsSendBtn = Helpers.Find<UI_TxtImgBtn>(nameof(CardSetsSendBtn));
                CardSetsMenuBackBtn = Helpers.Find<UI_TxtImgBtn>(nameof(CardSetsMenuBackBtn));
                isInitialized = true;
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            CardSetsCardTextTxt.SetText(TextTag.CardSetsCardTextTxt);
            CardSetsCardTextValueTxt.characterLimit = Card.MAX_CHARS;
            CardSetsCardTextValueTxt.OnValueChange((s) =>
            {
                UpdateUI();
            });
            CardSetsQuestionCardTxt.SetText(TextTag.CardSetsQuestionCardTxt);
            CardSetsQuestionCardBtn.OnCheck((v) => CardSetsQuestionPicksContainer.SetActive(v));
            CardSetsQuestionPicksContainer.SetActive(CardSetsQuestionCardBtn.Value());
            CardSetsQuestionPicksTxt.SetText(TextTag.CardSetsQuestionPicksTxt);
            CardSetsQuestionPicksValueTxt.SetText(this.Picks.ToString());

            Action<bool> ChangePicks = (b) =>
            {
                this.Picks = (this.Picks + (b ? 1 : -1)).Clamp(0, Card.MAX_PICKS);
                CardSetsQuestionPicksValueTxt.SetText(this.Picks.ToString());
            };
            CardSetsQuestionPicksPlusBtn.GetComponent<TouchBtn>().AddOnClickListener(() => ChangePicks(true));
            CardSetsQuestionPicksMinusBtn.GetComponent<TouchBtn>().AddOnClickListener(() => ChangePicks(false));
            CardSetsLanguageTxt.SetText(TextTag.CardSetsLanguageTxt);
            CardSetsLanguageValueTxt.AddOnClickListener(() =>
            {
                MenuHandler.Instance.UI_ChoiceSelectionDialog.gameObject.SetActive(true);
                List<UI_ChoiceSelectionDialog.ChoiceSelection> c = new List<UI_ChoiceSelectionDialog.ChoiceSelection>();
                foreach (Language choice in Enum.GetValues(typeof(Language)))
                {
                    c.Add(new UI_ChoiceSelectionDialog.ChoiceSelection(choice.ToString(), choice.GetString()));
                }
                c.Add(new UI_ChoiceSelectionDialog.ChoiceSelection("Any", TextTag.Any.GetText()));
                UI_ChoiceSelectionDialog.Instance.Activate(true, c, TextTag.Language.GetText(), 1, new Action<HashSet<string>>((choices) =>
                {
                    this.Language = choices.First();
                    CardSetsLanguageValueTxt.GetComponent<DynamicText>().SetText(this.Language);
                }));
            });
            CardSetsAuthorTxt.SetText(TextTag.CardSetsAuthorTxt);
            CardSetsAuthorValueTxt.characterLimit = Card.MAX_AUTHOR_CHARS;

            CardSetsSendBtn.SetText(TextTag.Send, (s) => s.ToUpper());
            CardSetsSendBtn.GetBtn().AddOnClickListener(() =>
            {
                if (CardSetsCardTextValueTxt.text.Length > 0)
                {
                    string title = Language + " " + "(" + (CardSetsAuthorValueTxt.text.IsValid() ? CardSetsAuthorValueTxt.text : "Anonymous") + ") " + (CardSetsQuestionCardBtn.Value() ? "Q" + Picks.ToString() : "A");
                    string body = CardSetsCardTextValueTxt.text;

                    var outcome = DataEmail.SendMail(title, body);
                    MenuHandler.Instance.UI_ResultDialog.gameObject.SetActive(true);
                    TextTag outcomeTag = TextTag.Empty;
                    string exceptionMsg = "";
                    switch (outcome)
                    {
                        case DataEmail.SendMailOutcome.Generic:
                            outcomeTag = TextTag.UnableToSendCard;
                            break;
                        case DataEmail.SendMailOutcome.OK:
                            outcomeTag = TextTag.CardSent;
                            break;
                    }
                    UI_ResultDialog.Instance.Activate(true, outcome == DataEmail.SendMailOutcome.OK ? TextTag.Success : TextTag.Failure, outcomeTag, TextTag.OK, exceptionMsg);
                }
            });

            CardSetsMenuBackBtn.SetText(TextTag.Back, (s) => s.ToUpper());
            CardSetsMenuBackBtn.GetBtn().AddOnClickListener(() =>
            {
                MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.Main);
            });
        }

        public void UpdateUI()
        {
            if (CardSetsElemsList.transform.childCount == 0)
            {
                foreach (var cardSet in DataCardSet.CardSets)
                {
                    var card = Instantiate(Resources.Load<GameObject>("Prefabs/Card")).GetComponent<Card>();
                    card.transform.SetParent(CardSetsElemsList.transform);
                    card.transform.localPosition = Vector3.zero;
                    card.transform.localScale = Vector3.one;
                    card.Data = new CardData() { Text = cardSet.Value.NameID, Picks = cardSet.Value.Cards.Count };
                    card.SetOnClickCallback(() =>
                    {
                        MenuHandler.Instance.UI_CardsDialog.gameObject.SetActive(true);
                        UI_CardsDialog.Instance.Activate(true, cardSet.Key);
                    });
                    card.SetUI(TextTag.CardsValue);
                }
            }
            CardSetsCardTextCharCountTxt.SetText((Card.MAX_CHARS - CardSetsCardTextValueTxt.text.Length).ToString() + "/" + Card.MAX_CHARS);
            CardSetsSendBtn.ColorBackground(CardSetsCardTextValueTxt.text.Length == 0 ? ColorType.Grey : ColorType.Gold);
            CardSetsLanguageValueTxt.GetComponent<DynamicText>().SetText(this.Language);
            CardSetsQuestionPicksValueTxt.SetText(this.Picks.ToString());
        }

        public void Activate(bool active)
        {
            this.gameObject.SetActive(active);
            if (active)
            {
                UpdateUI();
                CardSetsAuthorValueTxt.text = Profile.GetNickname().Left(Card.MAX_AUTHOR_CHARS); // Only do it once you enter the menu
            }
        }
    }
}
