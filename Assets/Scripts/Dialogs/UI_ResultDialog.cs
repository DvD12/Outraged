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
    public class UI_ResultDialog : MonoBehaviour
    {
        public static UI_ResultDialog Instance;
        public DynamicText ResultDialogTitleTxt;
        public DynamicText ResultDialogDescTxt;
        public UI_TxtImgBtn ResultDialogOKBtn;
        public UI_TxtImgBtn ResultDialogCancelBtn;

        private void Awake()
        {
            Instance = this;
            this.ResultDialogTitleTxt = Helpers.Find<DynamicText>(nameof(ResultDialogTitleTxt));
            this.ResultDialogDescTxt = Helpers.Find<DynamicText>(nameof(ResultDialogDescTxt));
            this.ResultDialogOKBtn = Helpers.Find<UI_TxtImgBtn>(nameof(ResultDialogOKBtn));
        }

        private void Start()
        {
            this.ResultDialogOKBtn.Resize(252, 97);
        }

        private void OnOKBtnPressed()
        {
            this.Activate(false);
        }

        public void Activate(bool active, TextTag titleName = TextTag.Empty, TextTag descTxt = TextTag.Empty, TextTag OKTxt = TextTag.OK, string desc = "")
        {
            // Activation logic
            MenuHandler.Instance.DialogsBackgroundImg.gameObject.SetActive(active);
            this.gameObject.SetActive(active);
            if (active)
            {
                this.ResultDialogOKBtn.SetText(OKTxt, (s) => s.ToUpper(), 50);
                this.ResultDialogTitleTxt.SetText(titleName);
                if (desc != "") { this.ResultDialogDescTxt.SetText(desc); }
                else { this.ResultDialogDescTxt.SetText(descTxt); }

                this.ResultDialogOKBtn.GetBtn().AddOnClickListener(() => OnOKBtnPressed());

            }
        }
    }
}
