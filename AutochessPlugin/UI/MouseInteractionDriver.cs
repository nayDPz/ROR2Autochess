using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace RORAutochess.UI
{
    [RequireComponent(typeof(InteractionDriver))]
    class MouseInteractionDriver : MonoBehaviour
    {
        private float storedInteractionDistance;
        private Interactor interactor;
        private void Awake()
        {
            this.interactor = base.GetComponent<Interactor>();
            storedInteractionDistance = this.interactor.maxInteractionDistance;
            this.interactor.maxInteractionDistance = 300f;
            On.RoR2.InteractionDriver.FixedUpdate += InteractionDriver_FixedUpdate;
            On.RoR2.InteractionDriver.FindBestInteractableObject += InteractionDriver_FindBestInteractableObject;
        }

        

        private GameObject InteractionDriver_FindBestInteractableObject(On.RoR2.InteractionDriver.orig_FindBestInteractableObject orig, InteractionDriver self)
        {
            if(GenericBoard.inBoardScene) // gamemode check plsss
            {
                if (self.interactableOverride)
                {
                    return self.interactableOverride;
                }
                float num = 0f;
                Ray originalAimRay = Camera.main.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
                Physics.Raycast(originalAimRay, out RaycastHit hit, 1000f, LayerIndex.world.intVal, QueryTriggerInteraction.Collide);
                float num2 = 5f;
                return self.interactor.FindBestInteractableObject(originalAimRay, num2 + num, hit.point, num2);
            }
            else
            {
                return orig(self);
            }
        }

        private void OnDestroy()
        {
            this.interactor.maxInteractionDistance = this.storedInteractionDistance;
            On.RoR2.InteractionDriver.FixedUpdate -= InteractionDriver_FixedUpdate;
            On.RoR2.InteractionDriver.FindBestInteractableObject -= InteractionDriver_FindBestInteractableObject;
        }
        private void InteractionDriver_FixedUpdate(On.RoR2.InteractionDriver.orig_FixedUpdate orig, InteractionDriver self)
        {
            if(GenericBoard.inBoardScene)
            {
                if (self.networkIdentity.hasAuthority)
                {
                    self.interactableCooldown -= Time.fixedDeltaTime;
                    self.inputReceived = Input.GetMouseButtonDown(0);
                }
                if (self.inputReceived)
                {
                    GameObject gameObject = self.FindBestInteractableObject();
                    if (gameObject)
                    {
                        self.interactor.AttemptInteraction(gameObject);
                        self.interactableCooldown = 0.25f;
                    }
                }
            }
            else
            {
                orig(self);
            }
        }
    }
}
