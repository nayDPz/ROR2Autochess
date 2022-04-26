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
    class BuyXPButton : MonoBehaviour
    {
        public HGButton button;
        public float cost = 50f;
        public TextMeshProUGUI costText;
        public CharacterMaster source;
        private void Awake()
        {
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new UnityAction(OnButtonClicked));
            if (costText) costText.text = this.cost.ToString();
        }

        private void Update()
        {
            if (this.button && this.source) // check money
            {
                button.interactable = true;
            }
            else
            {
                button.interactable = false;
            }
        }
        private void OnButtonClicked()
        {
            Chat.AddMessage("Added XP");
            TeamManager.instance.GiveTeamExperience(source.teamIndex, (ulong)this.cost); // probably need to make my own xp system
        }
    }
}
