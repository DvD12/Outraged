using Assets.Scripts;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Outraged
{
    public class GamePlayersMenu : MonoBehaviour
    {
        public static GamePlayersMenu Instance;

        public DynamicText PlayersMenuRoomTitleTxt;

        public GameObject PlayersMenuPlayerListElemsList;
        public Scrollbar PlayersMenuPlayerListBoxScrollbar;
        public Image PlayersMenuPlayerListBoxSlidingArrowDown;
        public Image PlayersMenuPlayerListBoxSlidingArrowUp;
        public Image PlayersMenuPlayerListBoxScrollbarHandleImg;

        public GameObject PlayersMenuChatElemsList;
        public DynamicText PlayersMenuChatValueTxt;
        public Image PlayersMenuChatBoxSlidingArrowDown;
        public Image PlayersMenuChatBoxSlidingArrowUp;
        public Scrollbar PlayersMenuChatBoxScrollbar;
        public Image PlayersMenuChatBoxScrollbarHandleImg;

        public TMP_InputField PlayersMenuChatInputTxt;
        public UI_TxtImgBtn PlayersMenuChatSendBtn;

        public TouchBtn PlayersMenuAddBotBtn;
        public UI_TxtImgBtn PlayersMenuActionBtn;
        public UI_TxtImgBtn PlayersMenuLeaveBtn;

        public Dictionary<string, PlayerEntry> PlayerEntries = new Dictionary<string, PlayerEntry>();
        public static Dictionary<string, AudioSource> PlayerAudioSources = new Dictionary<string, AudioSource>();
        public static Dictionary<string, Speaker> PlayerAudios = new Dictionary<string, Speaker>();
        public List<string> ChatLog = new List<string>();

        public (AudioSource source, Speaker speaker) GetPlayerAudio(string id)
        {
            return PlayerAudios.ContainsKey(id) ? (PlayerAudioSources[id], PlayerAudios[id]) : (null , null);
        }
        public (AudioSource source, Speaker speaker) GetPlayerAudio(Player player)
        {
            return GetPlayerAudio(player.GetID());
        }

        private void Awake()
        {
            Instance = this;
            PlayersMenuActionBtn = Helpers.Find<UI_TxtImgBtn>(nameof(PlayersMenuActionBtn));
            PlayersMenuLeaveBtn = Helpers.Find<UI_TxtImgBtn>(nameof(PlayersMenuLeaveBtn));
            PlayersMenuRoomTitleTxt = Helpers.Find<DynamicText>(nameof(PlayersMenuRoomTitleTxt));
            PlayersMenuPlayerListElemsList = Helpers.Find<GameObject>(nameof(PlayersMenuPlayerListElemsList));
            PlayersMenuPlayerListBoxScrollbar = Helpers.Find<Scrollbar>(nameof(PlayersMenuPlayerListBoxScrollbar));
            PlayersMenuPlayerListBoxSlidingArrowDown = Helpers.Find<Image>(nameof(PlayersMenuPlayerListBoxSlidingArrowDown));
            PlayersMenuPlayerListBoxSlidingArrowUp = Helpers.Find<Image>(nameof(PlayersMenuPlayerListBoxSlidingArrowUp));
            PlayersMenuPlayerListBoxScrollbarHandleImg = Helpers.Find<Image>(nameof(PlayersMenuPlayerListBoxScrollbarHandleImg));
            PlayersMenuChatElemsList = Helpers.Find<GameObject>(nameof(PlayersMenuChatElemsList));
            PlayersMenuChatValueTxt = Helpers.Find<DynamicText>(nameof(PlayersMenuChatValueTxt));
            PlayersMenuChatBoxSlidingArrowDown = Helpers.Find<Image>(nameof(PlayersMenuChatBoxSlidingArrowDown));
            PlayersMenuChatBoxSlidingArrowUp = Helpers.Find<Image>(nameof(PlayersMenuChatBoxSlidingArrowUp));
            PlayersMenuChatBoxScrollbar = Helpers.Find<Scrollbar>(nameof(PlayersMenuChatBoxScrollbar));
            PlayersMenuChatBoxScrollbarHandleImg = Helpers.Find<Image>(nameof(PlayersMenuChatBoxScrollbarHandleImg));
            PlayersMenuChatInputTxt = Helpers.Find<TMP_InputField>(nameof(PlayersMenuChatInputTxt));
            PlayersMenuChatSendBtn = Helpers.Find<UI_TxtImgBtn>(nameof(PlayersMenuChatSendBtn));
            PlayersMenuAddBotBtn = Helpers.Find<TouchBtn>(nameof(PlayersMenuAddBotBtn));
            PlayerAudios = new Dictionary<string, Speaker>();

            PlayersMenuAddBotBtn.AddOnClickListener(() => PunInterface.Instance?.ExtrAddAiPlayer(AiManager.GetNewAiName(), Guid.NewGuid().ToString()));
            PlayersMenuChatSendBtn.GetBtn().AddOnClickListener(() => SendChat());
            PlayersMenuLeaveBtn.GetBtn().AddOnClickListener(() =>
            {
                if (GameState.Instance.CurrentState != GameProgress.NotStarted && GameState.Instance.CurrentState != GameProgress.Ended)
                {
                    MenuHandler.Instance.UI_WarningDialog.gameObject.SetActive(true);
                    UI_WarningDialog.Instance.Activate(true, TextTag.ConfirmQuit, TextTag.ConfirmQuitDesc, () =>
                    {
                        PunInterface.Instance.LeaveRoom();
                        MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.Main);
                    }, TextTag.Yes, TextTag.No);
                }

            });
            PlayersMenuActionBtn.GetBtn().AddOnClickListener(() =>
            {
                switch (GameState.Instance.CurrentState)
                {
                    case GameProgress.NotStarted:
                        if (PhotonNetwork.IsMasterClient)
                        {
                            if (GameState.Instance.PlayerCount > 1)
                            {
                                PunInterface.Instance.ExtrSetGameProgress(GameProgress.ReadingQuestion);
                            }
                        }
                        break;
                    default:
                        MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.GameCards);
                        break;
                }
            });
        }
        private void Update()
        {
            if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
            {
                this.SendChat();
            }
            if (Input.GetKey(KeyCode.Tab))
            {
                if (PlayersMenuChatInputTxt.gameObject.activeInHierarchy) { PlayersMenuChatInputTxt.ActivateInputField(); }
            }
        }

        public void Activate(bool active)
        {
            this.gameObject.SetActive(active);
            if (active)
            {
                UpdatePlayers();
                UpdateChat();
                UpdateActionBtnUI();
                PlayersMenuAddBotBtn.SetActive(GameState.Instance?.GetLocalPlayer()?.IsMasterClient == true);
            }
        }
        public void AddAiPlayer(string name, string guid)
        {
            if (!PlayerEntries.ContainsKey(guid))
            {
                var prefab = Resources.Load("Prefabs/PlayerEntry") as GameObject;
                var entry = Instantiate(prefab, PlayersMenuPlayerListElemsList.transform).GetComponent<PlayerEntry>();
                PlayerEntries[guid] = entry;
                entry.SetInfo(name, guid);
                entry.UpdateInfo();
                UpdateActionBtnUI();
            }
        }
        public void AddPlayer(Player player)
        {
            if (player == null) { return; }
            if (!PlayerEntries.ContainsKey(player.GetID()))
            {
                var prefab = Resources.Load("Prefabs/PlayerEntry") as GameObject;
                var entry = Instantiate(prefab, PlayersMenuPlayerListElemsList.transform).GetComponent<PlayerEntry>();
                PlayerEntries[player.GetID()] = entry;
                entry.SetInfo(player);
                entry.UpdateInfo();
                UpdateActionBtnUI();
            }
        }
        public void RemoveAiPlayer(string guid)
        {
            if (PlayerEntries.ContainsKey(guid))
            {
                var obj = PlayerEntries[guid];
                GameObject.Destroy(obj.gameObject);
                PlayerEntries.Remove(guid);
                UpdateActionBtnUI();
            }
        }
        public void RemovePlayer(Player player)
        {
            if (player == null) { return; }
            if (PlayerEntries.ContainsKey(player.GetID()))
            {
                var id = player.GetID();
                var obj = PlayerEntries[id];
                GameObject.Destroy(obj.gameObject);
                PlayerEntries.Remove(id);
                UpdateActionBtnUI();
            }
        }
        public void UpdatePlayers()
        {
            foreach (var player in GameState.Instance.PlayerNicknames.Keys)
            {
                if (!PlayerEntries.ContainsKey(player))
                {
                    AddPlayer(GameState.Instance.GetPlayer(player));
                }
                if (PlayerEntries.ContainsKey(player)) { PlayerEntries[player].UpdateInfo(); }
            }
        }
        public void ResetData()
        {
            PlayersMenuPlayerListElemsList.DestroyChildren();
            PlayerEntries.Clear();
        }
        public void SendChat()
        {
            if (!PlayersMenuChatInputTxt.text.IsValid()) { return; }
            if (PlayersMenuChatInputTxt.gameObject.activeInHierarchy) { PlayersMenuChatInputTxt.ActivateInputField(); }
            string msg = "[" + GameState.Instance.GetLocalPlayer().NickName + "]: " + PlayersMenuChatInputTxt.text;
            PlayersMenuChatInputTxt.text = "";
            PunInterface.Instance.ExtrSendChat(msg);
        }
        public void SendChat(string msg)
        {
            ChatLog.Add(msg);
            GameMenuSharedContainer.Instance?.SetTitleTxt(msg);
            UpdateChat();
        }
        public void SendSystemChat(string msg)
        {
            msg = ("***" + msg + "***").WrapColor(ColorType.Red);
            SendChat(msg);
        }
        public void UpdateChat()
        {
            string result = "";
            for (int i = Math.Max(0, ChatLog.Count - 30); i < ChatLog.Count; i++)
            {
                result += ChatLog[i] + Environment.NewLine;
            }
            PlayersMenuChatValueTxt.Text.text = result;
            PlayersMenuChatBoxScrollbar.ScrollToEnd(true);
        }
        public void ResetChat()
        {
            ChatLog.Clear();
        }
        public void UpdateActionBtnUI()
        {
            switch (GameState.Instance.CurrentState)
            {
                case GameProgress.NotStarted:
                    PlayersMenuActionBtn.SetText(PhotonNetwork.IsMasterClient ? TextTag.Start : TextTag.Waiting, (s) => s.ToUpper());
                    PlayersMenuActionBtn.ColorBackground(PhotonNetwork.IsMasterClient && GameState.Instance.PlayerCount > 1 ? ColorType.Gold : ColorType.Grey);
                    break;
                default:
                    PlayersMenuActionBtn.SetText(TextTag.Back, (s) => s.ToUpper());
                    PlayersMenuActionBtn.ColorBackground(ColorType.Gold);
                    break;
            }
        }
    }
}
