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
            this.roundController.currentPhaseDuration = this.roundController.preCombatDuration;
            foreach (ChessBoard board in this.roundController.boards)
            {
                board.CreateEnemyTeam(board.CreatePVERound()); // for testing
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.roundController.preCombatDuration)
            {
                this.outer.SetNextState(new CombatPhase());
            }
        }

        
    }
}
