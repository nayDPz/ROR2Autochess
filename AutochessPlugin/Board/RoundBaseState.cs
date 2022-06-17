using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using EntityStates;

namespace RORAutochess.Board
{
    public class RoundBaseState : BaseState // STATES ARE OBSOLETE
    {
        public RoundController roundController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.roundController = base.GetComponent<RoundController>();
            Chat.AddMessage(this.GetType().Name);
        }

    }
}
