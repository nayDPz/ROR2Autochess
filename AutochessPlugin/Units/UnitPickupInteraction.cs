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
    [RequireComponent(typeof(Highlight), typeof(EntityLocator))]
    class UnitPickupInteraction : MonoBehaviour, IInteractable // SHOULD PROBABLY MAKE THIS A HEX INTERACTION INSTEAD OF UNIT
    {
        private bool pickedUp;

        private float cooldown;

        private CharacterMaster master;
        private CharacterBody body;
        private Renderer mainRenderer;
        private MinionOwnership.MinionGroup minionGroup;
        private InteractionDriver driver;
        private void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            this.body.GetComponent<EntityLocator>().entity = base.gameObject;
        }

        private void Start()
        {
            if(this.body.modelLocator && this.body.modelLocator.modelTransform)
            {
                this.mainRenderer = this.body.modelLocator.modelTransform.gameObject.GetComponent<CharacterModel>().mainSkinnedMeshRenderer;
            }
            base.GetComponent<Highlight>().targetRenderer = this.mainRenderer;
            this.master = this.body.master;
        }

        public string GetContextString([NotNull] Interactor activator)
        {
            return !pickedUp ? "Pick up Unit" : "Put down Unit";
        }

        public Interactability GetInteractability([NotNull] Interactor activator)
        {
            if(this.minionGroup == null) this.minionGroup = MinionOwnership.MinionGroup.FindGroup(activator.gameObject.GetComponent<CharacterBody>().master.netId);
            if(this.minionGroup == null) return Interactability.Disabled;
            if (this.master.minionOwnership.group != this.minionGroup) return Interactability.Disabled;

            return this.cooldown <= 0 ? Interactability.Available : Interactability.Disabled;
        }

        public void OnInteractionBegin([NotNull] Interactor activator)
        {
            if (!this.master)
                this.master = this.body.master;

            this.cooldown = 0.05f;

            driver = activator.GetComponent<InteractionDriver>();
            if(driver)
            {
                if (!pickedUp)
                {
                    pickedUp = true;
                    driver.interactableOverride = base.gameObject;
                    if(GenericBoard.inBoardScene)
                    {
                        TileNavigator t = TileNavigator.FindTileNavigator(this.master);
                        t.Pickup();
                    }

                }
                else
                {
                    pickedUp = false;
                    driver.interactableOverride = null;
                    this.PlaceUnit();

                }
            }
            else
            {
                Log.LogError("InteractionDriver missing, can't pick up Unit");
            }
        }

        private void PlaceUnit()
        {
            pickedUp = false;
            if (this.driver.interactableOverride) this.driver.interactableOverride = null;

            if (GenericBoard.inBoardScene)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
                Physics.Raycast(ray, out RaycastHit hit, 1000f);
                Vector3 vector = hit.point;
                TileNavigator t = TileNavigator.FindTileNavigator(this.master);

                GenericBoard.Tile tile = t.currentBoard.GetClosestTile(vector, true);

                this.body.characterMotor.AddDisplacement(tile.worldPosition - base.transform.position);
                this.body.characterMotor.velocity = Vector3.zero;
                t.SetCurrentTile(tile);
                //Log.LogInfo(tile.index);
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
                if(this.body.characterMotor)
                    this.body.characterMotor.AddDisplacement(vector - base.transform.position);
                else if (this.body.rigidbody)
                    this.body.rigidbody.MovePosition(vector);

                if(Input.GetMouseButtonUp(0))
                {
                    this.PlaceUnit();
                }
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
