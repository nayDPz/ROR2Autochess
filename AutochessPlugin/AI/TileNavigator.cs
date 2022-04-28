using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using RORAutochess.Units;
using System.Linq;
namespace RORAutochess.AI
{
    public class TileNavigator : MonoBehaviour
    {

        private CharacterMaster owner;
        private CharacterBody ownerBody;
        private EntityStateMachine aiStateMachine;
        public int attackRange = 1;
        public float moveDuration = 10f;

        private bool disabled;
        public GenericBoard currentBoard;
        public GameObject targetBodyObject;
        public CharacterMaster targetMaster;

        public GenericBoard.Tile currentTile;
        public GenericBoard.Tile nextTile;
        public GenericBoard.Tile targetTile;

        public event Action<GenericBoard.Tile> onMovementBegin;
        /*
         * select target
         * submit movement towards target
         * move towards target
         * finish movement
         * check if target in range
         * 
         */

        private void Awake()
        {
            if (!GenericBoard.inBoardScene) // should be gamemode check ?
            {
                GameObject.Destroy(this);
                return;
            }

            if (!this.owner)
                this.owner = base.GetComponent<CharacterMaster>();

            this.aiStateMachine = base.GetComponent<EntityStateMachine>();

            UnitData data = base.GetComponent<UnitData>();
            if(data)
            {
                this.attackRange = data.attackRange;
                this.moveDuration = data.movementDuration;
            }

        }

        public void PickUp()
        {
            this.enabled = false;
        }

        private void OnDisable()
        {
            this.aiStateMachine.SetNextState(new DoNothing());

            this.currentTile = null;
            this.nextTile = null;
            this.targetTile = null;
            this.targetBodyObject = null;
            this.targetMaster = null;
        }

        public void PlaceDown(GenericBoard.Tile tile)
        {
            this.enabled = true;
            this.currentTile = tile;
        }

        private void FixedUpdate()
        {

            if(!this.targetBodyObject)
            {
                TeamMask enemyTeams = TeamMask.GetEnemyTeams(this.ownerBody.teamComponent.teamIndex);               
                HurtBox hurtBox = new SphereSearch
                {
                    radius = 200f,
                    mask = LayerIndex.entityPrecise.mask,
                    origin = this.ownerBody.gameObject.transform.position,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
                }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().FirstOrDefault();

                if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.alive)
                {
                    this.ChangeTarget(hurtBox.healthComponent.gameObject);
                }                             
            }

            if (this.aiStateMachine.state is StandAttack && this.targetBodyObject && this.currentTile.tileDistances.TryGetValue(this.currentBoard.GetClosestTile(this.targetBodyObject.transform.position), out int d) && d > this.attackRange)
            {
                this.StartMovement();
            }
        }
        private void ChangeTarget(GameObject newTarget)
        {
            if(this.targetMaster) this.targetMaster.GetComponent<TileNavigator>().onMovementBegin -= OnTargetMovement;

            this.targetBodyObject = newTarget;
            this.targetMaster = this.targetBodyObject.GetComponent<CharacterBody>().master;
            this.targetMaster.GetComponent<TileNavigator>().onMovementBegin += OnTargetMovement;

            this.SetState();
        }

        private void OnTargetMovement(GenericBoard.Tile t)
        {
            if (this.currentTile.tileDistances.TryGetValue(t, out int d) && d > this.attackRange)
            {
                this.StartMovement();
            }
        }

        private void StartMovement()
        {
            this.nextTile = this.currentTile.GetClosestConnectedTile(this.targetBodyObject.transform.position);
            this.aiStateMachine.SetNextState(new MoveTowards { target = this.nextTile.worldPosition });

            if (this.onMovementBegin != null)
                this.onMovementBegin.Invoke(this.nextTile);
        }

        private void SetState()
        {
            GenericBoard.Tile t = this.currentBoard.GetClosestTile(this.targetBodyObject.transform.position);
            if (this.currentTile.tileDistances.TryGetValue(t, out int d) && d > this.attackRange)
            {
                this.StartMovement();
            }
            else if (d <= this.attackRange)
            {
                this.aiStateMachine.SetNextState(new StandAttack { target = this.targetBodyObject });
            }
        }

        public void ArriveAt(GenericBoard.Tile tile)
        {
            this.currentTile = tile;
            this.SetState();           
        }

        public GenericBoard.Tile GetNextTile()
        {
            return this.currentTile.GetClosestConnectedTile(this.targetBodyObject.transform.position);
        }

        private void Start()
        {
            if(this.owner.minionOwnership.group != null)
                this.currentBoard = GenericBoard.GetBoardFromMaster(this.owner.minionOwnership.group.resolvedOwnerMaster);
            else
                this.currentBoard = GenericBoard.instancesList[0]; // for testing remove later

            this.ownerBody = this.owner.GetBody();
        }


        private void MoveTowardsTile()
        {
            
        }

    }
}
