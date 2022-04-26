using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.UI;

namespace RORAutochess.UI
{
    class AutochessHUDAddon : MonoBehaviour
    {
        public Shop shop;
        public RerollButton rerollButton;
        public BuyXPButton xpButton;
        public EXPPanel xpPanel;
        public CharacterMaster targetMaster;


        private void Start()
        {
            this.targetMaster = base.GetComponent<HUD>().targetMaster;
        }
        public void Update()
        {
            if (!this.targetMaster)
            {
                this.targetMaster = base.GetComponent<HUD>().targetMaster;
            }
            if(this.xpPanel)
            {
                this.xpPanel.source = this.targetMaster;
            }
            if(this.rerollButton)
            {
                this.rerollButton.source = this.targetMaster;
            }
            if(this.xpButton)
            {
                this.xpButton.source = this.targetMaster;
            }
            if(this.shop)
            {
                this.shop.source = this.targetMaster;
            }

        }

    }
}
