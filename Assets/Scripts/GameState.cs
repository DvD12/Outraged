using Assets.Scripts;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Outraged
{
    public enum GameProgress
    {
        NotStarted,
        ReadingQuestion,
        SubmittingCards,
        ReadingAnswers,
        AwardingPoint,
        Ended
    }
    public class GameState
    {
        private static GameState _Instance;
        public static GameState Instance
        {
            get { return _Instance; }
        }
        public const int MAX_CARDS_PER_PLAYER = 5;
        public StatefulRandom StaticGameRandomGenerator;
        public int[] StaticGameRandomGeneratorState;
        public System.Random StaticLocalRandomGenerator = new System.Random();

        public GameProgress CurrentState = GameProgress.NotStarted;
        public RoomSettings Rules;

        public DateTime CurrentStateTimeoutStart;
        public DateTime CurrentStateTimeout;

        public Dictionary<Card.CardType, List<int>> AllCards = new Dictionary<Card.CardType, List<int>>() { { Card.CardType.Answer, new List<int>() }, { Card.CardType.Question, new List<int>() } };
        public Dictionary<Card.CardType, List<int>> CardStack = new Dictionary<Card.CardType, List<int>>() { { Card.CardType.Answer, new List<int>() }, { Card.CardType.Question, new List<int>() } }; // Using Stack<int> would make more sense, but Json deserializer reverts stack order -> OOS
        public Dictionary<string, HashSet<int>> CardsPerPlayer = new Dictionary<string, HashSet<int>>();
        public Dictionary<string, List<int>> StagedCardsPerPlayer = new Dictionary<string, List<int>>(); // Cards a player has selected to play this round
        public (List<string> players, int indexToOthers, int indexToDealer) PlayerAnswersToReveal = (new List<string>(), -1, 0); // During ReadingAnswers state, this stack is populated with all players; each time the dealer presses the Reveal btn one player's cards are revealed and we move on to the next
        public HashSet<string> PlayersConfirmed = new HashSet<string>(); // Players that have confirmed their selection
        public Dictionary<string, int> PlayersPoints = new Dictionary<string, int>();
        public string LastPointPlayer = "";
        public Dictionary<string, bool> AllPlayers = new Dictionary<string, bool>(); // value -> player in game = true (we want to save data about players that leave in case they rejoin)
        public Dictionary<string, int> AllPlayersActorNumbers = new Dictionary<string, int>();
        public int PlayerCount = 0;
        public Dictionary<string, string> PlayerNicknames = new Dictionary<string, string>(); // id -> nickname
        public HashSet<string> BotIds = new HashSet<string>();

        public string CurrentCardDealer;
        public int CardDealerIndex = 0;
        public int CurrentQuestion;

        public GameState()
        {
            Initialize();
        }

        public void SetActiveAsInstance()
        {
            Initialize();
        }
        public void Initialize()
        {
            _Instance = this;
            GamePlayersMenu.Instance?.ResetChat();
        }
        public void SaveState()
        {
            StaticGameRandomGeneratorState = StaticGameRandomGenerator.SaveState();
            if (GetLocalPlayer().IsMasterClient && PhotonNetwork.CurrentRoom != null)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
                {
                    { RoomSettings.GameState, Instance.Serialize() },
                    { RoomSettings.GameProgressString, this.CurrentState.ToString() }
                });
            }
            /*if (PhotonNetwork.CurrentRoom != null)
            {
                var fs = System.IO.File.Create(System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(Application.dataPath, ".."), "Logs"), "GameState.txt"));
                fs.Close();
                System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(Application.dataPath, ".."), "Logs"), "GameState.txt"), String.Empty);
                var sw = new System.IO.StreamWriter(System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(Application.dataPath, ".."), "Logs"), "GameState.txt"));
                sw.Write(Instance.Serialize());
                sw.Close();
            }*/
        }

        public bool IsStateTimed = false;
        public void SetGameProgress(GameProgress state)
        {
            if (CurrentState == GameProgress.NotStarted && state != GameProgress.NotStarted) // Game has started!
            {
                DealCards();
                MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.GameCards); 
            }
            GameState.Instance.CurrentState = state;
            CurrentStateTimeoutStart = DateTime.Now;
            switch (state)
            {
                case GameProgress.NotStarted:
                    IsStateTimed = false;
                    break;
                case GameProgress.ReadingQuestion:
                    CurrentStateTimeout = CurrentStateTimeoutStart.AddSeconds(180);
                    IsStateTimed = true;
                    ResetData();
                    DealQuestion();
                    DealCards();
                    NominateDealer();
                    break;
                case GameProgress.SubmittingCards:
                    GamePlayersMenu.Instance?.SendSystemChat(TextTag.WaitingForAnswers.GetText());
                    CurrentStateTimeout = CurrentStateTimeoutStart.AddSeconds(120);
                    IsStateTimed = true;
                    break;
                case GameProgress.ReadingAnswers:
                    GamePlayersMenu.Instance?.SendSystemChat(TextTag.DealerReadAnswers.GetText().Replace("#PLAYERNAME", PlayerNicknames[CurrentCardDealer].WrapColor(ColorType.Black)));
                    CurrentStateTimeout = CurrentStateTimeoutStart.AddSeconds(90 * (PlayerCount - 1));
                    IsStateTimed = true;
                    break;
                case GameProgress.AwardingPoint:
                    GamePlayersMenu.Instance?.SendSystemChat(TextTag.DealerSelectAnswer.GetText().Replace("#PLAYERNAME", PlayerNicknames[CurrentCardDealer].WrapColor(ColorType.Black)));
                    GameCardsMenu.SetIndexToDealer(PlayerAnswersToReveal.players.Count - 1); // Dealer needs to read through them from the end again
                    CurrentStateTimeout = CurrentStateTimeoutStart.AddSeconds(30 * (PlayerCount - 1));
                    IsStateTimed = true;
                    break;
            }
            GameCardsMenu.Instance?.UpdateUI();
            SaveState();
        }

        public bool IsLocalPlayerDealer() => GetLocalPlayerID() == CurrentCardDealer;
        public Player GetLocalPlayer() => PhotonNetwork.LocalPlayer;
        public string GetLocalPlayerID() => GetLocalPlayer().GetID();
        public void SetRandom(int Seed)
        {
            StaticGameRandomGenerator = new StatefulRandom(Seed);
            StaticGameRandomGeneratorState = StaticGameRandomGenerator.SaveState();
        }

        /// <summary>
        /// Returns an integer from min to max, both inclusive
        /// </summary>
        /// <param name="min">Inclusive lower bound</param>
        /// <param name="max">Inclusive upper bound</param>
        /// <returns>An integer within the bounds</returns>
        public int GetRandomRange(int min, int max, string logAppend = "", bool NetworkSync = false, bool log = true)
        {
            var value = 0;
            if (NetworkSync)
            {
                value = StaticGameRandomGenerator.NextInteger(min, max + 1); // Next takes integer in the interval [min, max) and we want [min, max], i.e. max inclusive
            }
            else if (GameState.Instance != null)
            {
                value = GameState.Instance.StaticLocalRandomGenerator.Next(min, max + 1);
            }
            else
            {
                value = new System.Random(Guid.NewGuid().GetHashCode()).Next(min, max + 1);
            }
            return value;
        }
        public void ResetData()
        {
            foreach (var player in StagedCardsPerPlayer.Keys)
            {
                StagedCardsPerPlayer[player].Clear();
            }
            PlayersConfirmed.Clear();
            PlayerAnswersToReveal.players.Clear();
            PlayerAnswersToReveal.indexToOthers = -1;
            PlayerAnswersToReveal.indexToDealer = 0;
            SaveState();
        }

        public void AddAiPlayer(string name, string guid)
        {
            BotIds.Add(guid);
            AllPlayers[guid] = true;
            PlayerNicknames[guid] = name;
            CardsPerPlayer.AddIfNotPresent(guid, new HashSet<int>());
            DealCards(false);
            StagedCardsPerPlayer.AddIfNotPresent(guid, new List<int>());
            PlayersPoints.AddIfNotPresent(guid, 0);
            PlayerCount++;
            SaveState();
            GamePlayersMenu.Instance?.AddAiPlayer(name, guid);

            if (GetLocalPlayer().IsMasterClient)
            {
                TaskController.Create(AiManager.GetRunName(guid), AiManager.Run(guid));
            }
        }
        public void AddPlayer(Player player)
        {
            bool stateChanged = false;
            var id = player.GetID();
            stateChanged |= AllPlayers.ContainsKey(id) && AllPlayers[id] == false; // player already in DB and in room -> state unchanged
            AllPlayers[id] = true;
            AllPlayersActorNumbers[id] = player.ActorNumber;
            PlayerNicknames[player.GetID()] = player.NickName;
            stateChanged |= CardsPerPlayer.AddIfNotPresent(player.GetID(), new HashSet<int>());
            if (stateChanged) { DealCards(false); }
            stateChanged |= StagedCardsPerPlayer.AddIfNotPresent(player.GetID(), new List<int>());
            stateChanged |= PlayersPoints.AddIfNotPresent(player.GetID(), 0);
            if (stateChanged)
            {
                PlayerCount++;
                SaveState();
            }
        }

        public void RemoveAiPlayer(string id)
        {
            TaskController.Stop(AiManager.GetRunName(id));
            bool stateChanged = !AllPlayers.ContainsKey(id) || AllPlayers[id] == true; // player not in DB or already removed -> state unchanged
            AllPlayers[id] = false;
            BotIds.Remove(id);
            RemovePlayerCallback(stateChanged);
            GamePlayersMenu.Instance?.RemoveAiPlayer(id);
        }
        public void RemovePlayer(Player player)
        {
            var id = player.GetID();
            bool stateChanged = !AllPlayers.ContainsKey(id) || AllPlayers[id] == true; // player not in DB or already removed -> state unchanged
            AllPlayers[id] = false;
            AllPlayersActorNumbers.Remove(id); // actor number is a non-permanent ID (the same player leaving and re-joining room might get a different actor number) so we need to remove it
            RemovePlayerCallback(stateChanged);
        }
        private void RemovePlayerCallback(bool stateChanged)
        {
            if (CurrentState != GameProgress.NotStarted && AllPlayers[CurrentCardDealer] == false) // Current dealer disconnected? Nominate a new one!
            {
                NominateDealer();
            }
            if (stateChanged)
            {
                PlayerCount--;
                SaveState();
            }
        }
        public Player GetPlayer(string id) { if (AllPlayersActorNumbers.ContainsKey(id)) { return PhotonNetwork.CurrentRoom.GetPlayer(AllPlayersActorNumbers[id]); } else { return null; } }
        public void NominateDealer()
        {
            if (GameCardsMenu.Instance != null && GetLocalPlayerID() == CurrentCardDealer) { GameCardsMenu.Instance.MyFirstTimeAsCardDealer = false; }
            if (!Rules.LastPointIsDealer || !AllPlayers.ContainsKey(LastPointPlayer) || AllPlayers[LastPointPlayer] == false)
            {
                var players = AllPlayers.Where(x => x.Value == true).ToList();
                CardDealerIndex = (CardDealerIndex + 1) % players.Count;
                CurrentCardDealer = players[CardDealerIndex].Key;
            }
            else
            {
                CurrentCardDealer = LastPointPlayer;
            }
            CheckMoveToReadingAnswers();
            GamePlayersMenu.Instance?.SendSystemChat(TextTag.DealerNew.GetText().Replace("#PLAYERNAME", PlayerNicknames[CurrentCardDealer].WrapColor(ColorType.Black)));
            GameCardsMenu.Instance?.UpdateUI();
            SaveState();
        }
        public void DealQuestion()
        {
            int q = GetCard(Card.CardType.Question, false);
            CurrentQuestion = q;
            SaveState();
        }
        public void RemoveCards(string player, bool saveState = true)
        {
            CardsPerPlayer[player].Clear();
            if (CurrentState != GameProgress.ReadingAnswers && CurrentState != GameProgress.AwardingPoint) // During these phases staged cards are committed - do not remove them!
            {
                StagedCardsPerPlayer[player].Clear();
            }
            if (saveState) { SaveState(); }
        }
        public void DealCards(string player, bool saveState = true)
        {
            while (CardsPerPlayer[player].Count + (StagedCardsPerPlayer.ContainsKey(player) ? StagedCardsPerPlayer[player].Count : 0) < MAX_CARDS_PER_PLAYER) // Player cards = cards in deck + cards currently selected for the current question
            {
                int newCard = GetCard(Card.CardType.Answer);
                CardsPerPlayer[player].Add(newCard);
            }
            if (saveState)
            {
                SaveState();
            }
        }
        public void DealCards(bool saveState = true)
        {
            foreach (var player in CardsPerPlayer.Keys)
            {
                DealCards(player, false);
            }
            if (saveState)
            {
                SaveState();
            }
        }
        public int GetCard(Card.CardType type, bool saveState = true)
        {
            if (CardStack[type].Count == 0)
            {
                GenerateCards(type);
            }
            var result = CardStack[type].Pop();
            if (saveState) { SaveState(); }
            return result;
        }
        public void GetCardSets(List<string> cardSetsOverride = null) // Generates card pool (ordered) from card sets chosen at room creation. Should only be done once
        {
            if (AllCards.Any(x => x.Value.Count != 0)) { return; }
            foreach (var cardset in cardSetsOverride ?? PhotonNetwork.CurrentRoom.DeserializeSettings().CardSets)
            {
                AllCards[Card.CardType.Answer].AddRange(DataCardSet.CardSetsToAnswers[cardset]);
                AllCards[Card.CardType.Question].AddRange(DataCardSet.CardSetsToQuestions[cardset]);
            }
            SaveState();
        }
        public void GenerateCards(Card.CardType type, List<string> cardSetsOverride = null)
        {
            if (AllCards == null || AllCards.Any(x => x.Value.Count == 0) || AllCards.Count == 0)
            {
                GetCardSets(cardSetsOverride);
            }
            foreach (var c in AllCards[type].RandomizeOrder(LogAppend: "GenerateCards(" + type + "): ", NetworkSync: true))
            {
                CardStack[type].Push(c);
            }
            SaveState();
        }
        /// <summary>
        /// Finalize the player's answer choice
        /// </summary>
        public void SetPlayerConfirmation(string id, int[] cards)
        {
            bool added = PlayersConfirmed.Add(id);
            if (added)
            {
                PlayerAnswersToReveal.players.Add(id);
            }

            StagedCardsPerPlayer[id].Clear();
            StagedCardsPerPlayer[id].AddRange(cards);

            // If player failed to select enough cards in time, we add the remaining automatically
            int missingCardCount = DataCardSet.AllCards[CurrentQuestion].Picks - cards.Count();
            if (missingCardCount > 0)
            {
                var remainingCards = CardsPerPlayer[id].Except(StagedCardsPerPlayer[id]);
                for (int i = 0; i < missingCardCount; i++)
                {
                    StagedCardsPerPlayer[id].Add(remainingCards.ElementAt(i));
                }
            }

            foreach (var card in StagedCardsPerPlayer[id])
            {
                CardsPerPlayer[id].Remove(card); // Up until this point all cards are still the player's even though they are in StagedCardsPerPlayer as well
            }
            CheckMoveToReadingAnswers();
            SaveState();
        }
        public void CheckMoveToReadingAnswers()
        {
            if (CurrentState == GameProgress.SubmittingCards && PlayersConfirmed.Count >= PlayerCount - 1) // All players minus the card dealer
            {
                SetGameProgress(GameProgress.ReadingAnswers);
            }
        }

        public void AlterPlayerPoints(string id, int delta, bool silent = false, bool stateProgress = true)
        {
            PlayersPoints[id] += delta;
            if (delta > 0)
            {
                LastPointPlayer = id;
            }
            if (!silent)
            {
                string PlayerPosition = "";
                var players = AllPlayers.Where(x => x.Value == true).OrderByDescending(x => PlayersPoints[x.Key]).Select(x => new { id = x.Key, name = PlayerNicknames[x.Key] });
                for (int i = 0; i < players.Count(); i++)
                {
                    if (players.ElementAt(i).id == LastPointPlayer)
                    {
                        PlayerPosition = (i + 1).ToString().WrapColor(ColorType.Red);
                        break;
                    }
                }
                GamePlayersMenu.Instance?.SendSystemChat(TextTag.PointsAwarded.GetText().Replace("#PLAYERPOSITION", PlayerPosition).Replace("#VALUE", delta.ToString().WrapColor(ColorType.Black)).Replace("#POINTORPOINTS", delta == 1 ? TextTag.Point.GetText().ToLower() : TextTag.Points.GetText().ToLower()).Replace("#PLAYERNAME", PlayerNicknames[id].WrapColor(ColorType.Black)).Replace("#CURRENTVALUE", PlayersPoints[id].ToString().WrapColor(ColorType.Black)));
            }
            if (stateProgress)
            {
                bool victory = CheckVictory().IsValid();
                if (!silent && !victory && delta > 0)
                {
                    MenuHandler.Instance.UI_PointAwardedDialog.gameObject.SetActive(true);
                    UI_PointAwardedDialog.Instance.Activate(true, delta);
                }
            }
        }
        public string CheckVictory()
        {
            var players = PlayersPoints.Where(x => x.Value >= Rules.MaxPoints);
            string result = players.Count() > 0 ? players.First().Key : "";
;
            if (result.IsValid())
            {
                SetGameProgress(GameProgress.Ended);
                MenuHandler.Instance.UI_GameEndDialog.gameObject.SetActive(true);
                UI_GameEndDialog.Instance.Activate(true);
            }
            else
            {
                SetGameProgress(GameProgress.ReadingQuestion);
            }
            SaveState();
            return result;
        }
    }
}
