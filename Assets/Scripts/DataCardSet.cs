//using FileHelpers;
using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Outraged
{
    //[DelimitedRecord(";")]
    [System.Serializable]
    public class CardData
    {
        [JsonProperty(Order = 1)]
        //[FieldHidden]
        public Card.CardType Type;
        [JsonProperty(Order = 2)]
        //[FieldOrder(1)]
        public string Text;
        [JsonProperty(Order = 3)]
        //[FieldNullValue(-1)]
        //[FieldOrder(2)]
        //[FieldOptional]
        public int Picks;
        [JsonProperty(Order = 4)]
        //[FieldNullValue("")]
        //[FieldOrder(3)]
        //[FieldOptional]
        public string Author;
        [JsonIgnore]
        //[FieldHidden]
        public bool IsQuestion => Type == Card.CardType.Question;

        public bool ShouldSerializeType() => true;
        public bool ShouldSerializeText() => true;
        public bool ShouldSerializePicks() => Type == Card.CardType.Question;
        public bool ShouldSerializeAuthor() => Author.IsValid();

        public override int GetHashCode()
        {
            return Text.GetHashCode() + Picks.GetHashCode() + IsQuestion.GetHashCode();
        }
    }

    [System.Serializable]
    public class CardSet : System.Object
    {
        public string NameID { get; set; }
        public Dictionary<int, CardData> Cards { get; set; }

        public override string ToString() => NameID + " [" + Cards.Count + "]";
    }
    public class DataCardSet
    {
        public static Dictionary<string, CardSet> CardSets;
        public static Dictionary<string, List<int>> CardSetsToQuestions;
        public static Dictionary<string, List<int>> CardSetsToAnswers;
        public static Dictionary<int, CardData> AllCards => PhotonNetwork.CurrentRoom != null ? AllCardsCurrentGame : AllCardsLocal;
        public static Dictionary<int, CardData> AllCardsLocal;
        public static Dictionary<int, CardData> AllCardsCurrentGame;
        public static int CardsChecksum;
        private static int GlobalCardID = 0;

        public static Action OnLoadCallback = null;
        public static bool IsDoneLoading = false;

        public static Action SetOnLoadCallback(Action callback) => OnLoadCallback = callback;
        public static IEnumerator LoadJson()
        {
            CardSets = new Dictionary<string, CardSet>();
            CardSetsToQuestions = new Dictionary<string, List<int>>();
            CardSetsToAnswers = new Dictionary<string, List<int>>();
            AllCardsLocal = new Dictionary<int, CardData>();

            List<Coroutine> coroutines = new()
            {
                MenuHandler.Instance.StartCoroutine(LoadJsonInternal("https://www.dropbox.com/sh/z30y342jr829hyv/AAA6WG9Ap-zGH2qAOSFYUmaha?dl=1")),
            };

            foreach (var c in coroutines)
                yield return c;

            IsDoneLoading = true;
            OnLoadCallback?.Invoke();
        }

        private static IEnumerator LoadJsonInternal(string url)
        {
            string jsonText = "";
            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            /*if (www.result != UnityWebRequest.Result.Success)
            {
                var path = Path.Combine(Application.dataPath, Path.Combine("Data", "DataCardSet.json"));
                var data = Resources.Load("Data/DataCardSet");
                jsonText = data.ToString();
            }
            else*/
            {
                var data = www.downloadHandler.data;
                using (MemoryStream zipStream = new MemoryStream(data))
                {
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (string.IsNullOrEmpty(entry.Name))
                                continue;
                            using Stream file = entry.Open();
                            using StreamReader reader = new StreamReader(file);
                            jsonText = reader.ReadToEnd();

                            var result = new[] { new { NameID = String.Empty, Cards = new List<CardData>() } }.ToList();
                            var obj = JsonConvert.DeserializeAnonymousType(jsonText, result, DataHeaders.GetFormatting()); // If this fails in standalone player you need to set compatibilty level to .net 4.0 in project's player settings
                            foreach (var cardset in obj)
                            {
                                CardSetsToQuestions[cardset.NameID] = new List<int>();
                                CardSetsToAnswers[cardset.NameID] = new List<int>();

                                var dict = new Dictionary<int, CardData>();
                                foreach (var card in cardset.Cards)
                                {
                                    if (card.IsQuestion)
                                    {
                                        CardSetsToQuestions[cardset.NameID].Add(GlobalCardID);
                                    }
                                    else
                                    {
                                        CardSetsToAnswers[cardset.NameID].Add(GlobalCardID);
                                    }
                                    AllCardsLocal[GlobalCardID] = card;
                                    dict[GlobalCardID] = card;
                                    CardsChecksum ^= card.GetHashCode();
                                    GlobalCardID++;
                                }

                                CardSets[cardset.NameID] = new CardSet() { NameID = cardset.NameID, Cards = dict };
                            }
                        }
                    }
                }
            }

            //string savePath = string.Format("{0}/{1}.pdb", Application.persistentDataPath, file_name);

            // Need to strip UTF-8 BOM character if present or JSON deserializer will complain
            /*var utf8Bom = Encoding.UTF8.GetPreamble();
            var data = www.downloadHandler.data;

            if (data.Take(utf8Bom.Length).ToArray().SequenceEqual(utf8Bom))
                jsonText = Encoding.UTF8.GetString(data.Skip(utf8Bom.Length).ToArray());
            else
                jsonText = Encoding.UTF8.GetString(data);*/
        }
    }
}
