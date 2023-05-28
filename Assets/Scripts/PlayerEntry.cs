using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
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
    public class PlayerEntry : MonoBehaviour
    {
        public Player Player;
        public DynamicText PointsRoleTxt;
        public DynamicText PlayerNameTxt;
        public GameObject PlayerVolumeContainer;
        public TouchBtn PlayerVolumeDownBtn;
        public TouchBtn PlayerVolumeUpBtn;
        public List<Image> PlayerVolumeLvls;
        public TouchBtn PlayerSwitchMicrophoneBtn;
        public TouchBtn PlayerKickBtn;
        public string PlayerName;
        public bool IsBot;
        public string PlayerId;

        private void Awake()
        {
            Initialize();
        }
        bool initialized = false;
        public void Initialize()
        {
            if (!initialized)
            {
                PointsRoleTxt = this.gameObject.GetChildFromName(nameof(PointsRoleTxt)).GetComponent<DynamicText>();
                PlayerNameTxt = this.gameObject.GetChildFromName(nameof(PlayerNameTxt)).GetComponent<DynamicText>();
                PlayerVolumeContainer = this.gameObject.GetChildFromName(nameof(PlayerVolumeContainer)).gameObject;
                PlayerVolumeDownBtn = PlayerVolumeContainer.GetChildFromName(nameof(PlayerVolumeDownBtn)).GetComponent<TouchBtn>();
                PlayerVolumeUpBtn = PlayerVolumeContainer.GetChildFromName(nameof(PlayerVolumeUpBtn)).GetComponent<TouchBtn>();
                PlayerKickBtn = this.gameObject.GetChildFromName(nameof(PlayerKickBtn)).GetComponent<TouchBtn>();
                PlayerVolumeLvls = new List<Image>();
                for (int i = 1; i <= 9; i++)
                {
                    PlayerVolumeLvls.Add(PlayerVolumeContainer.GetChildFromName("PlayerVolumeLvl" + i.ToString()).GetComponent<Image>());
                }
                PlayerSwitchMicrophoneBtn = this.gameObject.GetChildFromName(nameof(PlayerSwitchMicrophoneBtn)).GetComponent<TouchBtn>();
                PlayerVolumeDownBtn.AddOnClickListener(() => AlterVolume(-0.1f));
                PlayerVolumeUpBtn.AddOnClickListener(() => AlterVolume(0.1f));
                PlayerSwitchMicrophoneBtn.AddOnClickListener(() => PunInterface.Recorder.SwitchMicrophone());
                PlayerKickBtn.AddOnClickListener(() => KickPlayer());
                initialized = true;
            }
        }
        private void FixedUpdate()
        {
            Initialize();
            if (IsBot) { return; }
            bool isPlayingSound = false;
            if (this.Player.IsSelf())
            {
                var recorder = PunInterface.Recorder;
                isPlayingSound = recorder.IsSendingData();
                PlayerSwitchMicrophoneBtn.GetComponent<Image>().SetImage(recorder.TransmitEnabled ? "MicrophoneIcon" : "MicrophoneDisabledIcon");
            }
            else
            {
                var audio = GamePlayersMenu.Instance.GetPlayerAudio(this.Player);
                if (audio.speaker == null) { return; }
                isPlayingSound = audio.speaker.IsPlaying;
                int i = 0;
                foreach (var volumeBar in PlayerVolumeLvls)
                {
                    volumeBar.gameObject.SetActive(audio.source.volume * 10 > i);
                    i++;
                }
                audio.source.minDistance = (bool)GameMenuSharedContainer.Instance?.OthersMuted ? 0 : audio.source.maxDistance; // minDistance = maxDistance -> no attenuation
            }
            PlayerNameTxt.Color(isPlayingSound ? ColorType.Gold : ColorType.Blackened);
        }
        public void AlterVolume(float value)
        {
            var audio = GamePlayersMenu.Instance.GetPlayerAudio(this.Player);
            if (audio.source != null)
            {
                audio.source.volume += value;
            }
        }

        public void SetInfo(string botName, string playerId)
        {
            Initialize();
            this.PlayerName = botName;
            this.Player = null;
            this.IsBot = true;
            this.PlayerId = playerId;
            PlayerNameTxt.Text.text = botName;
            PlayerVolumeContainer.SetActive(false);
            PlayerSwitchMicrophoneBtn.gameObject.SetActive(false);
            PlayerKickBtn.SetActive(GameState.Instance?.GetLocalPlayer()?.IsMasterClient == true);
        }
        public void SetInfo(Player player)
        {
            Initialize();
            this.Player = player;
            PlayerNameTxt.Text.text = player.NickName;
            PlayerVolumeContainer.SetActive(GameState.Instance.Rules.AllowVoiceChat && !player.IsSelf()); // Don't show volume control to self
            PlayerSwitchMicrophoneBtn.gameObject.SetActive(GameState.Instance.Rules.AllowVoiceChat && player.IsSelf());
            PlayerKickBtn.SetActive(GameState.Instance?.GetLocalPlayer().GetID() != player.GetID() && GameState.Instance?.GetLocalPlayer()?.IsMasterClient == true);
        }
        public void UpdateInfo()
        {
            try
            {
                PointsRoleTxt.Text.text = (GameState.Instance.CurrentCardDealer == (IsBot ? PlayerId : this.Player.GetID()) ? "*" : "") + GameState.Instance.PlayersPoints[(IsBot ? PlayerId : this.Player.GetID())].ToString();
            }
            catch (Exception e)
            {
                PointsRoleTxt.Text.text = "?";
            }
        }

        public void KickPlayer()
        {
            if (GameState.Instance?.GetLocalPlayer()?.IsMasterClient == false)
            {
                return;
            }

            if (IsBot)
            {
                GameState.Instance.RemoveAiPlayer(PlayerId);
            }
            else if (Player != null)
            {
                PhotonNetwork.CloseConnection(Player);
            }
        }
    }
}
