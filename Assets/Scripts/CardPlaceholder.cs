using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Outraged
{
    public class CardPlaceholder : MonoBehaviour
    {
        public Image PlaceholderImg;

        private bool isInitialized = false;
        public void Initialize()
        {
            if (!isInitialized)
            {
                PlaceholderImg = this.GetComponent<Image>();
                isInitialized = true;
            }
        }
        public void SetActive(bool active) => this.gameObject.SetActive(active);
    }
}
