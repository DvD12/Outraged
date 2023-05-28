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
    public class UI_InputDialog : MonoBehaviour
    {
        public static UI_InputDialog Instance;
        public DynamicText InputDialogTitleTxt;
        public TMP_InputField InputDialogInputTxt;
        public UI_TxtImgBtn InputDialogOKBtn;
        public UI_TxtImgBtn InputDialogCancelBtn;

        Func<string, bool> Test;
        Action<string> Callback;
        private void Awake()
        {
            Instance = this;
            this.InputDialogTitleTxt = Helpers.Find<DynamicText>(nameof(InputDialogTitleTxt));
            this.InputDialogInputTxt = Helpers.Find<TMP_InputField>(nameof(InputDialogInputTxt));
            this.InputDialogOKBtn = Helpers.Find<UI_TxtImgBtn>(nameof(InputDialogOKBtn));
            this.InputDialogCancelBtn = Helpers.Find<UI_TxtImgBtn>(nameof(InputDialogCancelBtn));
        }

        private void Start()
        {
            this.InputDialogOKBtn.Resize(252, 97);
            this.InputDialogCancelBtn.Resize(252, 97);
        }

        private void Update()
        {
            if (this.InputDialogInputTxt.isFocused && Input.GetKeyDown(KeyCode.Return)) { this.OnOKBtnPressed(); }
        }

        private void OnOKBtnPressed()
        {
            if (Test(this.InputDialogInputTxt.text))
            {
                Callback(this.InputDialogInputTxt.text);
                this.Activate(false);
            }
        }
        private void OnCancelBtnPressed()
        {
            this.Activate(false);
        }

        // Dictionary<string, string> -> elem name -> elem displayed name (generic: can be applied to enums, numbers etc.)
        public void Activate(bool active, string titleName = "", Func<string, bool> test = null, Action<string> callback = null)
        {
            // Activation logic
            MenuHandler.Instance.DialogsBackgroundImg.gameObject.SetActive(active);
            this.gameObject.SetActive(active);
            if (active)
            {
                // Setup
                this.Callback = callback;
                this.Test = test;
                this.InputDialogOKBtn.SetText(TextTag.Select, (s) => s.ToUpper(), 50);
                this.InputDialogCancelBtn.SetText(TextTag.Cancel, (s) => s.ToUpper(), 50);
                this.InputDialogTitleTxt.Text.text = titleName;
                this.InputDialogInputTxt.text = "";
                this.InputDialogInputTxt.Select();

                var OnFilterChange = new TMP_InputField.OnChangeEvent();
                OnFilterChange.AddListener((s) =>
                {
                    Color();
                });

                this.InputDialogInputTxt.onValueChanged = OnFilterChange;

                this.InputDialogOKBtn.GetBtn().AddOnClickListener(() => OnOKBtnPressed());
                this.InputDialogCancelBtn.GetBtn().AddOnClickListener(() => OnCancelBtnPressed());

            }
        }
        public void Color()
        {
            InputDialogOKBtn.ColorBackground(Test(InputDialogInputTxt.text) ? ColorType.Gold : ColorType.Grey);
        }
    }
}
