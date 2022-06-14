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
        public static List<AutochessHUDAddon> instances = new List<AutochessHUDAddon>();
        public Shop shop;
        public RerollButton rerollButton;
        public BuyXPButton xpButton;
        public EXPText xpPanel;
        public CharacterMaster targetMaster;
        public AllyHealthBarViewer allyHealthBarViewer;

        public HUD hud;

        public static AutochessHUDAddon FindByMaster(CharacterMaster m)
        {
            foreach(AutochessHUDAddon i in instances)
            {
                if (i.targetMaster == m)
                    return i;
            }
            return null;
        }

        private void OnEnable()
        {
            instances.Add(this);
        }
        private void OnDisable()
        {
            instances.Remove(this);
        }
        private void Start()
        {
            this.hud = base.GetComponent<HUD>();
            this.targetMaster = this.hud.targetMaster;


        }
        public void Update()
        {

            this.targetMaster = this.hud.targetMaster;
            if (!this.targetMaster) return;

            if (this.allyHealthBarViewer)
            {
                this.allyHealthBarViewer.source = this.targetMaster;
                this.allyHealthBarViewer.viewerTeamIndex = this.targetMaster.teamIndex;
            }
            if (this.xpPanel)
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
