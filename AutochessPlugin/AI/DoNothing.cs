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
    class DoNothing : BaseAIState
    {
        public override BaseAI.BodyInputs GenerateBodyInputs(in BaseAI.BodyInputs previousBodyInputs)
        {
            this.bodyInputs.pressSkill1 = false;
            this.bodyInputs.pressSkill2 = false;
            this.bodyInputs.pressSkill3 = false;
            this.bodyInputs.pressSkill4 = false;
            base.body.moveSpeed = 0;
            this.bodyInputs.moveVector = Vector3.zero;

            return base.GenerateBodyInputs(previousBodyInputs);
        }
    }
}
