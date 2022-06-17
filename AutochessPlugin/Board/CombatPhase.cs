using System;
using System.Collections.Generic;
using System.Text;

namespace RORAutochess.Board
{
    class CombatPhase : RoundBaseState
    {
        // track unit deaths, dmg dealt

        public override void OnEnter()
        {
            base.OnEnter();


            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            foreach (ChessBoard board in this.roundController.boards)
            {
                if (board.inCombat)
                    return;
            }

            this.outer.SetNextState(new PostCombatPhase());
            
        }



    }
}
