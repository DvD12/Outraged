using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Outraged
{
    public enum Language
    {
        English,
        French,
        German,
        Italian,
        Spanish
    }
    public static class DataLanguages
    {
        public const Language FallbackLanguage = Language.English;
        public static Dictionary<Language, Dictionary<TextTag, string>> Dict;
        public static Dictionary<TextTag, string> CurrentDictionary;

        public static void LoadJson()
        {
            var data = Resources.Load("Data/Dictionary");
            Dict = JsonConvert.DeserializeObject<Dictionary<Language, Dictionary<TextTag, string>>>(data.ToString(), DataHeaders.GetFormatting());
            CurrentDictionary = new Dictionary<TextTag, string>();
            foreach (TextTag tag in Enum.GetValues(typeof(TextTag)))
            {
                CurrentDictionary.Add(tag, Dict[FallbackLanguage][tag]);
            }
        }

        public static string GetText(this TextTag tag)
        {
            if (Dict == null) { LoadJson(); }
            if (Dict[Profile.GetAppLanguage()].ContainsKey(tag)) { return Dict[Profile.GetAppLanguage()][tag]; }
            else if (Dict[FallbackLanguage].ContainsKey(tag)) { return Dict[FallbackLanguage][tag]; }
            else return tag.ToString();
        }
    }
    public enum TextTag
    {
        ActionBtnSelectTxt,
        AllowHotjoin,
        AllowVoiceChat,
        Any,
        AppLanguage,
        Back,
        Cancel,
        CardSent,
        CardSets,
        CardSetsAuthorTxt,
        CardSetsAuthorTxtTooltip,
        CardSetsCardTextTxt,
        CardSetsCardTextTooltip,
        CardSetsLanguageTxt,
        CardSetsLanguageTxtTooltip,
        CardSetsMenuBackBtn,
        CardSetsQuestionCardTxt,
        CardSetsQuestionCardTxtTooltip,
        CardSetsQuestionPicksTxt,
        CardSetsQuestionPicksTxtTooltip,
        CardSetsSendBtnTooltip,
        CardsValue,
        ChoiceSelectionDialogElemsLeftToSelectTxt,
        Confirm,
        ConfirmDealNewCards,
        ConfirmDealNewCardsDesc,
        ConfirmQuit,
        ConfirmQuitDesc,
        Create,
        CreateGame,
        CreateMenuAllowHotjoinTxt,
        CreateMenuBackBtn,
        CreateMenuCardSetsTxt,
        CreateMenuCreateBtn,
        CreateMenuEnableVoiceChatTxt,
        CreateMenuLanguageTxt,
        CreateMenuMaxPlayersTxt,
        CreateMenuMaxPointsTxt,
        CreateMenuNicknameTxt,
        CreateMenuNotConnected,
        CreateMenuPasswordTxt,
        CreateMenuRoomNameTxt,
        DealerNew,
        DealerReadAnswers,
        DealerSelectAnswer,
        Default,
        Disabled,
        Element,
        Elements,
        Empty,
        Failure,
        FunAtParties,
        GamePlayersMenuBackToCardsMenu,
        GamePlayersMenuLeave,
        GamePlayersMenuNotEnoughPlayers,
        GamePlayersMenuStartGame,
        HonourableMentions,
        HowTo,
        Join,
        JoinGame,
        JoinMenuBackBtn,
        JoinMenuJoinBtn,
        JoinMenuJoinBtnNoRoomSelected,
        JoinMenuRefreshBtn,
        InProgress,
        Input,
        Language,
        LastPointIsDealer,
        LastPointIsDealerTooltip,
        Leave,
        MainMenuCreateBtn,
        MainMenuJoinBtn,
        MainMenuTip,
        MainMenuQuitBtn,
        MaxPlayers,
        MaxPoints,
        Microphone,
        Next,
        Nickname,
        No,
        OK,
        Okaydots,
        Options,
        Password,
        PickCards,
        PlayerJoined,
        PlayerLeft,
        Point,
        Points,
        PointsAwarded,
        PointsAwardedWinning1,
        PointsAwardedWinning2,
        PointsAwardedWinning3,
        PointsAwardedWinning4,
        PointsAwardedWinning5,
        PointsAwardedWinning6,
        PointsAwardedWinning7,
        PointsAwardedWinning8,
        PointsAwardedWinning9,
        PointsAwardedWinning10,
        PointsAwardedWinning11,
        PointsAwardedWinning12,
        PointsAwardedMatchPoint1,
        PointsAwardedMatchPoint2,
        PointsAwardedMatchPoint3,
        PointsAwardedLoser1,
        PointsAwardedLoser2,
        PointsAwardedLoser3,
        PointsAwardedLoser4,
        PointsAwardedLoser5,
        PointsAwardedLoser6,
        PointsAwardedNormal1,
        PointsAwardedNormal2,
        PointsAwardedNormal3,
        PointsAwardedNormal4,
        PointsAwardedNormal5,
        PointsAwardedNormal6,
        PointsAwardedNormal7,
        PointsAwardedNormal8,
        ReadAnswersHint1,
        ReadAnswersHint2,
        ReadQuestionHint1,
        ReadQuestionHint2,
        Refresh,
        Reveal,
        RoomName,
        RoomTableCardSetsTxt,
        RoomTableIsInProgressTxt,
        RoomTableIsPasswordProtectedImg,
        RoomTableLanguageTxt,
        RoomTableRoomNameTxt,
        RoomTablePlayersImg,
        RoomTableVoiceEnabledImg,
        ScrollPlayerAnswers,
        Select,
        SelectElements,
        SelectCards,
        Send,
        Start,
        Success,
        TipPressHold,
        Quit,
        UnableToSendCard,
        Waiting,
        WaitingForAnswers,
        WeHaveAWinner,
        Yay,
        Yes
    }
}
