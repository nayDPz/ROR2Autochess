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
        public float cost = 6f;
        public float costIncreaseOnReroll = 1.5f;
        public TextMeshProUGUI costText;
        public CharacterMaster source;
        public CostTypeIndex costType = CostTypeIndex.Money;

        public Interactor bodyInteractor;
        public Shop shop;
        private void Awake()
        {
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new UnityAction(OnRerollClicked));
            if (costText) costText.text = ((int)this.cost).ToString();
        }

        private void Update()
        {
            if(this.shop)
                this.source = shop.source;

            if (this.source) this.bodyInteractor = this.source.GetBodyObject().GetComponent<Interactor>();

            if (costText) costText.text = ((int)this.cost).ToString();

            if (this.button && this.CanBeAffordedByInteractor(this.source)) // check money
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
            if(this.CanBeAffordedByInteractor(this.source))
            {
                shop.RefreshShop();

                this.source.money -= (uint)this.cost; /// ??????????????????????????????

                this.cost *= this.costIncreaseOnReroll;
            }
            Log.LogError("Can't afford to reroll");
        }

        public bool CanBeAffordedByInteractor(CharacterMaster activator)
        {
            
            return CostTypeCatalog.GetCostTypeDef(this.costType).IsAffordable((int)this.cost, this.bodyInteractor);
        }

    }
}
