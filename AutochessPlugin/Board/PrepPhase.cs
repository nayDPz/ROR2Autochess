using System;
using System.Collections.Generic;
using System.Text;

namespace RORAutochess.Board
{
    class PrepPhase : RoundBaseState
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.roundController.currentPhaseDuration = this.roundController.prepDuration;
            if (base.fixedAge >= this.roundController.prepDuration)
            {
                foreach (ChessBoard board in this.roundController.boards)
                {
                    board.SetUnitPositions();
                }
                this.outer.SetNextState(new PreCombatPhase());
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            
        }
    }
}
