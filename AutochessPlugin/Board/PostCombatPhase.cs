using System;
using System.Collections.Generic;
using System.Text;

namespace RORAutochess.Board
{
    class PostCombatPhase : RoundBaseState
    {
        // hand out rewards, deal damage to losing player

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (true)
            {
                foreach (ChessBoard board in this.roundController.boards)
                {
                    board.ResetBoard();
                }
                this.outer.SetNextState(new PrepPhase());
            }
        }

        public override void OnExit()
        {

        }


    }
}
