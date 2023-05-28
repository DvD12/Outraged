using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using Photon.Voice.PUN;
using Newtonsoft.Json;

namespace Outraged
{
    public class PunInterface : Photon.Pun.MonoBehaviourPunCallbacks
    {
        public static PunInterface Instance;
        public static Recorder Recorder => Instance.GetComponent<Recorder>();
        public Dictionary<string, RoomInfo> Rooms = new Dictionary<string, RoomInfo>();
        [HideInInspector] public Recorder VoiceRecorder;
        [HideInInspector] public PhotonVoiceNetwork VoiceNetwork;

        public const string PlayerPropertyIP = "PlayerPropertyIP";
        public static string PlayerGUID;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            this.VoiceRecorder = this.GetComponent<Recorder>();
            this.VoiceNetwork = PhotonVoiceNetwork.Instance;
            GetIP();

            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("Trying to join random room");
                // Try to join a random room. If it fails, we'll get notified in OnJoinRandomFailed()
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                Debug.Log("Connecting using settings");
                // We must first and foremost connect to Photon Online Server
                PhotonNetwork.ConnectUsingSettings();
            }
            SetNickname(Profile.GetNickname());
            Profile.SetMicrophone(Profile.GetMicrophone());
        }
        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster()");
            if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(Profile.PlayerPropertyGUID))
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { Profile.PlayerPropertyGUID, Profile.GetPlayerGUID() } }); // If this causes issues try editing PUN's library https://forum.photonengine.com/discussion/comment/50347/#Comment_50347
            }
            this.JoinLobby();

        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("OnDisconnected() " + cause);
        }
        public override void OnJoinRandomFailed(short returnCode, string message)
        {

        }
        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            GameState.Instance.AddPlayer(PhotonNetwork.MasterClient); // Add host AFTER creating room!
            GamePlayersMenu.Instance?.AddPlayer(PhotonNetwork.MasterClient);
        }
        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom(). Now this client is in a room.");
            if (!PhotonNetwork.LocalPlayer.IsMasterClient) // If I created the room I don't need to download the GS
            {
                var newGs = ((string)PhotonNetwork.CurrentRoom.CustomProperties[RoomSettings.GameState]).Deserialize();
                newGs.SetActiveAsInstance();
                DataCardSet.AllCardsCurrentGame = JsonConvert.DeserializeObject<Dictionary<int, CardData>>((string)PhotonNetwork.CurrentRoom.CustomProperties[RoomSettings.AllCards]);
                GameState.Instance.StaticGameRandomGenerator.LoadState(newGs.StaticGameRandomGeneratorState);
                GameState.Instance?.AddPlayer(GameState.Instance.GetLocalPlayer());
                GamePlayersMenu.Instance?.AddPlayer(GameState.Instance.GetLocalPlayer());
            }
            if (GameState.Instance.Rules.AllowVoiceChat)
            {
                GameObject instance = PhotonNetwork.Instantiate("Prefabs/VoiceSpeaker", Vector3.zero, Quaternion.identity);
            }
            Recorder.TransmitEnabled = GameState.Instance.Rules.AllowVoiceChat;
            GameMenuSharedContainer.Instance?.SwitchOthersMuted(false);
            PunInterface.Recorder.SwitchMicrophone(true);
            MenuHandler.Instance.SelectMenu(GameState.Instance.CurrentState == GameProgress.NotStarted ? MenuHandler.MenuState.GamePlayers : MenuHandler.MenuState.GameCards);
        }
        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            GamePlayersMenu.Instance?.ResetData();
            GamePlayersMenu.Instance?.ResetChat();
        }
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            GamePlayersMenu.Instance?.SendSystemChat(TextTag.PlayerJoined.GetText().Replace("#PLAYERNAME", newPlayer.NickName.WrapColor(ColorType.Black)));
            GameState.Instance?.AddPlayer(newPlayer);
            GamePlayersMenu.Instance?.AddPlayer(newPlayer);
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            GamePlayersMenu.Instance?.SendSystemChat(TextTag.PlayerLeft.GetText().Replace("#PLAYERNAME", otherPlayer.NickName.WrapColor(ColorType.Black)));
            GameState.Instance?.RemovePlayer(otherPlayer);
            GamePlayersMenu.Instance?.RemovePlayer(otherPlayer);
        }
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            base.OnRoomListUpdate(roomList);
            this.Rooms.Clear();
            foreach (var room in roomList)
            {
                this.Rooms.Add(room.Name, room);
            }
            Debug.Log("Rooms: " + roomList.Count);
        }
        public void SetNickname(string name)
        {
            PhotonNetwork.NickName = name;
        }
        public void CreateRoom(string name, byte maxPlayers, RoomSettings settings, Action onFailure = null)
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                var gs = new GameState();
                gs.Rules = settings; // ROOM SETTINGS TO GAMESTATE SETTINGS!
                gs.SetRandom((int)DateTime.Now.Ticks);
                gs.GenerateCards(Card.CardType.Question, settings.CardSets);
                gs.GenerateCards(Card.CardType.Answer, settings.CardSets);
                
                DataCardSet.AllCardsCurrentGame = DataCardSet.AllCardsLocal;
                //gs.SaveState();
                bool result = PhotonNetwork.CreateRoom(name, new RoomOptions()
                {
                    MaxPlayers = maxPlayers,
                    CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
                    {
                        { RoomSettings.RoomSettingsString, settings.Serialize() },
                        { RoomSettings.GameProgressString, GameProgress.NotStarted.ToString() },
                        { RoomSettings.GameState, GameState.Instance.Serialize() },
                        { RoomSettings.AllCards, JsonConvert.SerializeObject(gs.AllCards.SelectMany(x => x.Value).ToDictionary(x => x, x => DataCardSet.AllCardsLocal[x])) },
                    },
                    CustomRoomPropertiesForLobby = new string[2] // CustomRoomProperties exposed in lobby
                    {
                        RoomSettings.RoomSettingsString,
                        RoomSettings.GameProgressString
                    }
                });
                Debug.Log("Room " + name + " created: " + result);
            }
            else
            {
                onFailure?.Invoke();
            }
        }
        public void JoinLobby()
        {
            if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
            {
                PhotonNetwork.JoinLobby();
            }
        }
        public void JoinRoom(string name, string psw = "")
        {
            var room = this.Rooms[name];
            var settings = room.DeserializeSettings();
            if (room.GetState() != GameProgress.NotStarted && !settings.AllowHotjoin)
            {
                Debug.Log("Can't join: started match does not allow hotjoin");
            }
            else if (settings.Password.IsValid() && psw != settings.Password)
            {
                Debug.Log("Can't join: invalid password");
            }
            else if (room.GetState() == GameProgress.Ended)
            {
                Debug.Log("Can't join: game ended");
            }
            else
            {
                PhotonNetwork.JoinRoom(name);
            }
        }
        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log("ROOM LEFT!");
        }

        public string GetIP()
        {
            if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(PunInterface.PlayerPropertyIP))
            {
                var webClient = new System.Net.WebClient();
                webClient.DownloadStringCompleted += (o, e) =>
                {
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { PunInterface.PlayerPropertyIP, e.Result } });
                };
                webClient.DownloadStringAsync(new Uri("https://ipinfo.io/ip"));
            }
            return PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(PunInterface.PlayerPropertyIP) ? PhotonNetwork.LocalPlayer.CustomProperties[PunInterface.PlayerPropertyIP].ToString() : "";
        }

        public void ExtrSetGameProgress(GameProgress state)
        {
            photonView.RPC(nameof(SetGameProgress), RpcTarget.All, state);
        }
        [PunRPC]
        public void SetGameProgress(GameProgress state)
        {
            GameState.Instance.SetGameProgress(state);
        }
        public void ExtrSendChat(string msg)
        {
            photonView.RPC(nameof(SendChat), RpcTarget.All, msg);
        }
        [PunRPC]
        public void SendChat(string msg)
        {
            GamePlayersMenu.Instance?.SendChat(msg);
        }

        public void ExtrSetPlayerConfirmation(string id, int[] cards) => photonView.RPC(nameof(SetPlayerConfirmation), RpcTarget.All, id, cards);
        [PunRPC]
        public void SetPlayerConfirmation(string id, int[] cards)
        {
            GameState.Instance.SetPlayerConfirmation(id, cards);
        }

        public void ExtrAlterPlayerPoints(string id, int delta) => photonView.RPC(nameof(AlterPlayerPoints), RpcTarget.All, id, delta);
        [PunRPC]
        public void AlterPlayerPoints(string id, int delta) 
        {
            GameState.Instance.AlterPlayerPoints(id, delta);
        }

        public void ExtrSetAnswersShownIndex(int id, int dealerId)
        {
            photonView.RPC(nameof(SetAnswersShownIndex), RpcTarget.All, id, dealerId);
        }
        [PunRPC]
        public void SetAnswersShownIndex(int id, int dealerId)
        {
            GameState.Instance.PlayerAnswersToReveal.indexToOthers = id;
            bool advance = !SetDealerCardIndex(dealerId);
            if (advance)
            {
                GameState.Instance.SetGameProgress(GameProgress.AwardingPoint);
            }
            else
            {
                GameState.Instance.SaveState();
            }
        }

        public void ExtrSetDealerCardIndex(int value) => photonView.RPC(nameof(SetDealerCardIndex), RpcTarget.All, value);
        [PunRPC]
        public bool SetDealerCardIndex(int value)
        {
            var result = GameCardsMenu.SetIndexToDealer(value);
            return result;
        }

        public void ExtrDealNewAnswers(string playerId) => photonView.RPC(nameof(DealNewAnswers), RpcTarget.All, playerId);
        [PunRPC] public void DealNewAnswers(string playerId)
        {
            GameState.Instance.AlterPlayerPoints(playerId, GameState.Instance.Rules.CardReplacementPointPenalty, true, false);
            GameState.Instance.RemoveCards(playerId);
            GameState.Instance.DealCards(playerId, true);
        }

        public void ExtrAddAiPlayer(string name, string guid) => photonView.RPC(nameof(AddAiPlayer), RpcTarget.All, name, guid);
        [PunRPC] public void AddAiPlayer(string name, string guid)
        {
            GameState.Instance?.AddAiPlayer(name, guid);
        }
    }
}
