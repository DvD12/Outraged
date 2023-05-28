using Outraged;
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
    public class UI_PointAwardedDialog : MonoBehaviour
    {
        public static UI_PointAwardedDialog Instance;
        public TouchBtn PointAwardedDialogBackgroundImg;
        public Image PointAwardedDialogTextBackgroundImg;
        public TextMeshProUGUI PointAwardedDialogNameTxt;
        public TextMeshProUGUI PointAwardedDialogDescTxt;
        public TextMeshProUGUI PointAwardedDialogMatchPoint1Txt;
        public TextMeshProUGUI PointAwardedDialogMatchPoint2Txt;
        AnimationChain SlideAnimation;
        private Vector3 NameTxtStartPos = new Vector3(-2100, 30);
        private Vector3 NameTxtMidPos = new Vector3(0, 30);
        private Vector3 NameTxtMidSlowPos = new Vector3(200, 30);
        private Vector3 NameTxtEndPos = new Vector3(2100, 30);
        private Vector3 DescTxtStartPos = new Vector3(2100, -30);
        private Vector3 DescTxtMidPos = new Vector3(0, -30);
        private Vector3 DescTxtMidSlowPos = new Vector3(-200, -30);
        private Vector3 DescTxtEndPos = new Vector3(-2100, -30);
        private Vector3 MatchPoint1TxtStartPos = new Vector3(2100, 110);
        private Vector3 MatchPoint1TxtMidPos = new Vector3(0, 110);
        private Vector3 MatchPoint1TxtMidSlowPos = new Vector3(-200, 110);
        private Vector3 MatchPoint1TxtEndPos = new Vector3(-2100, 110);
        private Vector3 MatchPoint2TxtStartPos = new Vector3(-2100, -110);
        private Vector3 MatchPoint2TxtMidPos = new Vector3(0, -110);
        private Vector3 MatchPoint2TxtMidSlowPos = new Vector3(200, -110);
        private Vector3 MatchPoint2TxtEndPos = new Vector3(2100, -110);

        private bool IsInitialized = false;
        public void Initialize()
        {
            if (!IsInitialized)
            {
                Instance = this;
                PointAwardedDialogBackgroundImg = Helpers.Find<TouchBtn>(nameof(PointAwardedDialogBackgroundImg));
                PointAwardedDialogTextBackgroundImg = Helpers.Find<Image>(nameof(PointAwardedDialogTextBackgroundImg));
                PointAwardedDialogNameTxt = Helpers.Find<TextMeshProUGUI>(nameof(PointAwardedDialogNameTxt));
                PointAwardedDialogDescTxt = Helpers.Find<TextMeshProUGUI>(nameof(PointAwardedDialogDescTxt));
                PointAwardedDialogMatchPoint1Txt = Helpers.Find<TextMeshProUGUI>(nameof(PointAwardedDialogMatchPoint1Txt));
                PointAwardedDialogMatchPoint2Txt = Helpers.Find<TextMeshProUGUI>(nameof(PointAwardedDialogMatchPoint2Txt));
                PointAwardedDialogBackgroundImg.AddOnClickListener(() => Activate(false));
                SlideAnimation = new AnimationChain(false, () => Activate(false));
                SlideAnimation.Add(900, (start, end, now) =>
                {
                    PointAwardedDialogNameTxt.transform.localPosition = Vector3.Lerp(NameTxtStartPos, NameTxtMidPos, (float)((now - start).TotalMilliseconds / (end - start).TotalMilliseconds));
                    PointAwardedDialogDescTxt.transform.localPosition = Vector3.Lerp(DescTxtStartPos, DescTxtMidPos, (float)((now - start).TotalMilliseconds / (end - start).TotalMilliseconds));
                    PointAwardedDialogMatchPoint1Txt.transform.localPosition = Vector3.Lerp(MatchPoint1TxtStartPos, MatchPoint1TxtMidPos, (float)((now - start).TotalMilliseconds / (end - start).TotalMilliseconds));
                    PointAwardedDialogMatchPoint2Txt.transform.localPosition = Vector3.Lerp(MatchPoint2TxtStartPos, MatchPoint2TxtMidPos, (float)((now - start).TotalMilliseconds / (end - start).TotalMilliseconds));

                }, "StartFast");
                SlideAnimation.Add(4000, (start, end, now) =>
                {
                    PointAwardedDialogNameTxt.transform.localPosition = Vector3.Lerp(NameTxtMidPos, NameTxtMidSlowPos, (float)((now - start).TotalMilliseconds / (end - start).TotalMilliseconds).SmoothStart());
                    PointAwardedDialogDescTxt.transform.localPosition = Vector3.Lerp(DescTxtMidPos, DescTxtMidSlowPos, (float)((now - start).TotalMilliseconds / (end - start).TotalMilliseconds).SmoothStart());
                    PointAwardedDialogMatchPoint1Txt.transform.localPosition = Vector3.Lerp(MatchPoint1TxtMidPos, MatchPoint1TxtMidSlowPos, (float)((now - start).TotalMilliseconds / (end - start).TotalMilliseconds).SmoothStart());
                    PointAwardedDialogMatchPoint2Txt.transform.localPosition = Vector3.Lerp(MatchPoint2TxtMidPos, MatchPoint2TxtMidSlowPos, (float)((now - start).TotalMilliseconds / (end - start).TotalMilliseconds).SmoothStart());
                }, "MidSlow");
                SlideAnimation.Add(900, (start, end, now) =>
                {
                    PointAwardedDialogNameTxt.transform.localPosition = Vector3.Lerp(NameTxtMidSlowPos, NameTxtEndPos, (float)((now - start).TotalMilliseconds / (end - start).TotalMilliseconds).Smooth());
                    PointAwardedDialogDescTxt.transform.localPosition = Vector3.Lerp(DescTxtMidSlowPos, DescTxtEndPos, (float)((now - start).TotalMilliseconds / (end - start).TotalMilliseconds).Smooth());
                    PointAwardedDialogMatchPoint1Txt.transform.localPosition = Vector3.Lerp(MatchPoint1TxtMidSlowPos, MatchPoint1TxtEndPos, (float)((now - start).TotalMilliseconds / (end - start).TotalMilliseconds).Smooth());
                    PointAwardedDialogMatchPoint2Txt.transform.localPosition = Vector3.Lerp(MatchPoint2TxtMidSlowPos, MatchPoint2TxtEndPos, (float)((now - start).TotalMilliseconds / (end - start).TotalMilliseconds).Smooth());
                }, "EndFast");
                IsInitialized = true;
            }
        }

        private void Awake()
        {
            Initialize();
        }
        private void Update()
        {
            SlideAnimation.Update();
        }

        public void Activate(bool active, int delta = 1)
        {
            MenuHandler.Instance.DialogsBackgroundImg.gameObject.SetActive(active);
            this.gameObject.SetActive(active);
            if (active)
            {
                try
                {
                    var players = GameState.Instance.AllPlayers.Where(x => x.Value == true).OrderByDescending(x => GameState.Instance.PlayersPoints[x.Key]).Select(x => new { id = x.Key, name = GameState.Instance.PlayerNicknames[x.Key] });
                    bool IsMatchPoint = GameState.Instance.Rules.MaxPoints - GameState.Instance.PlayersPoints[players.First().id] <= 1;
                    PointAwardedDialogMatchPoint1Txt.gameObject.SetActive(IsMatchPoint);
                    PointAwardedDialogMatchPoint2Txt.gameObject.SetActive(IsMatchPoint);
                    string PlayerName = GameState.Instance.PlayerNicknames[GameState.Instance.LastPointPlayer].WrapColor(ColorType.Red);
                    string PlayerPosition = "";
                    for (int i = 0; i < players.Count(); i++)
                    {
                        if (players.ElementAt(i).id == GameState.Instance.LastPointPlayer)
                        {
                            PlayerPosition = ("#" + (i + 1).ToString()).WrapColor(ColorType.Red);
                            break;
                        }
                    }
                    string SecondPlacePlayerName = players.ElementAt(1).name.WrapColor(ColorType.Red);
                    string SecondPlaceCurrentValue = GameState.Instance.PlayersPoints[players.ElementAt(1).id].ToString().WrapColor(ColorType.Red);
                    int secondPlaceDelta = GameState.Instance.PlayersPoints[players.ElementAt(0).id] - GameState.Instance.PlayersPoints[players.ElementAt(1).id];
                    string SecondPlaceDelta = (secondPlaceDelta).ToString().WrapColor(ColorType.Red);
                    string LastPlacePlayerName = GameState.Instance.PlayerNicknames[players.Last().id].WrapColor(ColorType.Red);
                    string LastPlaceCurrentValue = GameState.Instance.PlayersPoints[players.Last().id].ToString().WrapColor(ColorType.Red);
                    int pointsToWin = GameState.Instance.Rules.MaxPoints - GameState.Instance.PlayersPoints[players.Last().id];
                    string PointsToWin = (pointsToWin).ToString().WrapColor(ColorType.Red);
                    string FirstPlayerName = GameState.Instance.PlayerNicknames[players.First().id].WrapColor(ColorType.Red);
                    string FirstCurrentValue = GameState.Instance.PlayersPoints[players.First().id].ToString().WrapColor(ColorType.Red);
                    string CurrentValue = GameState.Instance.PlayersPoints[GameState.Instance.LastPointPlayer].ToString().WrapColor(ColorType.Red);
                    string PointOrPoints = "";
                    string SecondPlayerPosition = "#2".WrapColor(ColorType.Red);
                    string FirstPlayerPosition = "#1".WrapColor(ColorType.Red);

                    PointAwardedDialogNameTxt.text = TextTag.PointsAwarded.GetText().Replace("#PLAYERPOSITION", PlayerPosition).Replace("#VALUE", delta.ToString().WrapColor(ColorType.Red)).Replace("#POINTORPOINTS", delta == 1 ? TextTag.Point.GetText().ToLower() : TextTag.Points.GetText().ToLower()).Replace("#PLAYERNAME", GameState.Instance.PlayerNicknames[GameState.Instance.LastPointPlayer].WrapColor(ColorType.Red)).Replace("#CURRENTVALUE", GameState.Instance.PlayersPoints[GameState.Instance.LastPointPlayer].ToString().WrapColor(ColorType.Red));
                    TextTag tag;
                    if (GameState.Instance.LastPointPlayer == players.First().id) // scorer is first
                    {
                        if (IsMatchPoint)
                        {
                            int rand = new System.Random(DateTime.Now.Millisecond).Next(1, 3);
                            tag = (TextTag)Enum.Parse(typeof(TextTag), "PointsAwardedMatchPoint" + rand);
                            PointOrPoints = (secondPlaceDelta == 1 ? TextTag.Point.GetText() : TextTag.Points.GetText()).WrapColor(ColorType.Red);
                        }
                        else
                        {
                            int rand = new System.Random(DateTime.Now.Millisecond).Next(1, 13);
                            tag = (TextTag)Enum.Parse(typeof(TextTag), "PointsAwardedWinning" + rand);
                        }
                    }
                    else if (GameState.Instance.LastPointPlayer == players.Last().id) // scorer is last
                    {
                        int rand = new System.Random(DateTime.Now.Millisecond).Next(1, 6);
                        tag = (TextTag)Enum.Parse(typeof(TextTag), "PointsAwardedLoser" + rand);
                        PointOrPoints = (pointsToWin == 1 ? TextTag.Point.GetText() : TextTag.Points.GetText()).WrapColor(ColorType.Red);
                    }
                    else
                    {
                        int rand = new System.Random(DateTime.Now.Millisecond).Next(1, 8);
                        tag = (TextTag)Enum.Parse(typeof(TextTag), "PointsAwardedNormal" + rand);
                    }
                    string text = tag.GetText()
                        .Replace("#PLAYERNAME", PlayerName)
                        .Replace("#PLAYERPOSITION", PlayerPosition)
                        .Replace("#SECONDPLACEPLAYERNAME", SecondPlacePlayerName)
                        .Replace("#SECONDPLACECURRENTVALUE", SecondPlaceCurrentValue)
                        .Replace("#SECONDPLACEDELTA", SecondPlaceDelta)
                        .Replace("#LASTPLACEPLAYERNAME", LastPlacePlayerName)
                        .Replace("#LASTPLACECURRENTVALUE", LastPlaceCurrentValue)
                        .Replace("#POINTSTOWIN", PointsToWin)
                        .Replace("#POINTORPOINTS", PointOrPoints)
                        .Replace("#FIRSTPLAYERNAME", FirstPlayerName)
                        .Replace("#FIRSTCURRENTVALUE", FirstCurrentValue)
                        .Replace("#CURRENTVALUE", CurrentValue)
                        .Replace("#FIRSTPLAYERPOSITION", FirstPlayerPosition)
                        .Replace("#SECONDPLAYERPOSITION", SecondPlayerPosition);
                    PointAwardedDialogDescTxt.text = text;
                    SlideAnimation.Start();
                }
                catch { }
            }
        }
    }
}
