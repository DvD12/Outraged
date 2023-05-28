using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Outraged
{
    public class CreateMenu : MonoBehaviour
    {
        public static CreateMenu Instance;
        public static DynamicText CreateMenuTitleTxt;
        public static TouchBtn CreateMenuNicknameTxt;
        public static TMP_InputField CreateMenuNicknameValueTxt;
        public static TouchBtn CreateMenuRoomNameTxt;
        public static TMP_InputField CreateMenuRoomNameValueTxt;
        public static TouchBtn CreateMenuMaxPlayersTxt;
        public static DynamicText CreateMenuMaxPlayersValueTxt;
        public static TouchBtn CreateMenuMaxPlayersPlusBtn;
        public static TouchBtn CreateMenuMaxPlayersMinusBtn;
        public static TouchBtn CreateMenuPasswordTxt;
        public static TMP_InputField CreateMenuPasswordValueTxt;
        public static TouchBtn CreateMenuLanguageTxt;
        public static TouchBtn CreateMenuLanguageValueTxt;
        public static TouchBtn CreateMenuCardSetsTxt;
        public static TouchBtn CreateMenuCardSetsValueTxt;
        public static TouchBtn CreateMenuMaxPointsTxt;
        public static DynamicText CreateMenuMaxPointsValueTxt;
        public static TouchBtn CreateMenuMaxPointsPlusBtn;
        public static TouchBtn CreateMenuMaxPointsMinusBtn;
        public static TouchBtn CreateMenuAllowHotjoinTxt;
        public static Checkbox CreateMenuAllowHotjoinBtn;
        public static TouchBtn CreateMenuLastPointIsDealerTxt;
        public static Checkbox CreateMenuLastPointIsDealerBtn;
        public static TouchBtn CreateMenuEnableVoiceChatTxt;
        public static Checkbox CreateMenuEnableVoiceChatBtn;
        public static UI_TxtImgBtn CreateMenuCreateBtn;
        public static UI_TxtImgBtn CreateMenuBackBtn;

        private RoomSettings NewRoomSettings;

        private void Awake()
        {
            Instance = this;
            CreateMenuTitleTxt = Helpers.Find<DynamicText>(nameof(CreateMenuTitleTxt));
            CreateMenuNicknameTxt = Helpers.Find<TouchBtn>(nameof(CreateMenuNicknameTxt));
            CreateMenuNicknameValueTxt = Helpers.Find<TMP_InputField>(nameof(CreateMenuNicknameValueTxt));
            CreateMenuRoomNameTxt = Helpers.Find<TouchBtn>(nameof(CreateMenuRoomNameTxt));
            CreateMenuRoomNameValueTxt = Helpers.Find<TMP_InputField>(nameof(CreateMenuRoomNameValueTxt));
            CreateMenuMaxPlayersTxt = Helpers.Find<TouchBtn>(nameof(CreateMenuMaxPlayersTxt));
            CreateMenuMaxPlayersValueTxt = Helpers.Find<DynamicText>(nameof(CreateMenuMaxPlayersValueTxt));
            CreateMenuMaxPlayersPlusBtn = Helpers.Find<TouchBtn>(nameof(CreateMenuMaxPlayersPlusBtn));
            CreateMenuMaxPlayersMinusBtn = Helpers.Find<TouchBtn>(nameof(CreateMenuMaxPlayersMinusBtn));
            CreateMenuPasswordTxt = Helpers.Find<TouchBtn>(nameof(CreateMenuPasswordTxt));
            CreateMenuPasswordValueTxt = Helpers.Find<TMP_InputField>(nameof(CreateMenuPasswordValueTxt));
            CreateMenuLanguageTxt = Helpers.Find<TouchBtn>(nameof(CreateMenuLanguageTxt));
            CreateMenuLanguageValueTxt = Helpers.Find<TouchBtn>(nameof(CreateMenuLanguageValueTxt));
            CreateMenuCardSetsTxt = Helpers.Find<TouchBtn>(nameof(CreateMenuCardSetsTxt));
            CreateMenuCardSetsValueTxt = Helpers.Find<TouchBtn>(nameof(CreateMenuCardSetsValueTxt));
            CreateMenuMaxPointsTxt = Helpers.Find<TouchBtn>(nameof(CreateMenuMaxPointsTxt));
            CreateMenuMaxPointsValueTxt = Helpers.Find<DynamicText>(nameof(CreateMenuMaxPointsValueTxt));
            CreateMenuMaxPointsPlusBtn = Helpers.Find<TouchBtn>(nameof(CreateMenuMaxPointsPlusBtn));
            CreateMenuMaxPointsMinusBtn = Helpers.Find<TouchBtn>(nameof(CreateMenuMaxPointsMinusBtn));
            CreateMenuAllowHotjoinTxt = Helpers.Find<TouchBtn>(nameof(CreateMenuAllowHotjoinTxt));
            CreateMenuAllowHotjoinBtn = Helpers.Find<Checkbox>(nameof(CreateMenuAllowHotjoinBtn));
            CreateMenuLastPointIsDealerTxt = Helpers.Find<TouchBtn>(nameof(CreateMenuLastPointIsDealerTxt));
            CreateMenuLastPointIsDealerBtn = Helpers.Find<Checkbox>(nameof(CreateMenuLastPointIsDealerBtn));
            CreateMenuEnableVoiceChatTxt = Helpers.Find<TouchBtn>(nameof(CreateMenuEnableVoiceChatTxt));
            CreateMenuEnableVoiceChatBtn = Helpers.Find<Checkbox>(nameof(CreateMenuEnableVoiceChatBtn));
            CreateMenuCreateBtn = Helpers.Find<UI_TxtImgBtn>(nameof(CreateMenuCreateBtn));
            CreateMenuBackBtn = Helpers.Find<UI_TxtImgBtn>(nameof(CreateMenuBackBtn));
        }

        private void Start()
        {
            CreateMenuTitleTxt.Color(ColorType.Gold);
            CreateMenuTitleTxt.SetText(TextTag.CreateGame, (s) => s.ToUpper());
            CreateMenuNicknameTxt.Color(ColorType.Gold);
            CreateMenuNicknameTxt.GetComponent<DynamicText>().SetText(TextTag.Nickname);
            CreateMenuNicknameValueTxt.Color(ColorType.Gold);
            CreateMenuRoomNameTxt.Color(ColorType.Gold);
            CreateMenuRoomNameTxt.GetComponent<DynamicText>().SetText(TextTag.RoomName);
            CreateMenuRoomNameValueTxt.Color(ColorType.Gold);
            CreateMenuMaxPlayersTxt.Color(ColorType.Gold);
            CreateMenuMaxPlayersTxt.GetComponent<DynamicText>().SetText(TextTag.MaxPlayers);
            CreateMenuMaxPlayersValueTxt.Color(ColorType.Gold);
            CreateMenuPasswordTxt.Color(ColorType.Gold);
            CreateMenuPasswordTxt.GetComponent<DynamicText>().SetText(TextTag.Password);
            CreateMenuPasswordValueTxt.Color(ColorType.Gold);
            CreateMenuLanguageTxt.Color(ColorType.Gold);
            CreateMenuLanguageTxt.GetComponent<DynamicText>().SetText(TextTag.Language);
            CreateMenuLanguageValueTxt.Color(ColorType.Gold);
            CreateMenuCardSetsTxt.Color(ColorType.Gold);
            CreateMenuCardSetsTxt.GetComponent<DynamicText>().SetText(TextTag.CardSets);
            CreateMenuCardSetsValueTxt.Color(ColorType.Gold);
            CreateMenuMaxPointsTxt.Color(ColorType.Gold);
            CreateMenuMaxPointsTxt.GetComponent<DynamicText>().SetText(TextTag.MaxPoints);
            CreateMenuMaxPointsValueTxt.Color(ColorType.Gold);
            CreateMenuAllowHotjoinTxt.Color(ColorType.Gold);
            CreateMenuAllowHotjoinTxt.GetComponent<DynamicText>().SetText(TextTag.AllowHotjoin);
            CreateMenuLastPointIsDealerTxt.Color(ColorType.Gold);
            CreateMenuLastPointIsDealerTxt.GetComponent<DynamicText>().SetText(TextTag.LastPointIsDealer);
            CreateMenuEnableVoiceChatTxt.Color(ColorType.Gold);
            CreateMenuEnableVoiceChatTxt.GetComponent<DynamicText>().SetText(TextTag.AllowVoiceChat);
            CreateMenuCreateBtn.SetText(TextTag.Create, (s) => s.ToUpper(), 60, true);
            CreateMenuBackBtn.SetText(TextTag.Back, (s) => s.ToUpper(), 60, true);
            CreateMenuCreateBtn.GetBtn().AddOnClickListener(() =>
            {
                PunInterface.Instance.CreateRoom(CreateMenuRoomNameValueTxt.text, byte.Parse(CreateMenuMaxPlayersValueTxt.Text.text), NewRoomSettings, () => { });
            });
            CreateMenuBackBtn.GetBtn().AddOnClickListener(() =>
            {
                MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.Main);
            });

            CreateMenuNicknameValueTxt.OnValueChanged((s) => Profile.SetNickname(s));
            CreateMenuRoomNameValueTxt.OnValueChanged((s) => Profile.SetRoomName(s));
            Action<int> MaxPlayersAction = (i) => { Profile.SetMaxPlayers((Profile.GetMaxPlayers() + i).Clamp(0, RoomSettings.MAX_PLAYERS)); CreateMenuMaxPlayersValueTxt.Text.text = Profile.GetMaxPlayers().ToString(); };
            CreateMenuMaxPlayersMinusBtn.AddOnClickListener(() => MaxPlayersAction(-1));
            CreateMenuMaxPlayersPlusBtn.AddOnClickListener(() => MaxPlayersAction(1));
            CreateMenuPasswordValueTxt.OnValueChanged((s) => this.NewRoomSettings.Password = s );
            CreateMenuLanguageValueTxt.AddOnClickListener(() =>
            {
                MenuHandler.Instance.UI_ChoiceSelectionDialog.gameObject.SetActive(true);
                List<UI_ChoiceSelectionDialog.ChoiceSelection> c = new List<UI_ChoiceSelectionDialog.ChoiceSelection>();
                foreach (Language choice in Enum.GetValues(typeof(Language)))
                {
                    c.Add(new UI_ChoiceSelectionDialog.ChoiceSelection(choice.ToString(), choice.GetString()));
                }
                UI_ChoiceSelectionDialog.Instance.Activate(true, c, "Language", 1, new Action<HashSet<string>>((choices) =>
                {
                    Language newLang = (Language)Enum.Parse(typeof(Language), choices.First());
                    NewRoomSettings.RoomLanguage = newLang;
                    Profile.SetLanguage(newLang);
                    CreateMenuLanguageValueTxt.GetComponent<DynamicText>().Text.text = newLang.GetString();
                }));
            });
            CreateMenuCardSetsValueTxt.AddOnClickListener(() =>
            {
                MenuHandler.Instance.UI_ChoiceSelectionDialog.gameObject.SetActive(true);
                List<UI_ChoiceSelectionDialog.ChoiceSelection> c = new List<UI_ChoiceSelectionDialog.ChoiceSelection>();
                foreach (string cardSetName in DataCardSet.CardSets.Keys)
                {
                    c.Add(new UI_ChoiceSelectionDialog.ChoiceSelection(cardSetName, DataCardSet.CardSets[cardSetName].ToString()));
                }
                UI_ChoiceSelectionDialog.Instance.Activate(true, c, "Card sets", callback: new Action<HashSet<string>>((choices) =>
                {
                    var result = choices.ToList();
                    NewRoomSettings.CardSets = result;
                    Profile.SetCardSets(result);
                    CreateMenuCardSetsValueTxt.GetComponent<DynamicText>().Text.text = result.Select(x => DataCardSet.CardSets[x]).GetString();
                }));
            });
            Action<int> MaxPointsAction = (i) =>
            {
                int value = (Profile.GetMaxPoints() + i).Clamp(0, RoomSettings.MAX_POINTS);
                NewRoomSettings.MaxPoints = value;
                Profile.SetMaxPoints(value);
                CreateMenuMaxPointsValueTxt.Text.text = Profile.GetMaxPoints().ToString();
            };
            CreateMenuMaxPointsMinusBtn.AddOnClickListener(() => MaxPointsAction(-1));
            CreateMenuMaxPointsPlusBtn.AddOnClickListener(() => MaxPointsAction(1));
            CreateMenuAllowHotjoinBtn.OnCheck((value) => { NewRoomSettings.AllowHotjoin = value; Profile.SetAllowHotjoin(value); });
            CreateMenuLastPointIsDealerBtn.OnCheck((value) => { NewRoomSettings.LastPointIsDealer = value; Profile.SetAllowLastPointDealer(value); });
            CreateMenuEnableVoiceChatBtn.OnCheck((value) => { NewRoomSettings.AllowVoiceChat = value; Profile.SetAllowVoiceChat(value); });
        }

        public void Activate(bool active)
        {
            this.gameObject.SetActive(active);
            if (active)
            {
                this.NewRoomSettings = new RoomSettings("", Profile.GetLanguage(), Profile.GetCardSets().ToList(), Profile.GetMaxPoints(), Profile.GetAllowHotjoin(), Profile.GetAllowVoiceChat(), PunInterface.Instance.GetIP());
                CreateMenuNicknameValueTxt.text = Profile.GetNickname();
                CreateMenuRoomNameValueTxt.text = Profile.GetRoomName();
                CreateMenuMaxPlayersValueTxt.Text.text = Profile.GetMaxPlayers().ToString();
                CreateMenuPasswordValueTxt.text = "";
                CreateMenuLanguageValueTxt.GetComponent<DynamicText>().Text.text = Profile.GetLanguage().GetString();
                CreateMenuCardSetsValueTxt.GetComponent<DynamicText>().Text.text = Profile.GetCardSets().Select(x => DataCardSet.CardSets[x]).GetString();
                CreateMenuMaxPointsValueTxt.Text.text = Profile.GetMaxPoints().ToString();
                CreateMenuAllowHotjoinBtn.SetValue(Profile.GetAllowHotjoin());
                CreateMenuLastPointIsDealerBtn.SetValue(Profile.GetAllowLastPointIsDealer());
                CreateMenuEnableVoiceChatBtn.SetValue(Profile.GetAllowVoiceChat());
            }
        }
    }
}
