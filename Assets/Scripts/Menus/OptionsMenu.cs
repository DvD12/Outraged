using Photon.Pun;
using Photon.Voice.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Outraged
{
    public class OptionsMenu : MonoBehaviour
    {
        public static OptionsMenu Instance;
        public DynamicText OptionsMenuTitleTxt;
        public DynamicText OptionsMenuNicknameTxt;
        public TMP_InputField OptionsMenuNicknameValueTxt;
        public DynamicText OptionsMenuAppLanguageTxt;
        public TouchBtn OptionsMenuAppLanguageValueTxt;
        public DynamicText OptionsMenuMicrophoneTxt;
        public TouchBtn OptionsMenuMicrophoneValueTxt;
        public Image OptionsMenuMicrophoneLevelBackgroundImg;
        public Image OptionsMenuMicrophoneLevelImg;

        public UI_TxtImgBtn OptionsMenuBackBtn;
        public UI_TxtImgBtn OptionsMenuActionBtn;

        private bool isInitialized = false;
        private void Awake()
        {
            Initialize();
        }
        private void Initialize()
        {
            if (!isInitialized)
            {
                Instance = this;
                OptionsMenuTitleTxt = Helpers.Find<DynamicText>(nameof(OptionsMenuTitleTxt));
                OptionsMenuNicknameTxt = Helpers.Find<DynamicText>(nameof(OptionsMenuNicknameTxt));
                OptionsMenuNicknameValueTxt = Helpers.Find<TMP_InputField>(nameof(OptionsMenuNicknameValueTxt));
                OptionsMenuAppLanguageTxt = Helpers.Find<DynamicText>(nameof(OptionsMenuAppLanguageTxt));
                OptionsMenuAppLanguageValueTxt = Helpers.Find<TouchBtn>(nameof(OptionsMenuAppLanguageValueTxt));
                OptionsMenuMicrophoneTxt = Helpers.Find<DynamicText>(nameof(OptionsMenuMicrophoneTxt));
                OptionsMenuMicrophoneValueTxt = Helpers.Find<TouchBtn>(nameof(OptionsMenuMicrophoneValueTxt));
                OptionsMenuMicrophoneLevelBackgroundImg = Helpers.Find<Image>(nameof(OptionsMenuMicrophoneLevelBackgroundImg));
                OptionsMenuMicrophoneLevelImg = Helpers.Find<Image>(nameof(OptionsMenuMicrophoneLevelImg));
                OptionsMenuBackBtn = Helpers.Find<UI_TxtImgBtn>(nameof(OptionsMenuBackBtn));
                OptionsMenuActionBtn = Helpers.Find<UI_TxtImgBtn>(nameof(OptionsMenuActionBtn));
                isInitialized = true;
            }
        }

        private void FixedUpdate()
        {
            Initialize();
            if (OptionsMenuMicrophoneLevelImg.gameObject.activeInHierarchy)
            {
                OptionsMenuMicrophoneLevelImg.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 80 * PunInterface.Recorder.LevelMeter.CurrentPeakAmp);
            }
        }

        private void Start()
        {
            OptionsMenuTitleTxt.SetText(TextTag.Options, (s) => s.ToUpper());
            OptionsMenuNicknameTxt.SetText(TextTag.Nickname);
            OptionsMenuNicknameValueTxt.OnValueChanged((s) => Profile.SetNickname(s));
            OptionsMenuAppLanguageTxt.SetText(TextTag.AppLanguage);
            OptionsMenuAppLanguageValueTxt.AddOnClickListener(() =>
            {
                MenuHandler.Instance.UI_ChoiceSelectionDialog.gameObject.SetActive(true);
                List<UI_ChoiceSelectionDialog.ChoiceSelection> c = new List<UI_ChoiceSelectionDialog.ChoiceSelection>();
                foreach (var lang in DataLanguages.Dict.Keys)
                {
                    c.Add(new UI_ChoiceSelectionDialog.ChoiceSelection(lang.ToString(), lang.GetString()));
                }
                UI_ChoiceSelectionDialog.Instance.Activate(true, c, TextTag.Language.GetText(), 1, new Action<HashSet<string>>((choices) =>
                {
                    Language newLang = (Language)Enum.Parse(typeof(Language), choices.First());
                    OptionsMenuAppLanguageValueTxt.GetComponent<TextMeshProUGUI>().text = newLang.GetString();
                    Profile.SetAppLanguage(newLang);
                }));
            });
            OptionsMenuMicrophoneTxt.SetText(TextTag.Microphone);
            OptionsMenuBackBtn.SetText(TextTag.Back, (s) => s.ToUpper());
            OptionsMenuBackBtn.GetBtn().AddOnClickListener(() => MenuHandler.Instance.SelectMenu(MenuHandler.PreviousMenu));
            OptionsMenuActionBtn.SetText(TextTag.Leave, (s) => s.ToUpper());
            OptionsMenuActionBtn.GetBtn().AddOnClickListener(() =>
            {
                MenuHandler.Instance.UI_WarningDialog.gameObject.SetActive(true);
                UI_WarningDialog.Instance.Activate(true, TextTag.ConfirmQuit, TextTag.ConfirmQuitDesc, () =>
                {
                    PunInterface.Instance.LeaveRoom();
                    MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.Main);
                }, TextTag.Yes, TextTag.No);
            });
            OptionsMenuMicrophoneValueTxt.AddOnClickListener(() =>
            {
                var microphones = Recorder.PhotonMicrophoneEnumerator;

                MenuHandler.Instance.UI_ChoiceSelectionDialog.gameObject.SetActive(true);
                List<UI_ChoiceSelectionDialog.ChoiceSelection> c = new List<UI_ChoiceSelectionDialog.ChoiceSelection>();
                for (int i = 0; i < microphones.Count; i++)
                {
                    c.Add(new UI_ChoiceSelectionDialog.ChoiceSelection(i.ToString(), microphones.NameAtIndex(i)));
                }
                if (microphones.Count == 0) // Might be the case in Android
                {
                    c.Add(new UI_ChoiceSelectionDialog.ChoiceSelection("0", TextTag.Default.GetText()));
                }
                c.Add(new UI_ChoiceSelectionDialog.ChoiceSelection("-1", TextTag.Disabled.GetText()));
                UI_ChoiceSelectionDialog.Instance.Activate(true, c, TextTag.Microphone.GetText(), 1, new Action<HashSet<string>>((choices) =>
                {
                    int newMic = Int32.Parse(choices.First());
                    OptionsMenuMicrophoneValueTxt.GetComponent<TextMeshProUGUI>().text = microphones.NameAtIndex(newMic);
                    OptionsMenuMicrophoneValueTxt.GetComponent<TextMeshProUGUI>().Restrict();
                    Profile.SetMicrophone(newMic);
                }));
            });
        }

        public void Activate(bool active)
        {
            Initialize();
            this.gameObject.SetActive(active);
            if (active)
            {
                bool inGame = GameMenuSharedContainer.Instance != null && GameMenuSharedContainer.Instance.gameObject.activeInHierarchy;
                OptionsMenuTitleTxt.gameObject.SetActive(!inGame);
                OptionsMenuNicknameTxt.Color(inGame ? ColorType.Grey : ColorType.Gold);
                OptionsMenuNicknameValueTxt.Color(inGame ? ColorType.Grey : ColorType.Gold);
                OptionsMenuNicknameValueTxt.interactable = !PhotonNetwork.InRoom; // Do not change username while ingame
                UpdateUI();

                OptionsMenuBackBtn.GetComponent<Transform>().localPosition = new Vector2(inGame ? 335 : 0, -350);
                OptionsMenuActionBtn.SetActive(inGame);
            }
        }
        public void UpdateUI()
        {
            OptionsMenuNicknameValueTxt.text = Profile.GetNickname();
            OptionsMenuAppLanguageValueTxt.GetComponent<TextMeshProUGUI>().text = Profile.GetAppLanguage().GetString();
            string microphone = "";
            var currentMic = Profile.GetMicrophone();
            for (int i = 0; i < Recorder.PhotonMicrophoneEnumerator.Count; i++)
            {
                if (Recorder.PhotonMicrophoneEnumerator.IDAtIndex(i) == currentMic)
                {
                    microphone = Recorder.PhotonMicrophoneEnumerator.NameAtIndex(i);
                    break;
                }
            }
            OptionsMenuMicrophoneValueTxt.GetComponent<TextMeshProUGUI>().text = microphone;
            OptionsMenuMicrophoneValueTxt.GetComponent<TextMeshProUGUI>().Restrict();
        }
    }
}
