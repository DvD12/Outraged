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
    public class UI_GameEndDialog : MonoBehaviour
    {
        public static UI_GameEndDialog Instance;
        public Image GameEndDialogBackgroundImg;
        public DynamicText GameEndDialogTitleTxt;
        public Image GameEndDialogWinnerNameBackgroundImg;
        public DynamicText GameEndDialogWinnerNameTxt;
        public DynamicText GameEndDialogWinnerPointsTxt;
        public DynamicText GameEndDialogHonourableMentionsTxt;
        public DynamicText GameEndDialogHonourableMentionsNamesTxt;
        public DynamicText GameEndDialogHonourableMentionsPointsTxt;
        public DynamicText GameEndDialogFunAtPartiesTxt;
        public DynamicText GameEndDialogFunAtPartiesNameTxt;
        public DynamicText GameEndDialogFunAtPartiesPointsTxt;
        public UI_TxtImgBtn GameEndDialogOKBtn;

        private bool isInitialized = false;
        private void Awake()
        {
            Initialize();
        }
        public void Initialize()
        {
            if (!isInitialized)
            {
                Instance = this;
                GameEndDialogBackgroundImg = Helpers.Find<Image>(nameof(GameEndDialogBackgroundImg));
                GameEndDialogTitleTxt = Helpers.Find<DynamicText>(nameof(GameEndDialogTitleTxt));
                GameEndDialogWinnerNameBackgroundImg = Helpers.Find<Image>(nameof(GameEndDialogWinnerNameBackgroundImg));
                GameEndDialogWinnerNameTxt = Helpers.Find<DynamicText>(nameof(GameEndDialogWinnerNameTxt));
                GameEndDialogWinnerPointsTxt = Helpers.Find<DynamicText>(nameof(GameEndDialogWinnerPointsTxt));
                GameEndDialogHonourableMentionsTxt = Helpers.Find<DynamicText>(nameof(GameEndDialogHonourableMentionsTxt));
                GameEndDialogHonourableMentionsNamesTxt = Helpers.Find<DynamicText>(nameof(GameEndDialogHonourableMentionsNamesTxt));
                GameEndDialogHonourableMentionsPointsTxt = Helpers.Find<DynamicText>(nameof(GameEndDialogHonourableMentionsPointsTxt));
                GameEndDialogFunAtPartiesTxt = Helpers.Find<DynamicText>(nameof(GameEndDialogFunAtPartiesTxt));
                GameEndDialogFunAtPartiesNameTxt = Helpers.Find<DynamicText>(nameof(GameEndDialogFunAtPartiesNameTxt));
                GameEndDialogFunAtPartiesPointsTxt = Helpers.Find<DynamicText>(nameof(GameEndDialogFunAtPartiesPointsTxt));
                GameEndDialogOKBtn = Helpers.Find<UI_TxtImgBtn>(nameof(GameEndDialogOKBtn));
                isInitialized = true;
            }
        }

        private void Start()
        {
            this.GameEndDialogOKBtn.Resize(252, 97);
        }

        private void OnOKBtnPressed()
        {
            this.Activate(false);
            MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.GamePlayers);
        }

        // Dictionary<string, string> -> elem name -> elem displayed name (generic: can be applied to enums, numbers etc.)
        public void Activate(bool active)
        {
            // Activation logic
            MenuHandler.Instance.DialogsBackgroundImg.gameObject.SetActive(active);
            this.gameObject.SetActive(active);
            if (active)
            {
                var players = GameState.Instance.AllPlayers.Where(x => x.Value == true).OrderByDescending(x => GameState.Instance.PlayersPoints[x.Key]).Select(x => new { id = x.Key, name = GameState.Instance.PlayerNicknames[x.Key] });
                var winner = players.First();
                var last = players.Last();

                GameEndDialogTitleTxt.SetText(TextTag.WeHaveAWinner, (s) => s.ToUpper());
                GameEndDialogHonourableMentionsTxt.SetText(TextTag.HonourableMentions);
                GameEndDialogFunAtPartiesTxt.SetText(TextTag.FunAtParties);

                GameEndDialogWinnerNameTxt.Text.text = winner.name;
                GameEndDialogWinnerPointsTxt.Text.text = GameState.Instance.PlayersPoints[winner.id].ToString() + " " + (GameState.Instance.PlayersPoints[winner.id] == 1 ? DataLanguages.GetText(TextTag.Point) : DataLanguages.GetText(TextTag.Points));
                GameEndDialogHonourableMentionsNamesTxt.Text.text = "";
                GameEndDialogHonourableMentionsPointsTxt.Text.text = "";
                int i = 0;
                foreach (var player in players)
                {
                    if (i > 2) { break; }
                    if (player.id == winner.id || player.id == last.id) { continue; }
                    GameEndDialogHonourableMentionsNamesTxt.Text.text += player.name + Environment.NewLine;
                    GameEndDialogHonourableMentionsPointsTxt.Text.text += GameState.Instance.PlayersPoints[player.id] + " " + (GameState.Instance.PlayersPoints[player.id] == 1 ? DataLanguages.GetText(TextTag.Point) : DataLanguages.GetText(TextTag.Points)) + Environment.NewLine;
                    i++;
                }
                GameEndDialogFunAtPartiesNameTxt.Text.text = last.name;
                GameEndDialogFunAtPartiesPointsTxt.Text.text = GameState.Instance.PlayersPoints[last.id] + " " + (GameState.Instance.PlayersPoints[last.id] == 1 ? DataLanguages.GetText(TextTag.Point) : DataLanguages.GetText(TextTag.Points));

                if (GameState.Instance.GetLocalPlayerID() != winner.id)
                {
                    GameEndDialogOKBtn.SetText(TextTag.Okaydots, (s) => s.ToUpper());
                }
                else
                {
                    GameEndDialogOKBtn.SetText(TextTag.Yay, (s) => s.ToUpper());
                }
                GameEndDialogOKBtn.GetBtn().AddOnClickListener(() => OnOKBtnPressed());
                Color();
            }
        }
        public void Color()
        {
            GameEndDialogOKBtn.ColorBackground(ColorType.Gold);
        }
    }
}
