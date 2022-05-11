using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using EntityStates;

namespace RORAutochess.Board
{
    public class RoundBaseState : BaseState
    {
        public RoundController roundController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.roundController = base.GetComponent<RoundController>();
            this.roundController.currentPhaseTime = 0f;
            Chat.AddMessage(this.GetType().Name);
        }

    }
}
