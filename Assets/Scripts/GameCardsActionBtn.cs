using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Outraged
{
    public class GameCardsActionBtn : MonoBehaviour
    {
        public UICircle Clock;

        private void Awake()
        {
            Initialize();
        }

        private bool isInitialized = false;
        public void Initialize()
        {
            if (!isInitialized)
            {
                Clock = this.GetComponent<UICircle>();
                isInitialized = true;
            }
        }

        private void Update()
        {
            if (GameState.Instance != null)
            {
                bool doAction = UpdateCircle();
                if (doAction)
                {
                    GameCardsMenu.Instance?.ActionBtnCallback(true);
                }
            }
        }
        public bool UpdateCircle()
        {
            Initialize();
            if (GameState.Instance.IsStateTimed)
            {
                long timeoutStart = GameState.Instance.CurrentStateTimeoutStart.Ticks;
                long timeoutEnd = GameState.Instance.CurrentStateTimeout.Ticks;
                long timeoutRemaining = timeoutEnd - DateTime.Now.Ticks;
                float percent = 100*timeoutRemaining / (timeoutEnd - timeoutStart);
                Clock.FillPercent = Math.Max(0, (int)percent);
                Clock.color = new Color(percent < 50 ? (100 - percent)/100 : 0, percent > 50 ? percent/100 : 0, 0);
                return timeoutRemaining <= 0;
            }
            else
            {
                Clock.FillPercent = 0;
                return false;
            }
        }
    }
}
