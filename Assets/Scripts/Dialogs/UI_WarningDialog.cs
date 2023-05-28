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
    public class UI_WarningDialog : MonoBehaviour
    {
        public static UI_WarningDialog Instance;
        public DynamicText WarningDialogTitleTxt;
        public DynamicText WarningDialogDescTxt;
        public UI_TxtImgBtn WarningDialogOKBtn;
        public UI_TxtImgBtn WarningDialogCancelBtn;

        Action Callback;
        private void Awake()
        {
            Instance = this;
            this.WarningDialogTitleTxt = Helpers.Find<DynamicText>(nameof(WarningDialogTitleTxt));
            this.WarningDialogDescTxt = Helpers.Find<DynamicText>(nameof(WarningDialogDescTxt));
            this.WarningDialogOKBtn = Helpers.Find<UI_TxtImgBtn>(nameof(WarningDialogOKBtn));
            this.WarningDialogCancelBtn = Helpers.Find<UI_TxtImgBtn>(nameof(WarningDialogCancelBtn));
        }

        private void Start()
        {
            this.WarningDialogOKBtn.Resize(252, 97);
            this.WarningDialogCancelBtn.Resize(252, 97);
        }

        private void OnOKBtnPressed()
        {
            this.Callback?.Invoke();
            this.Activate(false);
        }
        private void OnCancelBtnPressed()
        {
            this.Activate(false);
        }

        // Dictionary<string, string> -> elem name -> elem displayed name (generic: can be applied to enums, numbers etc.)
        public void Activate(bool active, TextTag titleName = TextTag.Empty, TextTag descTxt = TextTag.Empty, Action callback = null, TextTag OKTxt = TextTag.Yes, TextTag NoTxt = TextTag.No, Func<string, string> DescEditor = null)
        {
            // Activation logic
            MenuHandler.Instance.DialogsBackgroundImg.gameObject.SetActive(active);
            this.gameObject.SetActive(active);
            if (active)
            {
                // Setup
                this.Callback = callback;
                this.WarningDialogOKBtn.SetText(OKTxt, (s) => s.ToUpper(), 50);
                this.WarningDialogCancelBtn.SetText(NoTxt, (s) => s.ToUpper(), 50);
                this.WarningDialogTitleTxt.SetText(titleName);
                this.WarningDialogDescTxt.SetText(descTxt, DescEditor);

                this.WarningDialogOKBtn.GetBtn().AddOnClickListener(() => OnOKBtnPressed());
                this.WarningDialogCancelBtn.GetBtn().AddOnClickListener(() => OnCancelBtnPressed());

            }
        }
    }
}
