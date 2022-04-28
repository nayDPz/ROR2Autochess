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
    class StandAttack : BaseAIState
    {
        private TileNavigator navigator;
        private EntityStateMachine bodyStateMachine;
        private UnitData data;

        public GameObject target;
        public override void OnEnter()
        {
            base.OnEnter();
            this.data = base.GetComponent<UnitData>();
            this.navigator = base.GetComponent<TileNavigator>();
            this.bodyStateMachine = base.body.GetComponents<EntityStateMachine>().Where(x => x.customName == "Body").FirstOrDefault();

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override BaseAI.BodyInputs GenerateBodyInputs(in BaseAI.BodyInputs previousBodyInputs)
        {
            this.bodyInputs.pressSkill1 = true;
            this.bodyInputs.pressSkill2 = false;
            this.bodyInputs.pressSkill3 = false;
            this.bodyInputs.pressSkill4 = false; // idk

            this.bodyInputs.desiredAimDirection = this.target.transform.position - base.body.gameObject.transform.position;
            base.body.moveSpeed = 0;
            this.bodyInputs.moveVector = Vector3.zero;

            return base.GenerateBodyInputs(previousBodyInputs);
        }
    }
}
