using System;
using System.Collections.Generic;
using System.Text;

namespace RORAutochess.Board
{
    class PreCombatPhase : RoundBaseState
    {
        public override void OnEnter() // create matchups and send units to enemy boards
        {
            base.OnEnter();

            foreach (ChessBoard board in this.roundController.boards)
            {
                 // for testing
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (true)
            {
                this.outer.SetNextState(new CombatPhase());
            }
        }

        
    }
}
