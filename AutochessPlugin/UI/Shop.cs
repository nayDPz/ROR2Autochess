using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
namespace RORAutochess.UI
{
    public class Shop : MonoBehaviour
    {
        public GameObject slotPrefab;
        public GameObject entryPrefab;

        public CharacterMaster source;
        public ShopSlot[] shopSlots;
        public event Action onShopRefreshed;

        private void Update() // need to do a pass over awake/start in these UI components
        {
            foreach (ShopSlot slot in shopSlots)
            {
                slot.source = this.source;
                slot.shop = this;
            }
        }

        private void Awake() // this code is fucking worthless and does nothing apparently
        {
            foreach (ShopSlot slot in shopSlots)
            {
                slot.source = this.source;
                slot.shop = this;
            }
            this.RefreshShop();
        }

        public void RefreshShop()
        {
            foreach (ShopSlot slot in shopSlots)
            {
                slot.source = this.source;
                slot.shop = this;
                slot.RefreshEntry();
            }
            if (onShopRefreshed != null)
                onShopRefreshed.Invoke();
        }
    }
}
