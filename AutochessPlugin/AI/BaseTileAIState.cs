using System;
using System.Collections.Generic;
using System.Text;
using RoR2.Navigation;
using RoR2;
using EntityStates;
using EntityStates.AI;
using RoR2.CharacterAI;
using UnityEngine;
using System.Linq;

namespace RORAutochess.AI
{
    class BaseTileAIState : BaseAIState
    {
        private TileNavigator navigator;
        private EntityStateMachine bodyStateMachine;
        private HurtBox target;
        private bool targetInRange;
        public override void OnEnter()
        {
            base.OnEnter();
            this.navigator = base.GetComponent<TileNavigator>();
            this.bodyStateMachine = base.body.GetComponents<EntityStateMachine>().Where(x => x.customName == "Body").FirstOrDefault();

            TileNavigator.beforeTileUpdate += RequestAI;
            TileNavigator.afterTileUpdate += UpdateAI;
            
        }

        private void RequestAI()
        {
            if(!this.navigator.benched)
            {
                TeamMask enemyTeams = TeamMask.GetEnemyTeams(base.body.teamComponent.teamIndex);
                if (!this.target)
                {
                    HurtBox hurtBox = new SphereSearch
                    {
                        radius = 200f,
                        mask = LayerIndex.entityPrecise.mask,
                        origin = base.body.gameObject.transform.position,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
                    }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().FirstOrDefault();

                    if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.alive)
                        this.target = hurtBox;
                }

                bool needMovement = false;
                if (this.target && this.bodyStateMachine.IsInMainState())
                {
                    if (!this.targetInRange)
                    {
                        needMovement = true;
                        this.navigator.RequestTileTowards(this.target.transform.position);
                    }
                }
                this.navigator.navigationEnabled = needMovement;
            }
            
		}

        private void UpdateAI()
        {
            if(!this.navigator.benched)
            {
                if (this.target)
                {
                    CharacterMaster m = this.target.healthComponent.body.master;
                    TileNavigator t = m.GetComponent<TileNavigator>();

                    GenericBoard.Tile tile = t != null ? t.currentTile : this.navigator.currentBoard.GetClosestTile(this.target.transform.position);
                    int d;
                    this.targetInRange = this.navigator.currentTile.tileDistances.TryGetValue(tile, out d) && d <= this.navigator.attackRange;
                }
                else this.targetInRange = false;
            }
                   
            
        }

        public override void OnExit()
        {
            base.OnExit();
            TileNavigator.beforeTileUpdate -= RequestAI;
            TileNavigator.afterTileUpdate -= UpdateAI;
        }

        public override BaseAI.BodyInputs GenerateBodyInputs(in BaseAI.BodyInputs previousBodyInputs) // implement skilldrivers somehow
        {
            this.bodyInputs.moveVector = Vector3.zero;
            bool pressSkill1 = false;
            bool pressSkill2 = false;
            bool pressSkill3 = false;
            bool pressSkill4 = false;
			if (this.target)
			{
                int d;
                pressSkill1 = this.targetInRange && !this.navigator.navigationEnabled;

                this.bodyInputs.desiredAimDirection = this.target.transform.position - base.body.transform.position;

                if(this.navigator.navigationEnabled)
                {
                    base.body.moveSpeed = this.navigator.moveVector.magnitude / TileNavigator.updateInterval;
                    this.bodyInputs.moveVector = this.navigator.moveVector.normalized;
                }
                
            }

            base.body.SetAimTimer(pressSkill1 ? Mathf.Infinity : 0);
            this.bodyInputs.pressSkill1 = pressSkill1;

            return base.GenerateBodyInputs(previousBodyInputs);
        }
    }
}
