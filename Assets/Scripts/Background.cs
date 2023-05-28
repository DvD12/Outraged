using Outraged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    internal class BackgroundCardStats
    {
        public Card BackgroundCard;
        public float ZRotation;
        public float YRotationSpeed;
        public float Scale;
        public float XSpeed;
        public float YSpeed;
    }
    public class Background : MonoBehaviour
    {
        private const float CardCreationTime = .5f;
        private static readonly (int min, int max) SCALE_RANGE = (4, 6);
        private const int SCALE_FACTOR = 10;
        private static readonly (int min, int max) SPEED_RANGE = (3, 5);
        private static readonly (int min, int max) Y_ROTATION_SPEED = (0, 1);
        private const int MAX_BACKGROUND_CARDS = 30;
        private static readonly Vector3 MAX_EXISTENCE_POSITION = new Vector3(2300, -700, 0);
        private static readonly (int x_min, int x_max, int y_min, int y_max) SPAWN_RANGE = (-2239, 492, 778, 1480);
        private List<BackgroundCardStats> BackgroundCards;

        private GameObject BackgroundCardsContainer;
        private System.Random RNG = new System.Random(DateTime.Now.Millisecond);

        private void Awake()
        {
            BackgroundCardsContainer = Helpers.Find<GameObject>(nameof(BackgroundCardsContainer));
            BackgroundCards = new List<BackgroundCardStats>();
        }

        private float LastCardCreation;
        private void FixedUpdate()
        {
            var asd = QualitySettings.vSyncCount;
            if (Time.fixedTime > LastCardCreation + CardCreationTime)
            {
                PopulateCards();
            }
            foreach (var card in BackgroundCards)
            {
                card.BackgroundCard.transform.Rotate(new Vector3(0, card.YRotationSpeed, 0));
                card.BackgroundCard.transform.localPosition += new Vector3(card.XSpeed, card.YSpeed, 0);
            }
            RemoveCards();
        }

        private void PopulateCards()
        {
            if (BackgroundCards.Count() < MAX_BACKGROUND_CARDS)
            {
                LastCardCreation = Time.fixedTime;
                var speed = RNG.Next(SPEED_RANGE.min, SPEED_RANGE.max);
                var newBackgroundCard = new BackgroundCardStats()
                {
                    Scale = (float)RNG.Next(SCALE_RANGE.min, SCALE_RANGE.max) / SCALE_FACTOR,
                    XSpeed = speed,
                    YSpeed = -speed,
                    YRotationSpeed = RNG.Next(Y_ROTATION_SPEED.min, Y_ROTATION_SPEED.max),
                    ZRotation = 45
                };
                var newCard = Instantiate(Resources.Load<GameObject>("Prefabs/Card")).GetComponent<Card>();
                newCard.GetComponent<Image>().Color(ColorType.Blackened);
                newCard.ColorAlpha(0.75f);
                newCard.CardTxt.SetText("");
                newCard.CardNumberPickTxt.Text.text = "";
                newCard.transform.SetParent(BackgroundCardsContainer.transform);
                newCard.transform.localPosition = new Vector3(RNG.Next(SPAWN_RANGE.x_min, SPAWN_RANGE.x_max), RNG.Next(SPAWN_RANGE.y_min, SPAWN_RANGE.y_max), 0);
                newCard.transform.Rotate(new Vector3(0, 0, newBackgroundCard.ZRotation));
                newCard.transform.localScale = new Vector3(newBackgroundCard.Scale, newBackgroundCard.Scale, newBackgroundCard.Scale);
                newBackgroundCard.BackgroundCard = newCard;
                BackgroundCards.Add(newBackgroundCard);
            }
        }
        private void RemoveCards()
        {
            for (int i = 0; i < BackgroundCards.Count(); i++)
            {
                var card = BackgroundCards[i];
                if (card.BackgroundCard.transform.localPosition.x > MAX_EXISTENCE_POSITION.x || card.BackgroundCard.transform.localPosition.y < MAX_EXISTENCE_POSITION.y)
                {
                    BackgroundCards.RemoveAt(i);
                    Destroy(card.BackgroundCard.gameObject);
                }
            }
        }
    }
}