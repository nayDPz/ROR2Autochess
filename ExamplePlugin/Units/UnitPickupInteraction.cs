using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using RORAutochess.AI;

namespace RORAutochess.Units
{
    class UnitPickupInteraction : MonoBehaviour, IInteractable // SHOULD PROBABLY MAKE THIS A HEX INTERACTION INSTEAD OF UNIT
    {
        private bool pickedUp;

        private float cooldown;

        private CharacterMaster master;
        private CharacterBody body;
        private void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            this.master = body.GetComponent<CharacterMaster>();
        }
        public string GetContextString([NotNull] Interactor activator)
        {
            return !pickedUp ? "Pick up Unit" : "Put down Unit";
        }

        public Interactability GetInteractability([NotNull] Interactor activator)
        {
            return this.cooldown <= 0 ? Interactability.Available : Interactability.Disabled;
        }

        public void OnInteractionBegin([NotNull] Interactor activator)
        {
            if (!this.master)
                this.master = this.body.master;

            this.cooldown = 0.5f;

            InteractionDriver driver = activator.GetComponent<InteractionDriver>();
            if(driver)
            {
                if (!pickedUp)
                {
                    pickedUp = true;
                    driver.interactableOverride = base.gameObject;
                    if(GenericBoard.onBoard)
                    {
                        TileNavigator t = TileNavigator.FindTileNavigatorByMaster(this.master);
                        t.Pickup();
                    }

                }
                else
                {
                    pickedUp = false;
                    driver.interactableOverride = null;
                    if (GenericBoard.onBoard)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
                        Physics.Raycast(ray, out RaycastHit hit, 1000f);
                        Vector3 vector = hit.point;
                        
                        GenericBoard.Tile tile = GenericBoard.GetClosestTile(vector);
                        TileNavigator t = TileNavigator.FindTileNavigatorByMaster(this.master);
                        this.body.characterMotor.AddDisplacement(tile.worldPosition - base.transform.position);
                        this.body.characterMotor.velocity = Vector3.zero;
                        t.PlaceOnTile(tile);
                        Log.LogInfo(tile.index);
                    }

                }
            }
            else
            {
                Log.LogError("InteractionDriver missing, can't pick up Unit");
            }
        }

        private void FixedUpdate()
        {
            this.cooldown -= Time.fixedDeltaTime;
            if(this.pickedUp)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
                Physics.Raycast(ray, out RaycastHit hit, 1000f);
                Vector3 vector = hit.point;
                vector.y += 5f;
                this.body.characterMotor.AddDisplacement(vector - base.transform.position);
                Log.LogInfo(vector);
            }
        }      

        public bool ShouldIgnoreSpherecastForInteractibility([NotNull] Interactor activator)
        {
            return false;
        }

        public bool ShouldShowOnScanner()
        {
            return false;
        }
    }
}
