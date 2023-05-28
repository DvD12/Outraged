using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Outraged
{
    public class UI_CardsDialog : MonoBehaviour
    {
        public static UI_CardsDialog Instance;
        public GameObject CardsDialogElemsList;
        public Scrollbar CardsDialogBoxScrollbar;
        public Image CardsDialogBoxSlidingArrowDown;
        public Image CardsDialogBoxSlidingArrowUp;
        public Image CardsDialogBoxScrollbarHandleImg;
        public UI_TxtImgBtn CardsDialogBackBtn;

        public List<Card> CardsCache = new List<Card>();

        private bool isInitialized = false;
        public void Initialize()
        {
            if (!isInitialized)
            {
                Instance = this;
                CardsDialogElemsList = Helpers.Find<GameObject>(nameof(CardsDialogElemsList));
                CardsDialogBoxScrollbar = Helpers.Find<Scrollbar>(nameof(CardsDialogBoxScrollbar));
                CardsDialogBoxSlidingArrowDown = Helpers.Find<Image>(nameof(CardsDialogBoxSlidingArrowDown));
                CardsDialogBoxSlidingArrowUp = Helpers.Find<Image>(nameof(CardsDialogBoxSlidingArrowUp));
                CardsDialogBoxScrollbarHandleImg = Helpers.Find<Image>(nameof(CardsDialogBoxScrollbarHandleImg));
                CardsDialogBackBtn = Helpers.Find<UI_TxtImgBtn>(nameof(CardsDialogBackBtn));
                isInitialized = true;
            }
        }

        private void Awake()
        {
            Initialize();
        }
        private void Start()
        {
            CardsDialogBackBtn.SetText(TextTag.Back, (s) => s.ToUpper());
            CardsDialogBackBtn.GetBtn().AddOnClickListener(() => this.Activate(false));
        }

        public void Activate(bool active, string cardSet = "")
        {
            MenuHandler.Instance.DialogsBackgroundImg.gameObject.SetActive(active);
            this.gameObject.SetActive(active);
            if (active)
            {
                var data = DataCardSet.CardSets[cardSet];
                int i = 0;
                foreach (var cardData in data.Cards)
                {
                    Card card = null;
                    if (CardsCache.HasIndex(i))
                    {
                        card = CardsCache[i];
                        card.gameObject.SetActive(true);
                    }
                    else
                    {
                        card = Instantiate(Resources.Load<GameObject>("Prefabs/Card")).GetComponent<Card>();
                        card.transform.SetParent(CardsDialogElemsList.transform);
                        card.transform.localPosition = Vector3.zero;
                        card.transform.localScale = Vector3.one;
                        CardsCache.Add(card);
                    }
                    card.SetCard(cardData.Key);
                    i++;
                }
                for (int j = i; j < CardsCache.Count; j++) { CardsCache[j].gameObject.SetActive(false); }
            }
        }
    }
}
