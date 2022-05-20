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
            this.roundController.currentPhaseDuration = this.roundController.postCombatDuration;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.roundController.postCombatDuration)
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
