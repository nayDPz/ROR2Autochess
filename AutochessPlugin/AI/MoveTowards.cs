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
using RORAutochess.Units;
namespace RORAutochess.AI
{
    class MoveTowards : BaseAIState
    {
        private TileNavigator navigator;
        private EntityStateMachine bodyStateMachine;
        private UnitData data;

        public Vector3 target;
        private float distance;
        public override void OnEnter()
        {
            base.OnEnter();
            this.data = base.GetComponent<UnitData>();
            this.navigator = base.GetComponent<TileNavigator>();
            this.bodyStateMachine = base.body.GetComponents<EntityStateMachine>().Where(x => x.customName == "Body").FirstOrDefault();

            this.target = this.navigator.nextTile.worldPosition;
            Vector3 v = this.target;
            v.y = 0;
            Vector3 v2 = base.body.gameObject.transform.position;
            v2.y = 0;
            this.distance = (v - v2).magnitude;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= data.movementDuration)
                this.navigator.ArriveAt(this.navigator.nextTile);
                
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override BaseAI.BodyInputs GenerateBodyInputs(in BaseAI.BodyInputs previousBodyInputs)
        {
            this.bodyInputs.pressSkill1 = false;
            this.bodyInputs.pressSkill2 = false;
            this.bodyInputs.pressSkill3 = false;
            this.bodyInputs.pressSkill4 = false;

            base.body.moveSpeed = this.distance / this.data.movementDuration;
            this.bodyInputs.moveVector = this.target - base.body.gameObject.transform.position;

            return base.GenerateBodyInputs(previousBodyInputs);
        }
    }
}
