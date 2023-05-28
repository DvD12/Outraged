using Outraged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class AiManager
    {
        public static readonly List<string> AiPlayerNames = new List<string>()
        {
            "Bot mcBotFace",
            "Botty the dumb AI",
            "xXx_/cARD~dESTROYER\\_xXx",
            "HAL 9000",
            "Smart AI",
            "Alexa 2.0",
            "I'm on a BOT",
            "David Bottie",
            "PokerMaster",
            "RageAgainstHumanity",
            "format C:\\>",
            "Stuxnet",
            "Hello, world",
            "CTRL+ALT+DEL",
            "Steve Jobs",
            "Bill Gates",
            "Mark Zuckerberg",
            "Jeff Bezos",
        };

        public static string GetRunName(string playerGuid) => $"{nameof(Run)}_{playerGuid}";

        public static string GetNewAiName()
        {
            if (GameState.Instance == null)
            {
                return AiPlayerNames.GetRandomElement();
            }

            var existing = GameState.Instance.AllPlayers.Where(x => x.Value == true && GameState.Instance.BotIds.Contains(x.Key));
            var available = AiPlayerNames.Except(existing.Select(x => GameState.Instance.PlayerNicknames[x.Key]));

            if (available.Any())
            {
                return available.GetRandomElement();
            }
            else
            {
                return AiPlayerNames.GetRandomElement() + $"_#{GameState.Instance.GetRandomRange(100, 999)}";
            }
        }

        public static IEnumerator Run(string playerGuid)
        {
            while (GameState.Instance == null)
            {
                yield return new WaitForSeconds(5);
            }

            // Player not present!
            if (!GameState.Instance.AllPlayers.ContainsKey(playerGuid) || GameState.Instance.AllPlayers[playerGuid] == false)
            {
                yield break;
            }

            // Player is not a bot!
            if (!GameState.Instance.BotIds.Contains(playerGuid))
            {
                yield break;
            }

            while (GameState.Instance.CurrentState != GameProgress.Ended)
            {
                if (GameState.Instance.CurrentCardDealer == playerGuid)
                {
                    if (GameState.Instance.CurrentState == GameProgress.ReadingQuestion)
                    {
                        yield return new WaitForSeconds(2);
                        PunInterface.Instance.ExtrSetGameProgress(GameProgress.SubmittingCards);
                    }
                    else if (GameState.Instance.CurrentState == GameProgress.ReadingAnswers)
                    {
                        yield return new WaitForSeconds(GameState.Instance.GetRandomRange(1, 3, "", false, true));
                        var currentIndex = GameState.Instance.PlayerAnswersToReveal.indexToDealer;
                        PunInterface.Instance.ExtrSetAnswersShownIndex(currentIndex, currentIndex + 1); // Show whatever the dealer is reading to everyone else; if we've reached the end, move on to AwardingPoint state
                    }
                    else if (GameState.Instance.CurrentState == GameProgress.AwardingPoint)
                    {
                        var winningCardIndex = GameState.Instance.GetRandomRange(0, GameState.Instance.PlayerAnswersToReveal.players.Count - 1);
                        int i = 100;
                        while (GameState.Instance.PlayerAnswersToReveal.indexToDealer != winningCardIndex)
                        {
                            yield return new WaitForSeconds(GameState.Instance.GetRandomRange(1, 2));
                            PunInterface.Instance.ExtrSetDealerCardIndex(GameState.Instance.PlayerAnswersToReveal.indexToDealer + (GameState.Instance.PlayerAnswersToReveal.indexToDealer < winningCardIndex ? 1 : -1));
                            i--;
                            if (i < 0) { break; }
                        }
                        yield return new WaitForSeconds(GameState.Instance.GetRandomRange(2, 4));
                        PunInterface.Instance.ExtrAlterPlayerPoints(GameState.Instance.PlayerAnswersToReveal.players[GameState.Instance.PlayerAnswersToReveal.indexToDealer], 1);
                    }
                }
                else
                {
                    if (GameState.Instance.CurrentState == GameProgress.SubmittingCards)
                    {
                        while (!GameState.Instance.PlayersConfirmed.Contains(playerGuid) && GameState.Instance.StagedCardsPerPlayer[playerGuid].Count < DataCardSet.AllCards[GameState.Instance.CurrentQuestion].Picks)
                        {
                            yield return new WaitForSeconds(GameState.Instance.GetRandomRange(6, 18));
                            var remainingCards = GameState.Instance.CardsPerPlayer[playerGuid].Except(GameState.Instance.StagedCardsPerPlayer[playerGuid]);
                            var chosenCard = remainingCards.GetRandomElement();
                            GameState.Instance.StagedCardsPerPlayer[playerGuid].Add(chosenCard);
                            PunInterface.Instance.ExtrSetPlayerConfirmation(playerGuid, GameState.Instance.StagedCardsPerPlayer[playerGuid].ToArray());
                        }
                    }
                }
                yield return new WaitForSeconds(3);
            }
        }
    }
}
