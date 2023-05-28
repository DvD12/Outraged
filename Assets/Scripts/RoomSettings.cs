using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outraged
{
    public class RoomSettings // used to be encapsulated by PUN when creating room
    {
        public static string RoomSettingsString = "RoomSettings"; // used by photon's customsettings as key
        public static string GameProgressString = "GameProgress";
        public static string GameState = "GameState";
        public static string AllCards = "AllCards";
        public string Password;
        /// <summary>
        /// The language the host intends to speak with players -- not necessarily the language or area of the card sets chosen
        /// </summary>
        public Language RoomLanguage;
        public List<string> CardSets; // Name identifiers
        public int MaxPoints = 10; // Points to win
        public bool AllowHotjoin;
        public bool AllowVoiceChat;
        public bool LastPointIsDealer;
        public bool AllowCardReplace = true; // TODO allow edit in room creation settings
        public int CardReplacementPointPenalty = -1; // TODO allow edit in room creation settings

        public string HostIP;

        public const int MAX_PLAYERS = 20;
        public const int MAX_POINTS = 30;

        public RoomSettings(string pass, Language lang, List<string> cardSets, int maxPoints, bool allowHotjoin, bool allowVoiceChat, string hostIP)
        {
            this.Password = pass;
            this.CardSets = cardSets;
            this.MaxPoints = maxPoints;
            this.AllowHotjoin = allowHotjoin;
            this.AllowVoiceChat = allowVoiceChat;
            this.HostIP = hostIP;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, formatting: Formatting.None);
        }
    }
}
