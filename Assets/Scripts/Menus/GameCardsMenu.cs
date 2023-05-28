using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Outraged
{
    public class GameCardsMenu : MonoBehaviour
    {
        public static GameCardsMenu Instance;

        public UICircle GameCardsActionBtnClock;
        public TouchBtn GameCardsActionBtn;
        public DynamicText GameCardsActionTxt;
        public Card[] PlayerCards = new Card[GameState.MAX_CARDS_PER_PLAYER];
        public Card[] StagedCards = new Card[Card.MAX_PICKS];
        public Card QuestionCard;
        public CardPlaceholder[] Placeholders = new CardPlaceholder[Card.MAX_PICKS];

        public GameObject PlayerAnswersScrollBtnContainer;
        public TouchBtn PlayerAnswersScrollUpBtn;
        public DynamicText PlayerAnswersScrollTxt;
        public TouchBtn PlayerAnswersScrollDownBtn;

        public TouchBtn DealNewCardsBtn;

        public GameObject ReadingQuestionHints;
        public DynamicText ReadingQuestionHints1Txt;
        public DynamicText ReadingQuestionHints2Txt;
        public GameObject ReadingAnswerHints;
        public DynamicText ReadingAnswerHints1Txt;
        public DynamicText ReadingAnswerHints2Txt;
        public bool MyFirstTimeAsCardDealer = true;

        private bool IsInitialized = false;
        public void Initialize()
        {
            if (!IsInitialized)
            {
                GameCardsActionBtnClock = Helpers.Find<UICircle>(nameof(GameCardsActionBtnClock));
                GameCardsActionBtn = Helpers.Find<TouchBtn>(nameof(GameCardsActionBtn));
                GameCardsActionTxt = Helpers.Find<DynamicText>(nameof(GameCardsActionTxt));
                for (int i = 0; i < GameState.MAX_CARDS_PER_PLAYER; i++)
                {
                    PlayerCards[i] = Helpers.Find<Card>("BottomCard" + (i + 1).ToString());
                }
                QuestionCard = Helpers.Find<Card>("TopCard1");
                for (int i = 0; i < Card.MAX_PICKS; i++)
                {
                    Placeholders[i] = Helpers.Find<CardPlaceholder>("CardPlaceholder" + (i + 1).ToString());
                    StagedCards[i] = Helpers.Find<Card>("TopCard" + (i + 2).ToString());
                }
                PlayerAnswersScrollBtnContainer = Helpers.Find<GameObject>(nameof(PlayerAnswersScrollBtnContainer));
                PlayerAnswersScrollUpBtn = Helpers.Find<TouchBtn>(nameof(PlayerAnswersScrollUpBtn));
                PlayerAnswersScrollTxt = Helpers.Find<DynamicText>(nameof(PlayerAnswersScrollTxt));
                PlayerAnswersScrollDownBtn = Helpers.Find<TouchBtn>(nameof(PlayerAnswersScrollDownBtn));

                DealNewCardsBtn = Helpers.Find<TouchBtn>(nameof(DealNewCardsBtn));
                DealNewCardsBtn.AddOnClickListener(() =>
                {
                    MenuHandler.Instance.UI_WarningDialog.gameObject.SetActive(true);
                    UI_WarningDialog.Instance.Activate(true, TextTag.ConfirmDealNewCards, TextTag.ConfirmDealNewCardsDesc, () =>
                    {
                        PunInterface.Instance.ExtrDealNewAnswers(GameState.Instance.GetLocalPlayerID());
                        UpdateUI();
                    }, TextTag.Yes, TextTag.No, s => s.Replace("#VALUE", Math.Abs(GameState.Instance.Rules.CardReplacementPointPenalty).ToString().WrapColor(ColorType.Red)).Replace("#POINTORPOINTS", Math.Abs(GameState.Instance.Rules.CardReplacementPointPenalty) == 1 ? TextTag.Point.GetText() : TextTag.Points.GetText()));
                });

                ReadingQuestionHints = Helpers.Find<GameObject>(nameof(ReadingQuestionHints));
                ReadingQuestionHints1Txt = Helpers.Find<DynamicText>(nameof(ReadingQuestionHints1Txt));
                ReadingQuestionHints2Txt = Helpers.Find<DynamicText>(nameof(ReadingQuestionHints2Txt));
                ReadingAnswerHints = Helpers.Find<GameObject>(nameof(ReadingAnswerHints));
                ReadingAnswerHints1Txt = Helpers.Find<DynamicText>(nameof(ReadingAnswerHints1Txt));
                ReadingAnswerHints2Txt = Helpers.Find<DynamicText>(nameof(ReadingAnswerHints2Txt));
                IsInitialized = true;

                GameCardsActionBtn.AddOnClickListener(() => ActionBtnCallback(false));
            }
        }
        public void ActionBtnCallback(bool forceSelection)
        {
            switch (GameState.Instance.CurrentState)
            {
                case GameProgress.ReadingQuestion:
                    if (GameState.Instance.IsLocalPlayerDealer())
                    {
                        PunInterface.Instance.ExtrSetGameProgress(GameProgress.SubmittingCards);
                    }
                    break;
                case GameProgress.SubmittingCards:
                    if (!GameState.Instance.IsLocalPlayerDealer()
                    && ( forceSelection || (GameState.Instance.StagedCardsPerPlayer[GameState.Instance.GetLocalPlayerID()].Count == DataCardSet.AllCards[GameState.Instance.CurrentQuestion].Picks
                        && !GameState.Instance.PlayersConfirmed.Contains(GameState.Instance.GetLocalPlayerID()))
                        ))
                    {
                        PunInterface.Instance.ExtrSetPlayerConfirmation(GameState.Instance.GetLocalPlayerID(), GameState.Instance.StagedCardsPerPlayer[GameState.Instance.GetLocalPlayerID()].ToArray());
                        UpdateActionBtnUI();
                    }
                    break;
                case GameProgress.ReadingAnswers:
                    if (GameState.Instance.IsLocalPlayerDealer())
                    {
                        var currentIndex = GameState.Instance.PlayerAnswersToReveal.indexToDealer;
                        PunInterface.Instance.ExtrSetAnswersShownIndex(currentIndex, currentIndex + 1); // Show whatever the dealer is reading to everyone else; if we've reached the end, move on to AwardingPoint state
                    }
                    break;
                case GameProgress.AwardingPoint:
                    if (GameState.Instance.IsLocalPlayerDealer())
                    {
                        PunInterface.Instance.ExtrAlterPlayerPoints(GameState.Instance.PlayerAnswersToReveal.players[GameState.Instance.PlayerAnswersToReveal.indexToDealer], 1);
                    }
                    break;
            }
        }
        public void UpdateActionBtnUI()
        {
            TextTag text = TextTag.Waiting;
            ColorType color = ColorType.Grey;
            int cardsLeftToSelect = 0;
            switch (GameState.Instance.CurrentState)
            {
                case GameProgress.ReadingQuestion:
                    if (GameState.Instance.IsLocalPlayerDealer())
                    {
                        text = TextTag.Reveal;
                        color = ColorType.Cyan;
                    }
                    break;
                case GameProgress.SubmittingCards:
                    if (!GameState.Instance.IsLocalPlayerDealer() && !GameState.Instance.PlayersConfirmed.Contains(GameState.Instance.GetLocalPlayerID()))
                    {
                        cardsLeftToSelect = DataCardSet.AllCards[GameState.Instance.CurrentQuestion].Picks - GameState.Instance.StagedCardsPerPlayer[GameState.Instance.GetLocalPlayerID()].Count;
                        if (cardsLeftToSelect > 0)
                        {
                            text = TextTag.SelectCards;
                            color = ColorType.Grey;
                        }
                        else
                        {
                            text = TextTag.Confirm;
                            color = ColorType.Cyan;
                        }
                    }
                    break;
                case GameProgress.ReadingAnswers:
                    if (GameState.Instance.IsLocalPlayerDealer())
                    {
                        text = TextTag.Next;
                        color = ColorType.Cyan;
                    }
                    break;
                case GameProgress.AwardingPoint:
                    if (GameState.Instance.IsLocalPlayerDealer())
                    {
                        text = TextTag.Select;
                        color = ColorType.Cyan;
                    }
                    break;
            }
            GameCardsActionTxt.SetText(text, (s) => { s = s.ToUpper(); if (cardsLeftToSelect > 0) { s = s.Replace("#VALUE", " " + cardsLeftToSelect.ToString()); } return s; });
            GameCardsActionBtn.GetComponent<UICircle>().Color(color);
        }
        private void Awake()
        {
            Instance = this;
            Initialize();
        }
        private void Start()
        {
            ReadingQuestionHints1Txt.SetText(TextTag.ReadQuestionHint1);
            ReadingQuestionHints2Txt.SetText(TextTag.ReadQuestionHint2);
            ReadingAnswerHints1Txt.SetText(TextTag.ReadAnswersHint1);
            ReadingAnswerHints2Txt.SetText(TextTag.ReadAnswersHint2);
            PlayerAnswersScrollTxt.SetText(TextTag.ScrollPlayerAnswers, (s) => s.ToUpper());
        }
        public void Activate(bool active)
        {
            this.gameObject.SetActive(active);
            if (active)
            {
                UpdateUI();
            }
        }
        public void UpdateUI()
        {
            DealNewCardsBtn.SetActive(GameState.Instance.Rules.AllowCardReplace);
            var playerCards = GameState.Instance.CardsPerPlayer[GameState.Instance.GetLocalPlayerID()].Except(GameState.Instance.StagedCardsPerPlayer[GameState.Instance.GetLocalPlayerID()]);
            List<int> topCards = new List<int>();
            switch (GameState.Instance.CurrentState)
            {
                case GameProgress.SubmittingCards:
                    topCards.AddRange(GameState.Instance.StagedCardsPerPlayer[GameState.Instance.GetLocalPlayerID()]);
                    break;
                case GameProgress.ReadingAnswers:
                    // Show indexToDealer-th player's cards to dealer, show indexToOther-th player's cards to everyone else
                    int index = GameState.Instance.IsLocalPlayerDealer() ? GameState.Instance.PlayerAnswersToReveal.indexToDealer : GameState.Instance.PlayerAnswersToReveal.indexToOthers;
                    if (GameState.Instance.PlayerAnswersToReveal.players.HasIndex(index))
                    {
                        topCards.AddRange(GameState.Instance.StagedCardsPerPlayer[GameState.Instance.PlayerAnswersToReveal.players[index]]);
                    }
                    break;
                case GameProgress.AwardingPoint:
                    // Show whatever the dealer is reading
                    topCards.AddRange(GameState.Instance.StagedCardsPerPlayer[GameState.Instance.PlayerAnswersToReveal.players[GameState.Instance.PlayerAnswersToReveal.indexToDealer]]);
                    break;
            }
            // Bottom cards: player cards
            for (int i = 0; i < GameState.MAX_CARDS_PER_PLAYER; i++)
            {
                if (playerCards.HasIndex(i))
                {
                    var cardID = playerCards.ElementAt(i);
                    PlayerCards[i].SetCard(cardID);
                    PlayerCards[i].SetActive(true);
                    PlayerCards[i].SetOnClickCallback(() =>
                    {
                        if (!GameState.Instance.IsLocalPlayerDealer())
                        {
                            switch (GameState.Instance.CurrentState)
                            {
                                case GameProgress.SubmittingCards:
                                    if (!GameState.Instance.PlayersConfirmed.Contains(GameState.Instance.GetLocalPlayerID())
                                     && GameState.Instance.StagedCardsPerPlayer[GameState.Instance.GetLocalPlayerID()].Count < DataCardSet.AllCards[GameState.Instance.CurrentQuestion].Picks)
                                    {
                                        GameState.Instance.StagedCardsPerPlayer[GameState.Instance.GetLocalPlayerID()].Add(cardID);
                                        UpdateUI();
                                    }
                                    break;
                            }
                        }
                    });
                    PlayerCards[i].ColorAlpha(GameState.Instance.IsLocalPlayerDealer() ? .6f : 1f);
                }
                else { PlayerCards[i].SetActive(false); }
            }
            // Top cards: question + player answers
            QuestionCard.SetActive(GameState.Instance.IsLocalPlayerDealer() || GameState.Instance.CurrentState != GameProgress.ReadingQuestion); // Only dealer should be able to see question during ReadingQuestion state
            QuestionCard.SetCard(GameState.Instance.CurrentQuestion, GameState.Instance.IsLocalPlayerDealer() ? 25 : 0);
            QuestionCard.ColorAlpha(1f);
            for (int i = 0; i < Card.MAX_PICKS; i++)
            {
                if (topCards.HasIndex(i))
                {
                    var cardID = topCards.ElementAt(i);
                    StagedCards[i].CardTxt.SetLastCharReveal(DateTime.UtcNow.AddMilliseconds(i == 0 ? 250 : (250 * i) + 25 * StagedCards.Take(i).Sum(x => x.CardTxt.TextLength)));
                    StagedCards[i].SetCard(cardID, GameState.Instance.IsLocalPlayerDealer() ? 25 : 0);
                    StagedCards[i].ColorAlpha(1f);
                    StagedCards[i].SetActive(true);
                    StagedCards[i].SetOnClickCallback(() =>
                    {
                        if (!GameState.Instance.IsLocalPlayerDealer() && !DataCardSet.AllCards[cardID].IsQuestion)
                        {
                            switch (GameState.Instance.CurrentState)
                            {
                                case GameProgress.SubmittingCards:
                                    if (!GameState.Instance.PlayersConfirmed.Contains(GameState.Instance.GetLocalPlayerID()))
                                    {
                                        GameState.Instance.StagedCardsPerPlayer[GameState.Instance.GetLocalPlayerID()].Remove(cardID);
                                        UpdateUI();
                                    }
                                    UpdateUI();
                                    break;
                            }
                        }
                    });
                }
                else { StagedCards[i].SetActive(false); }
                Placeholders[i].SetActive(
                    !GameState.Instance.IsLocalPlayerDealer()
                 && DataCardSet.AllCards.ContainsKey(GameState.Instance.CurrentQuestion)
                 && i < DataCardSet.AllCards[GameState.Instance.CurrentQuestion].Picks
                 && GameState.Instance.CurrentState == GameProgress.SubmittingCards);
            }
            PlayerAnswersScrollBtnContainer.SetActive(GameState.Instance.IsLocalPlayerDealer() && GameState.Instance.CurrentState == GameProgress.AwardingPoint);
            Action<bool> scroll = (down) =>
            {
                PunInterface.Instance.ExtrSetDealerCardIndex(GameState.Instance.PlayerAnswersToReveal.indexToDealer + (down ? 1 : -1));
            };
            PlayerAnswersScrollUpBtn.AddOnClickListener(() => scroll(false));
            PlayerAnswersScrollDownBtn.AddOnClickListener(() => scroll(true));

            UpdateActionBtnUI();

            bool showQuestionHints = MyFirstTimeAsCardDealer && GameState.Instance.IsLocalPlayerDealer() && GameState.Instance.CurrentState == GameProgress.ReadingQuestion;
            ReadingQuestionHints.SetActive(showQuestionHints);
            bool showAnswerHints = MyFirstTimeAsCardDealer && GameState.Instance.IsLocalPlayerDealer() && GameState.Instance.CurrentState == GameProgress.ReadingAnswers;
            ReadingAnswerHints.SetActive(showAnswerHints);
            ReadingAnswerHints1Txt.transform.localPosition = new Vector2(!DataCardSet.AllCards.ContainsKey(GameState.Instance.CurrentQuestion) || DataCardSet.AllCards[GameState.Instance.CurrentQuestion].Picks <= 1 ? 90 : 405, 167);
        }

        public static bool SetIndexToDealer(int newIndex)
        {
            Instance?.Initialize();
            if (GameState.Instance.PlayerAnswersToReveal.players.HasIndex(newIndex))
            {
                GameState.Instance.PlayerAnswersToReveal.indexToDealer = newIndex;
                Instance?.UpdateScrollerUI();
                Instance?.UpdateUI();
                return true;
            }
            return false;
        }

        public void UpdateScrollerUI()
        {
            PlayerAnswersScrollUpBtn.Color(GameState.Instance.PlayerAnswersToReveal.indexToDealer > 0 ? ColorType.Gold : ColorType.Grey);
            PlayerAnswersScrollDownBtn.Color(GameState.Instance.PlayerAnswersToReveal.indexToDealer < GameState.Instance.PlayerAnswersToReveal.players.Count - 1 ? ColorType.Gold : ColorType.Grey);
        }
    }
}
