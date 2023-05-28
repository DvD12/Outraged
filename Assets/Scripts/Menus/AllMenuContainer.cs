using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Outraged
{
    public class AllMenuContainer : MonoBehaviour
    {
        public static AllMenuContainer Instance;
        public TouchBtn AllMenuOptionsBtn;
        public TextMeshProUGUI DevelopmentBuildTxt;

        private void Awake()
        {
            Instance = this;
            AllMenuOptionsBtn = Helpers.Find<TouchBtn>(nameof(AllMenuOptionsBtn));
            DevelopmentBuildTxt = Helpers.Find<TextMeshProUGUI>(nameof(DevelopmentBuildTxt));
        }

        private void Start()
        {
            AllMenuOptionsBtn.AddOnClickListener(() => MenuHandler.Instance.SelectMenu(MenuHandler.MenuState.Options));
            DevelopmentBuildTxt.text = $"Development Build {BuildDate.ToString()}";
        }

        public void Color()
        {
            AllMenuOptionsBtn.Color(MenuHandler.CurrentMenu == MenuHandler.MenuState.Options ? ColorType.Cyan : ColorType.Gold);
        }
    }
}
