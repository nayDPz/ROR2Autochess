using System;
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
        public UICamera uiCamera;

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

        private void LateUpdate()
        {
            if (!podObject)
                return;

            Camera uiCam = uiCamera.camera;
            Camera sceneCam = uiCamera.cameraRigController.sceneCam;
            if (sceneCam && uiCam)
            {

                Vector3 position = podObject.transform.position;
                position.y += 4.5f;
                Vector3 vector = sceneCam.WorldToScreenPoint(position);
                Vector3 position2 = uiCam.ScreenToWorldPoint(vector);

                base.transform.position = position2;
            }
        }


    }
}
