using System;
using System.Collections.Generic;
using System.Text;

namespace RORAutochess.Board
{
    class PrepPhase : RoundBaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            this.roundController.roundCount++;
            
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            foreach (ChessBoard board in this.roundController.boards)
            {
                if (!board.ReadyForCombat)
                    return;
            }

            foreach(ChessBoard board in this.roundController.boards)
            {
                board.SetUnitPositions();
            }

            this.outer.SetNextState(new CombatPhase());
            
        }
        public override void OnExit()
        {
            base.OnExit();
            
        }
    }
}
