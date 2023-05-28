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
    public class GameMenuSharedContainer : MonoBehaviour
    {
        public static GameMenuSharedContainer Instance;
        public TouchBtn GameMenuSharedMuteOthersBtn;
        public Image GameMenuSharedMuteOthersImg;
        public TouchBtn GameMenuSharedMuteSelfBtn;
        public Image GameMenuSharedMuteSelfImg;
        public Image GameMenuSharedMuteSelfBorderImg;
        public TouchBtn GameMenuSharedGamePlayersBtn;
        public Image GameMenuSharedGamePlayersBorderImg;
        public Image GameMenuSharedTitleBackgroundImg;
        public DynamicText GameMenuSharedTitleTxt;

        public bool OthersMuted = false;
        private Queue<string> MessageQueue = new Queue<string>();

        private void Awake()
        {
            Instance = this;

            GameMenuSharedMuteOthersBtn = Helpers.Find<TouchBtn>(nameof(GameMenuSharedMuteOthersBtn));
            GameMenuSharedMuteOthersImg = Helpers.Find<Image>(nameof(GameMenuSharedMuteOthersImg));
            GameMenuSharedMuteSelfBtn = Helpers.Find<TouchBtn>(nameof(GameMenuSharedMuteSelfBtn));
            GameMenuSharedMuteSelfImg = Helpers.Find<Image>(nameof(GameMenuSharedMuteSelfImg));
            GameMenuSharedMuteSelfBorderImg = Helpers.Find<Image>(nameof(GameMenuSharedMuteSelfBorderImg));
            GameMenuSharedGamePlayersBtn = Helpers.Find<TouchBtn>(nameof(GameMenuSharedGamePlayersBtn));
            GameMenuSharedGamePlayersBorderImg = Helpers.Find<Image>(nameof(GameMenuSharedGamePlayersBorderImg));
            GameMenuSharedTitleBackgroundImg = Helpers.Find<Image>(nameof(GameMenuSharedTitleBackgroundImg));
            GameMenuSharedTitleTxt = Helpers.Find<DynamicText>(nameof(GameMenuSharedTitleTxt));

            GameMenuSharedMuteOthersBtn.AddOnClickListener(() => SwitchOthersMuted());
            GameMenuSharedMuteSelfBtn.AddOnClickListener(() => PunInterface.Recorder.SwitchMicrophone());
            GameMenuSharedGamePlayersBtn.AddOnClickListener(() => MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.GamePlayers));
        }

        private void FixedUpdate()
        {
            CheckTitleQueue();
            if (this.gameObject.activeInHierarchy)
            {
                bool isPlayingSound = GameState.Instance != null && GamePlayersMenu.Instance != null && GameState.Instance.AllPlayers.Any(x => x.Key != GameState.Instance.GetLocalPlayerID() && GamePlayersMenu.Instance.GetPlayerAudio(x.Key).speaker != null && GamePlayersMenu.Instance.GetPlayerAudio(x.Key).speaker.IsPlaying);
                GameMenuSharedGamePlayersBorderImg.gameObject.SetActive(isPlayingSound);

                var recorder = PunInterface.Recorder;
                isPlayingSound = recorder.IsSendingData();
                GameMenuSharedMuteSelfBorderImg.gameObject.SetActive(isPlayingSound);
            }
        }

        public void SwitchOthersMuted(bool? val = null)
        {
            OthersMuted = GameState.Instance.Rules.AllowVoiceChat && (val.HasValue ? val.Value : !OthersMuted);
            UpdateMuteOthers();
        }

        public void UpdateMuteSelf()
        {
            var recorder = PunInterface.Recorder;
            GameMenuSharedMuteSelfImg.SetImage(recorder.TransmitEnabled ? "MicrophoneIcon" : "MicrophoneDisabledIcon");
        }
        public void UpdateMuteOthers()
        {
            GameMenuSharedMuteOthersImg.SetImage(OthersMuted ? "SpeakerDisabledIcon" : "SpeakerIcon");
        }

        public void SetTitleTxt(string s)
        {
            MessageQueue.Enqueue(s);
        }
        private DateTime LastMsgTime = DateTime.Now;
        private void CheckTitleQueue()
        {
            if (LastMsgTime > DateTime.Now || MessageQueue.Count == 0) { return; }
            else
            {
                GameMenuSharedTitleTxt.SetText(MessageQueue.Dequeue());
                GameMenuSharedTitleTxt.Restrict();
                LastMsgTime = DateTime.Now.AddSeconds(2);
            }
        }
        public void Color()
        {
            GameMenuSharedGamePlayersBtn.Color(MenuHandler.CurrentMenu == MenuHandler.MenuState.GamePlayers ? ColorType.Cyan : ColorType.Gold);
        }
    }
}
