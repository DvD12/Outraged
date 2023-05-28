using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Outraged
{
    public class UI_ChoiceSelectionDialog : MonoBehaviour
    {
        public static UI_ChoiceSelectionDialog Instance;
        public GameObject ChoiceSelectionDialogElemsList;
        public UI_TxtImgBtn ChoiceSelectionDialogOKBtn;
        public UI_TxtImgBtn ChoiceSelectionDialogCancelBtn;
        public DynamicText ChoiceSelectionDialogElemsLeftToSelectTxt;
        public TMP_InputField ChoiceSelectionDialogFilterInputTxt;
        public DynamicText ChoiceSelectionDialogFilterTxt;
        public Scrollbar ChoiceSelectionDialogBoxScrollbar;
        public Image ChoiceSelectionDialogBoxSlidingArrowDown;
        public Image ChoiceSelectionDialogBoxSlidingArrowUp;
        public Image ChoiceSelectionDialogBoxScrollbarHandleImg;
        public Image ChoiceSelectionDialogHeaderImg;
        public Image ChoiceSelectionDialogFooterImg;
        public DynamicText ChoiceSelectionDialogSelectionTitleTxt;

        public int MaxChoicesAllowed = Int32.MaxValue;
        public int ChoicesShown = 0;
        public HashSet<string> CurrentlySelectedValues;
        public Action<HashSet<string>> Callback;
        private Dictionary<UI_TxtImgBtn, ChoiceSelection> Entries = new Dictionary<UI_TxtImgBtn, ChoiceSelection>();

        private const string CannotPressOKTooltip = "ChoiceSelectionDialogCannotPressOKTooltip";
        private const string OKBtnTooltip = "ChoiceSelectionDialogOKBtnTooltip";
        private const string CancelBtnTooltip = "ChoiceSelectionDialogCancelBtnTooltip";
        private const string ElementKeyboardShortcutTooltip = "ChoiceSelectionDialogElementKeyboardShortcutTooltip";
        public static List<string> ScreenTooltips = new List<string>() { CannotPressOKTooltip, OKBtnTooltip, CancelBtnTooltip, ElementKeyboardShortcutTooltip };

        private void Awake()
        {
            Instance = this;
            this.ChoiceSelectionDialogElemsList = Helpers.Find<GameObject>("ChoiceSelectionDialogElemsList");
            this.ChoiceSelectionDialogOKBtn = Helpers.Find<UI_TxtImgBtn>("ChoiceSelectionDialogOKBtn");
            this.ChoiceSelectionDialogCancelBtn = Helpers.Find<UI_TxtImgBtn>("ChoiceSelectionDialogCancelBtn");
            this.ChoiceSelectionDialogElemsLeftToSelectTxt = Helpers.Find<DynamicText>("ChoiceSelectionDialogElemsLeftToSelectTxt");
            this.ChoiceSelectionDialogFilterInputTxt = Helpers.Find<TMP_InputField>("ChoiceSelectionDialogFilterInputTxt");
            this.ChoiceSelectionDialogFilterTxt = Helpers.Find<DynamicText>("ChoiceSelectionDialogFilterTxt");
            this.ChoiceSelectionDialogBoxScrollbar = Helpers.Find<Scrollbar>("ChoiceSelectionDialogBoxScrollbar");
            this.ChoiceSelectionDialogBoxSlidingArrowDown = Helpers.Find<Image>("ChoiceSelectionDialogBoxSlidingArrowDown");
            this.ChoiceSelectionDialogBoxSlidingArrowUp = Helpers.Find<Image>("ChoiceSelectionDialogBoxSlidingArrowUp");
            this.ChoiceSelectionDialogBoxScrollbarHandleImg = Helpers.Find<Image>("ChoiceSelectionDialogBoxScrollbarHandleImg");
            this.ChoiceSelectionDialogHeaderImg = Helpers.Find<Image>("ChoiceSelectionDialogHeaderImg");
            this.ChoiceSelectionDialogFooterImg = Helpers.Find<Image>("ChoiceSelectionDialogFooterImg");
            this.ChoiceSelectionDialogSelectionTitleTxt = Helpers.Find<DynamicText>("ChoiceSelectionDialogSelectionTitleTxt");
        }

        private void Start()
        {
            this.ChoiceSelectionDialogOKBtn.Resize(252, 97);
            this.ChoiceSelectionDialogCancelBtn.Resize(252, 97);
        }

        private void FixedUpdate()
        {
            this.ChoiceSelectionDialogBoxScrollbar.ScrollWithMouse(0.01f, false);
        }

        private void Update()
        {
            for (int i = 1; i < 10; i++)
            {
                if (!this.ChoiceSelectionDialogFilterInputTxt.isFocused && Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "Alpha" + i)))
                {
                    if (Entries.Count >= i) { EntryOnClickListener(Entries.ElementAt(i - 1)); }
                }
            }
            if (!this.ChoiceSelectionDialogFilterInputTxt.isFocused && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))) { this.OnCancelBtnPressed(); }
            else if (!this.ChoiceSelectionDialogFilterInputTxt.isFocused && Input.GetKeyDown(KeyCode.Return)) { this.OnOKBtnPressed(); }
        }

        private void OnOKBtnPressed()
        {
            if (this.CurrentlySelectedValues.Count > 0)
            {
                this.Callback(this.CurrentlySelectedValues);
                this.Activate(false);
            }
            else
            {
                AudioManager.Instance.PlayErrorSound();
            }
        }
        private void OnCancelBtnPressed()
        {
            this.Activate(false);
        }

        private void UpdateItemsToSelectTxt()
        {
            this.ChoiceSelectionDialogElemsLeftToSelectTxt.SetText(TextTag.ChoiceSelectionDialogElemsLeftToSelectTxt, (s) =>
            {
                s = s.Replace("#VALUE1", this.CurrentlySelectedValues.Count().ToString()).Replace("#VALUE2", this.MaxChoicesAllowed == Int32.MaxValue ? TextTag.Any.GetText().ToLower() : this.MaxChoicesAllowed.ToString());
                s = s.Replace("#ELEMENTORELEMENTS", this.CurrentlySelectedValues.Count() == 1 ? TextTag.Element.GetText() : TextTag.Elements.GetText());
                if (this.ChoicesShown != this.Entries.Count)
                {
                    s = s.Replace("#VALUE3", this.ChoicesShown.ToString());
                    s = s.Replace("#IFFILTER", "");
                    s = s.Replace("#/IFFILTER", "");
                }
                else
                {
                    s = s.RemoveWithin("#IFFILTER", "#/IFFILTER");
                }
                return s;
            });
        }

        public class ChoiceSelection
        {
            public string InternalName;
            public string DisplayedName;
            public ChoiceSelection(string internalName, string displayedName) { this.InternalName = internalName; this.DisplayedName = displayedName; }
            public static List<ChoiceSelection> CreateBoolSelection()
            {
                return new List<ChoiceSelection>() { new ChoiceSelection("False", "False"), new ChoiceSelection("True", "True") };
            }
        }

        // Dictionary<string, string> -> elem name -> elem displayed name (generic: can be applied to enums, numbers etc.)
        public void Activate(bool active, List<ChoiceSelection> elems = null, string titleName = null, int MaxChoices = Int32.MaxValue, Action<HashSet<string>> callback = null, List<string> SelectedChoices = null)
        {
            // Activation logic
            MenuHandler.Instance.DialogsBackgroundImg.gameObject.SetActive(active);
            this.gameObject.SetActive(active);
            if (active)
            {
                // Setup
                this.Callback = callback;
                this.Entries.Clear();
                this.CurrentlySelectedValues = new HashSet<string>();
                this.MaxChoicesAllowed = MaxChoices;
                this.ChoicesShown = elems.Count;
                this.ChoiceSelectionDialogElemsList.DestroyChildren();
                this.ChoiceSelectionDialogOKBtn.SetText(TextTag.Select, (s) => s.ToUpper());
                this.ChoiceSelectionDialogCancelBtn.SetText(TextTag.Cancel, (s) => s.ToUpper());
                this.ChoiceSelectionDialogSelectionTitleTxt.Text.text = TextTag.Select.GetText().ToUpper() + " " + titleName;
                this.ChoiceSelectionDialogFilterInputTxt.text = "";
                if (!Application.isMobilePlatform)
                {
                    this.ChoiceSelectionDialogFilterInputTxt.Select();
                }

                var OnFilterChange = new TMP_InputField.OnChangeEvent();
                OnFilterChange.AddListener((s) =>
                {
                    this.ChoicesShown = elems.Count;
                    this.Entries.ForEach(entry =>
                    {
                        bool filterIn = entry.Value.DisplayedName.ToLower().Contains(s.ToLower());
                        entry.Key.gameObject.SetActive(filterIn);
                        this.ChoicesShown += (filterIn ? 0 : -1);
                    });
                    UpdateItemsToSelectTxt();
                });

                this.ChoiceSelectionDialogFilterInputTxt.onValueChanged = OnFilterChange;

                this.ChoiceSelectionDialogOKBtn.GetBtn().AddOnClickListener(() => OnOKBtnPressed());
                this.ChoiceSelectionDialogCancelBtn.GetBtn().AddOnClickListener(() => OnCancelBtnPressed());

                // UI
                foreach (var elem in elems)
                {
                    UI_TxtImgBtn entry = UI_TxtImgBtn.Create(this.ChoiceSelectionDialogElemsList.transform, 32);
                    this.Entries.Add(entry, elem);
                    entry.SetTextRestriction(false);
                    entry.SetText(elem.DisplayedName, 48);
                    entry.GetComponent<LayoutElement>().preferredHeight = 70;
                    entry.Resize(740, 60);
                    entry.Color(ColorType.Blackened, ColorType.Cyan);
                }
                UpdateItemsToSelectTxt();

                // Logic
                int i = 1;
                foreach (var entry in this.Entries)
                {
                    var btn = entry.Key.GetBtn();
                    btn.AddOnClickListener(() => EntryOnClickListener(entry));
                }

                if (SelectedChoices != null)
                {
                    foreach (var choice in SelectedChoices)
                    {
                        EntryOnClickListener(this.Entries.First(x => x.Value.InternalName == choice));
                    }
                }
                Color();
            }
        }
        private void EntryOnClickListener(KeyValuePair<UI_TxtImgBtn, ChoiceSelection> entry)
        {
            // Element not selected - highlight it
            if (!this.CurrentlySelectedValues.Contains(entry.Value.InternalName))
            {
                entry.Key.ColorBackground(ColorType.Gold);
                this.ChoiceSelectionDialogOKBtn.ColorBackground(ColorType.Gold);

                this.CurrentlySelectedValues.Add(entry.Value.InternalName);
                while (this.CurrentlySelectedValues.Count() > this.MaxChoicesAllowed) // Reset coloring
                {
                    string firstElem = this.CurrentlySelectedValues.Where(x => x != entry.Value.InternalName).First();
                    this.Entries.First(x => x.Value.InternalName == firstElem).Key.ColorBackground(ColorType.Cyan);
                    this.CurrentlySelectedValues.Remove(firstElem);
                }
            }
            // Element was selected - deselect
            else
            {
                entry.Key.ColorBackground(ColorType.Cyan);
                this.CurrentlySelectedValues.Remove(entry.Value.InternalName);
                if (this.CurrentlySelectedValues.Count() == 0)
                {
                    this.ChoiceSelectionDialogOKBtn.ColorBackground(ColorType.Grey);
                }
            }
            UpdateItemsToSelectTxt();
        }

        public void Color()
        {
            this.ChoiceSelectionDialogOKBtn.ColorBackground(ColorType.Grey);
            this.ChoiceSelectionDialogCancelBtn.ColorBackground(ColorType.Gold);
        }
    }
}
