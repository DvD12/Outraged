using Photon.Pun;
using Photon.Voice.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Outraged
{
    public class VoiceSpeaker : MonoBehaviour
    {
        [HideInInspector]
        public string OwnerID;
        private void Start()
        {
            this.transform.SetParent(MenuHandler.CanvasBack.transform);
            this.OwnerID = GetComponent<PhotonView>().Owner.GetID();
            this.name = "VoiceSpeaker" + this.OwnerID;
            GamePlayersMenu.PlayerAudios[OwnerID] = GetComponent<Speaker>();
            GamePlayersMenu.PlayerAudioSources[OwnerID] = GetComponent<AudioSource>();
            var rect = this.GetComponent<RectTransform>();
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;
        }
    }
}
