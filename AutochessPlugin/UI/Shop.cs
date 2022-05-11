using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
namespace RORAutochess.UI
{
    public class Shop : MonoBehaviour
    {
        public static GameObject slotPrefab;
        public static GameObject entryPrefab;
        //needs reroll odds

        public static void Init()
        {
            if(!slotPrefab)
                slotPrefab = AutochessPlugin.hud.LoadAsset<GameObject>("ShopSlot");
            if(!entryPrefab)
                entryPrefab = AutochessPlugin.hud.LoadAsset<GameObject>("ShopEntry");
        }

        public CharacterMaster source;
        public ShopSlot[] shopSlots;
        public event Action onShopRefreshed;

        private void Update()
        {
            foreach (ShopSlot slot in shopSlots)
            {
                slot.source = this.source;
            }
        }
        private void Awake()
        {
            foreach (ShopSlot slot in shopSlots)
            {
                slot.source = this.source;
            }
            this.RefreshShop();
        }
        public void RefreshShop()
        {
            foreach (ShopSlot slot in shopSlots)
            {
                slot.RefreshEntry();
            }
            if (onShopRefreshed != null)
                onShopRefreshed.Invoke();
        }
    }
}
