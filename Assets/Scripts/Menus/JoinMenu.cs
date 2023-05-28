using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Outraged
{
    public class JoinMenu : MonoBehaviour
    {
        public static JoinMenu Instance;
        public static DynamicText JoinMenuTitleTxt;
        public static TouchBtn RoomTableRoomNameTxt;
        public static TouchBtn RoomTablePlayersImg;
        public static TouchBtn RoomTableLanguageTxt;
        public static TouchBtn RoomTableCardSetsTxt;
        public static TouchBtn RoomTableVoiceEnabledImg;
        public static TouchBtn RoomTableIsPasswordProtectedImg;
        public static TouchBtn RoomTableIsInProgressTxt;
        public GameObject RoomTableElemsList;
        public Scrollbar RoomTableBoxScrollbar;
        public Image RoomTableBoxSlidingArrowDown;
        public Image RoomTableBoxSlidingArrowUp;
        public Image RoomTableBoxScrollbarHandleImg;

        public UI_TxtImgBtn JoinMenuBackBtn;
        public UI_TxtImgBtn JoinMenuRefreshBtn;
        public UI_TxtImgBtn JoinMenuJoinBtn;

        public string CurrentlySelectedValue;
        private Dictionary<RoomEntry, string> Entries = new Dictionary<RoomEntry, string>();
        private void Awake()
        {
            Instance = this;
            JoinMenuTitleTxt = Helpers.Find<DynamicText>(nameof(JoinMenuTitleTxt));
            RoomTableRoomNameTxt = Helpers.Find<TouchBtn>(nameof(RoomTableRoomNameTxt));
            RoomTablePlayersImg = Helpers.Find<TouchBtn>(nameof(RoomTablePlayersImg));
            RoomTableLanguageTxt = Helpers.Find<TouchBtn>(nameof(RoomTableLanguageTxt));
            RoomTableCardSetsTxt = Helpers.Find<TouchBtn>(nameof(RoomTableCardSetsTxt));
            RoomTableVoiceEnabledImg = Helpers.Find<TouchBtn>(nameof(RoomTableVoiceEnabledImg));
            RoomTableIsPasswordProtectedImg = Helpers.Find<TouchBtn>(nameof(RoomTableIsPasswordProtectedImg));
            RoomTableIsInProgressTxt = Helpers.Find<TouchBtn>(nameof(RoomTableIsInProgressTxt));
            RoomTableElemsList = Helpers.Find<GameObject>(nameof(RoomTableElemsList));
            RoomTableBoxScrollbar = Helpers.Find<Scrollbar>(nameof(RoomTableBoxScrollbar));
            RoomTableBoxSlidingArrowDown = Helpers.Find<Image>(nameof(RoomTableBoxSlidingArrowDown));
            RoomTableBoxSlidingArrowUp = Helpers.Find<Image>(nameof(RoomTableBoxSlidingArrowUp));
            RoomTableBoxScrollbarHandleImg = Helpers.Find<Image>(nameof(RoomTableBoxScrollbarHandleImg));
            JoinMenuBackBtn = Helpers.Find<UI_TxtImgBtn>(nameof(JoinMenuBackBtn));
            JoinMenuRefreshBtn = Helpers.Find<UI_TxtImgBtn>(nameof(JoinMenuRefreshBtn));
            JoinMenuJoinBtn = Helpers.Find<UI_TxtImgBtn>(nameof(JoinMenuJoinBtn));
        }

        private void Start()
        {
            JoinMenuBackBtn.GetBtn().AddOnClickListener(() => MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.Main));
            JoinMenuRefreshBtn.GetBtn().AddOnClickListener(() => CheckRooms());
            JoinMenuJoinBtn.GetBtn().AddOnClickListener(() =>
            {
                if (this.CurrentlySelectedValue.IsValid())
                {
                    Action<string> action = (pw) => PunInterface.Instance.JoinRoom(this.CurrentlySelectedValue, pw);
                    var room = PunInterface.Instance.Rooms[this.CurrentlySelectedValue];
                    if (room != null && !room.RemovedFromList)
                    {
                        var psw = room.DeserializeSettings().Password;
                        if (!psw.IsValid())
                        {
                            action("");
                        }
                        else
                        {
                            MenuHandler.Instance.UI_InputDialog.gameObject.SetActive(true);
                            UI_InputDialog.Instance.Activate(true, "Input password", (s) => s == psw, (s) => action(psw));
                        }
                    }
                    else
                    {
                        CheckRooms();
                    }
                }
            });
        }

        public void CheckRooms()
        {
            PunInterface.Instance.JoinLobby();
            this.Entries.Clear();
            this.RoomTableElemsList.DestroyChildren();
            this.CurrentlySelectedValue = "";
            RefreshJoinBtnUI();
            foreach (var elem in PunInterface.Instance.Rooms.Values)
            {
                if (elem.RemovedFromList) { continue; }
                var settings = elem.DeserializeSettings();
                if (!settings.AllowHotjoin && elem.GetState() != GameProgress.NotStarted) { continue; } // Don't show rooms you can't join
                var prefab = Resources.Load("Prefabs/RoomEntry") as GameObject;
                var entry = Instantiate(prefab, RoomTableElemsList.transform).GetComponent<RoomEntry>();
                entry.RoomNameID = elem.Name;
                this.Entries.Add(entry, entry.RoomNameID);
                entry.SetInfo(elem);
                entry.SetHighlight(false);
                entry.SetCallback(() =>
                {
                    this.CurrentlySelectedValue = elem.Name;
                    entry.SetHighlight(true);
                    this.Entries.Keys.ForEach(x => { if (x != entry) { x.SetHighlight(false); } });
                    RefreshJoinBtnUI();
                });
            }
            RefreshJoinBtnUI();
        }

        public void Activate(bool active)
        {
            this.gameObject.SetActive(active);
            if (active)
            {
                PunInterface.Instance.JoinLobby();
                // UI
                CheckRooms();
            }
        }

        public void RefreshJoinBtnUI()
        {
            this.JoinMenuJoinBtn.ColorBackground(String.IsNullOrEmpty(this.CurrentlySelectedValue) ? ColorType.Grey : ColorType.Gold);
        }
    }
}
