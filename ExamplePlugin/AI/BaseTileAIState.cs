using System;
using System.Collections.Generic;
using System.Text;
using RoR2.Navigation;
using RoR2;
using EntityStates;
using EntityStates.AI;
using RoR2.CharacterAI;
using UnityEngine;
namespace RORAutochess.AI
{
    class BaseTileAIState : BaseAIState
    {
        private TileNavigator navigator;
        public static float range = 20f;
        public override void OnEnter()
        {
            base.OnEnter();
            this.navigator = new TileNavigator(base.body);
            TileNavigator.beforeTileUpdate += RequestAI;
            TileNavigator.afterTileUpdate += UpdateAI;
            
        }

        private void RequestAI()
        {
			BaseAI.SkillDriverEvaluation skillDriverEvaluation = base.ai.skillDriverEvaluation;
			BaseAI.Target target = skillDriverEvaluation.target;
			if ((target != null) ? target.gameObject : null)
			{
				Vector3 vector;
				target.GetBullseyePosition(out vector);
                if((vector - base.body.transform.position).magnitude > range)
				    this.navigator.RequestTileTowardsPosition(vector);
			}
		}

        private void UpdateAI()
        {
			this.bodyInputs.moveVector = Vector3.zero;
			if (!base.body || !base.bodyInputBank)
			{
				return;
			}
			//
			
		}
        public override void FixedUpdate()
        {
            base.FixedUpdate();
                
        }

        public override void OnExit()
        {
            base.OnExit();
            TileNavigator.beforeTileUpdate -= RequestAI;
            TileNavigator.afterTileUpdate -= UpdateAI;
            this.navigator.Dispose();
        }

        public override BaseAI.BodyInputs GenerateBodyInputs(in BaseAI.BodyInputs previousBodyInputs)
        {
            this.bodyInputs.moveVector = Vector3.zero;
            bool pressSkill1 = false;
            bool pressSkill2 = false;
            bool pressSkill3 = false;
            bool pressSkill4 = false;
            BaseAI.Target aimTarget = base.ai.skillDriverEvaluation.aimTarget;
			if (aimTarget != null && this.navigator.navigationEnabled)
			{
				base.AimAt(ref this.bodyInputs, aimTarget);
                pressSkill1 = (aimTarget.bestHurtBox.transform.position - base.body.transform.position).magnitude <= range;

                base.body.moveSpeed = this.navigator.moveVector.magnitude / TileNavigator.updateInterval;
                this.bodyInputs.moveVector = this.navigator.moveVector.normalized;
            }

            this.bodyInputs.pressSkill1 = pressSkill1;

            return base.GenerateBodyInputs(previousBodyInputs);
        }
    }
}
