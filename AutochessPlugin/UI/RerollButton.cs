using System;
using System.Collections.Generic;
using System.Text;
using RORAutochess.Units;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoR2.UI;
using UnityEngine.Events;
using RoR2;

namespace RORAutochess.UI
{
    public class RerollButton : MonoBehaviour
    {
        public HGButton button;
        public float cost = 25f;
        public TextMeshProUGUI costText;
        public CharacterMaster source;

        public Shop shop;
        private void Awake()
        {
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new UnityAction(OnRerollClicked));
            if (costText) costText.text = this.cost.ToString();
        }

        private void Update()
        {
            if(this.shop)
                this.source = shop.source;

            if(this.button) // check money
            {
                button.interactable = true;
            }
            else
            {
                button.interactable = false;
            }
        }
        private void OnRerollClicked() // spend money
        {
            shop.RefreshShop();

        }
    }
}
