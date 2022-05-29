﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RoR2.UI;
using RoR2;
using UnityEngine.Events;

namespace RORAutochess.UI
{
    public class PodShop : Shop
    {
        public GameObject podObject;
        public GameObject flashPanel;
        public RerollButton rerollButton;

        private void OnDestroy()
        {
            Destroy(podObject);
        }

        private void Awake()
        {
            this.onShopRefreshed += OnShopRefreshed;
            this.RefreshShop();
        }

        private void OnShopRefreshed()
        {
            this.flashPanel.SetActive(false);
            this.flashPanel.SetActive(true);
        }


    }
}
