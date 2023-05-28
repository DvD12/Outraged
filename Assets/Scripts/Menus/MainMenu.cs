using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Outraged
{
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance;
        public static UI_TxtImgBtn CreateBtn;
        public static UI_TxtImgBtn JoinBtn;
        public static UI_TxtImgBtn HowToBtn;
        public static UI_TxtImgBtn CardSetsBtn;
        public static UI_TxtImgBtn QuitBtn;
        public static TouchBtn TipBackgroundImg;
        public static DynamicText TipBackgroundTxt;

        private void Awake()
        {
            Instance = this;
            CreateBtn = Helpers.Find<UI_TxtImgBtn>(nameof(CreateBtn));
            JoinBtn = Helpers.Find<UI_TxtImgBtn>(nameof(JoinBtn));
            HowToBtn = Helpers.Find<UI_TxtImgBtn>(nameof(HowToBtn));
            CardSetsBtn = Helpers.Find<UI_TxtImgBtn>(nameof(CardSetsBtn));
            QuitBtn = Helpers.Find<UI_TxtImgBtn>(nameof(QuitBtn));
            TipBackgroundImg = Helpers.Find<TouchBtn>(nameof(TipBackgroundImg));
            TipBackgroundTxt = Helpers.Find<DynamicText>(nameof(TipBackgroundTxt));
        }

        private void Start()
        {
            CreateBtn.SetText(TextTag.Create, (s) => s.ToUpper(), 60, true);
            CreateBtn.ColorBackground(ColorType.Grey);
            JoinBtn.SetText(TextTag.Join, (s) => s.ToUpper(), 60, true);
            JoinBtn.ColorBackground(ColorType.Grey);
            HowToBtn.SetText(TextTag.HowTo, (s) => s.ToUpper(), 60, true);
            HowToBtn.ColorBackground(ColorType.Grey);
            CardSetsBtn.SetText(TextTag.CardSets, (s) => s.ToUpper(), 60, true);
            CardSetsBtn.GetBtn().AddOnClickListener(() =>
            {
                if (DataCardSet.IsDoneLoading)
                {
                    MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.CardSets);
                }
            });
            CardSetsBtn.ColorBackground(ColorType.Grey);
            QuitBtn.SetText(TextTag.Quit, (s) => s.ToUpper(), 60, true);
            QuitBtn.ColorBackground(ColorType.Gold);
            CreateBtn.GetBtn().AddOnClickListener(() =>
            {
                if (DataCardSet.IsDoneLoading)
                {
                    MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.Create);
                }
            });
            JoinBtn.GetBtn().AddOnClickListener(() =>
            {
                if (DataCardSet.IsDoneLoading)
                {
                    MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.Join);
                }
            });
            QuitBtn.GetBtn().AddOnClickListener(() => MenuHandler.Instance.Quit());
            //TipBackgroundTxt.SetText(TextTag.TipPressHold);
        }

        public void Activate(bool active)
        {
            this.gameObject.SetActive(active);
        }

        public void UpdateUI()
        {
            CreateBtn.ColorBackground(DataCardSet.IsDoneLoading ? ColorType.Gold : ColorType.Grey);
            JoinBtn.ColorBackground(DataCardSet.IsDoneLoading ? ColorType.Gold : ColorType.Grey);
            CardSetsBtn.ColorBackground(DataCardSet.IsDoneLoading ? ColorType.Gold : ColorType.Grey);
        }
    }
}
