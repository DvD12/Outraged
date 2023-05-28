using Photon.Realtime;
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
    public class RoomEntry : MonoBehaviour
    {
        public string RoomNameID;
        public Image RoomEntryHighlightedBackgroundImg;
        public Image RoomEntryHostCountryFlagImg;
        public DynamicText RoomEntryRoomNameTxt;
        public DynamicText RoomEntryPlayersTxt;
        public DynamicText RoomEntryLanguageTxt;
        public DynamicText RoomEntryCardSetsTxt;
        public Image RoomEntryIsVoiceEnabledImg;
        public Image RoomEntryIsPasswordProtectedImg;
        public Image RoomEntryIsInProgressImg;
        private void Awake()
        {
            RoomEntryHighlightedBackgroundImg = this.gameObject.GetChildFromName(nameof(RoomEntryHighlightedBackgroundImg)).GetComponent<Image>();
            RoomEntryHostCountryFlagImg = this.gameObject.GetChildFromName(nameof(RoomEntryHostCountryFlagImg)).GetComponent<Image>();
            RoomEntryRoomNameTxt = this.gameObject.GetChildFromName(nameof(RoomEntryRoomNameTxt)).GetComponent<DynamicText>();
            RoomEntryPlayersTxt = this.gameObject.GetChildFromName(nameof(RoomEntryPlayersTxt)).GetComponent<DynamicText>();
            RoomEntryLanguageTxt = this.gameObject.GetChildFromName(nameof(RoomEntryLanguageTxt)).GetComponent<DynamicText>();
            RoomEntryCardSetsTxt = this.gameObject.GetChildFromName(nameof(RoomEntryCardSetsTxt)).GetComponent<DynamicText>();
            RoomEntryIsVoiceEnabledImg = this.gameObject.GetChildFromName(nameof(RoomEntryIsVoiceEnabledImg)).GetComponent<Image>();
            RoomEntryIsPasswordProtectedImg = this.gameObject.GetChildFromName(nameof(RoomEntryIsPasswordProtectedImg)).GetComponent<Image>();
            RoomEntryIsInProgressImg = this.gameObject.GetChildFromName(nameof(RoomEntryIsInProgressImg)).GetComponent<Image>();
        }

        public void SetInfo(RoomInfo room)
        {
            var roomSettings = room.DeserializeSettings();
            RoomEntryRoomNameTxt.Text.text = room.Name;
            RoomEntryPlayersTxt.Text.text = room.PlayerCount + "/" + room.MaxPlayers;
            RoomEntryLanguageTxt.Text.text = roomSettings.RoomLanguage.GetString();
            RoomEntryCardSetsTxt.Text.text = roomSettings.CardSets.Select(x => DataCardSet.CardSets[x]).GetString();
            RoomEntryCardSetsTxt.Restrict();
            RoomEntryIsVoiceEnabledImg.gameObject.SetActive(roomSettings.AllowVoiceChat);
            RoomEntryIsPasswordProtectedImg.gameObject.SetActive(roomSettings.Password.IsValid());
            RoomEntryIsInProgressImg.gameObject.SetActive(room.GetState() != GameProgress.NotStarted);
        }

        public void SetCallback(Action action)
        {
            RoomEntryHighlightedBackgroundImg.GetComponent<TouchBtn>().AddOnClickListener(() => action?.Invoke());
            RoomEntryHostCountryFlagImg.GetComponent<TouchBtn>().AddOnClickListener(() => action?.Invoke());
            RoomEntryRoomNameTxt.GetComponent<TouchBtn>().AddOnClickListener(() => action?.Invoke());
            RoomEntryPlayersTxt.GetComponent<TouchBtn>().AddOnClickListener(() => action?.Invoke());
            RoomEntryLanguageTxt.GetComponent<TouchBtn>().AddOnClickListener(() => action?.Invoke());
            RoomEntryCardSetsTxt.GetComponent<TouchBtn>().AddOnClickListener(() => action?.Invoke());
            RoomEntryIsVoiceEnabledImg.GetComponent<TouchBtn>().AddOnClickListener(() => action?.Invoke());
            RoomEntryIsPasswordProtectedImg.GetComponent<TouchBtn>().AddOnClickListener(() => action?.Invoke());
            RoomEntryIsInProgressImg.GetComponent<TouchBtn>().AddOnClickListener(() => action?.Invoke());
        }
        public void SetHighlight(bool active)
        {
            RoomEntryHighlightedBackgroundImg.Color(active ? ColorType.Gold : ColorType.Cyan);
            RoomEntryHighlightedBackgroundImg.ColorAlpha(0.66f);
        }
    }
}
