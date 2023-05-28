using Photon.Voice.Unity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Outraged
{
    public class Profile
    {
        public static string PlayerName = "PlayerName";
        public static string RoomName = "RoomName";
        public static string MaxPlayers = "MaxPlayers";
        public static string Language = "Language";
        public static string CardSets = "CardSets";
        public static string MaxPoints = "MaxPoints";
        public static string AllowHotjoin = "AllowHotjoin";
        public static string AllowLastPointIsDealer = "AllowLastPointIsDealer";
        public static string AllowVoiceChat = "AllowVoiceChat";
        public const string PlayerPropertyGUID = "PlayerPropertyGUID";

        public static string AppLanguage = "AppLanguage";
        public static string Microphone = "Microphone";
        public static void SetNickname(string name)
        {
            PunInterface.Instance.SetNickname(name);
            PlayerPrefs.SetString(PlayerName, name);
        }
        public static void SetRoomName(string name) => PlayerPrefs.SetString(RoomName, name);
        public static void SetMaxPlayers(int num) => PlayerPrefs.SetInt(MaxPlayers, num);
        public static void SetLanguage(Language language) => PlayerPrefs.SetString(Language, language.ToString());
        public static void SetCardSets(List<string> cardSets) => PlayerPrefs.SetString(CardSets, cardSets.Aggregate((x, y) => x + "," + y));
        public static void SetMaxPoints(int num) => PlayerPrefs.SetInt(MaxPoints, num);
        public static void SetAllowHotjoin(bool value) => PlayerPrefs.SetString(AllowHotjoin, value.ToString());
        public static void SetAllowLastPointDealer(bool value) => PlayerPrefs.SetString(AllowLastPointIsDealer, value.ToString());
        public static void SetAllowVoiceChat(bool value) => PlayerPrefs.SetString(AllowVoiceChat, value.ToString());
        public static void SetAppLanguage(Language language)
        {
            PlayerPrefs.SetString(AppLanguage, language.ToString());
        }
        public static void SetMicrophone(int id)
        {
            PlayerPrefs.SetInt(Microphone, id);
            PunInterface.Recorder.StopRecording();
            if (id != -1)
            {
                PunInterface.Recorder.PhotonMicrophoneDeviceId = id;
                PunInterface.Recorder.StartRecording();
            }
        }
        public static string GetNickname() => PlayerPrefs.GetString(PlayerName, GetRandomName());
        public static string GetRoomName() => PlayerPrefs.GetString(RoomName, "New Room");
        public static int GetMaxPlayers() => PlayerPrefs.GetInt(MaxPlayers, 10);
        public static Language GetLanguage() => (Language)Enum.Parse(typeof(Language), PlayerPrefs.GetString(Language, Outraged.Language.English.ToString()));
        public static string[] GetCardSets() => PlayerPrefs.GetString(CardSets, DataCardSet.CardSets.Keys.First()).Split(',');
        public static int GetMaxPoints() => PlayerPrefs.GetInt(MaxPoints, 10);
        public static bool GetAllowHotjoin() => bool.Parse(PlayerPrefs.GetString(AllowHotjoin, "true"));
        public static bool GetAllowLastPointIsDealer() => bool.Parse(PlayerPrefs.GetString(AllowLastPointIsDealer, "false"));
        public static bool GetAllowVoiceChat() => bool.Parse(PlayerPrefs.GetString(AllowVoiceChat, "true"));
        public static string GetPlayerGUID()
        {
            if (!PlayerPrefs.HasKey(PlayerPropertyGUID))
            {
                PlayerPrefs.SetString(PlayerPropertyGUID, Guid.NewGuid().ToString());
            }
            string post = Application.dataPath; // Not part of player GUID so as to allow multiple instances of the game to run on the same device (otherwise they'd all share the same GUID)
            return PlayerPrefs.GetString(PlayerPropertyGUID) + post;
        }
        public static Language GetAppLanguage()
        {
            Func<Language> GetDefaultLanguage = () =>
            {
                string lang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
                switch (lang)
                {
                    case "en": return Outraged.Language.English;
                    case "es": return Outraged.Language.Spanish;
                    case "de": return Outraged.Language.German;
                    case "fr": return Outraged.Language.French;
                    case "it": return Outraged.Language.Italian;
                    default: return DataLanguages.FallbackLanguage;
                }
            };
            if (!PlayerPrefs.HasKey(AppLanguage))
            {
                SetAppLanguage(GetDefaultLanguage());
            }
            return (Language)Enum.Parse(typeof(Language), PlayerPrefs.GetString(AppLanguage));
        }
        public static int GetMicrophone()
        {
            var microphones = Recorder.PhotonMicrophoneEnumerator;
            if (!PlayerPrefs.HasKey(Microphone) || !microphones.IDIsValid(PlayerPrefs.GetInt(Microphone)))
            {
                if (microphones.Count > 0)
                {
                    SetMicrophone(microphones.IDAtIndex(0));
                }
                else { SetMicrophone(0); }
            }
            return PlayerPrefs.GetInt(Microphone);
        }

        public static void ClearPrefs()
        {
            PlayerPrefs.DeleteKey(PlayerPropertyGUID);
        }

        private static List<string> Adjectives = new List<string>()
        {
            "Adorable", "All-powerful", "Amicable", "Annoyed", "Anxious", "Atomic", "Average", "Bewildered", "Cautious", "Cheerful", "Clumsy", "Defiant", "Disgusted", "Dizzy", "Doubtful", "Drab", "Elegant", "Elongated", "Energetic", "Enthusiastic", "Eternal", "Explosive", "Extremist", "Exuberant", "Faithful", "Fancy", "Fashionable", "Gargantuan", "Grumpy", "Grotesque", "Holistic", "Homely", "Honest", "Hungry", "Immense", "Incredible", "Inexpensive", "Introverted", "Itchy", "Jealous", "Jittery", "Jolly", "Lazy", "Magnificent", "Majestic", "Moderate", "Motionless", "Muddy", "Nutty", "Obnoxious", "Outrageous", "Prickly", "Proud", "Repulsive", "Shiny", "Silly", "Sleepy", "Splendid", "Stinky", "Stormy", "Sturdy", "Super", "Tame", "Tasty", "Thoughtful", "Timid", "Tipsy", "Unquestionable", "Unusual", "Vast", "Wandering", "Weary", "Wicked", "Wild", "Witty", "Zany", "Zealous"
        };
        private static List<string> Nouns = new List<string>()
        {
            "Aardvark", "Affenpinscher", "Ainu", "Akita", "Albatross", "Alligator", "Anaconda", "Ant", "Ape", "Axolotl", "Baboon", "Badger", "Barracuda", "Beagle", "Bear", "Bee", "Beetle", "Bobcat", "Bonobo", "Buffalo", "Bulldog", "Bullfrog", "Butterfly", "Camel", "Caribou", "Cat", "Chihuahua", "Chimpanzee", "Cicada", "Cow", "Crocodile", "Crow", "Cuttlefish", "Deer", "Dodo", "Dog", "Duck", "Eagle", "Emu", "Falcon", "Fox", "Frog", "Gibbon", "Giraffe", "Goat", "Hippo", "Horse", "Hyena", "Ibis", "Jackal", "Jellyfish", "Kangaroo", "Lemming", "Llama", "Lynx", "Macaque", "Meerkat", "Mongoose", "Moose", "Mouse", "Muskrat", "Needlefish", "Ocelot", "Otter", "Owl", "Ox", "Oyster", "Pangolin", "Panther", "Pomeranian", "Python", "Quetzal", "Rabbit", "Raccoon", "Rooster", "Salmon", "Seagull", "Seal", "Swan", "Termite", "Tiger", "Tuna", "Viper", "Weasel", "Wolf", "Yak", "Zebra"
        };

        private static string GetRandomName() => $"{Adjectives.GetRandomElement()} {Nouns.GetRandomElement()}";
    }
}
