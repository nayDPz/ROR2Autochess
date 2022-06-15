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

            this.roundController.currentPhaseDuration = this.roundController.combatDuration;

            foreach (ChessBoard board in this.roundController.boards)
            {
                if (board.onCombatPhase != null)
                    board.onCombatPhase.Invoke();
                board.SetCombat(true); 
            }
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

        public override void OnExit()
        {
            
        }


    }
}
