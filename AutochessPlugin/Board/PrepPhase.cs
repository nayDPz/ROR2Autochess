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
            this.roundController.stageCount++;
            foreach (ChessBoard board in this.roundController.boards)
            {
                int i = UnityEngine.Random.RandomRangeInt(0, board.tiles.Length / 2); // bad bad

                while (board.tiles[i].occupied)
                    i++;

                board.CreatePodShop(board.tiles[i]);
            }
        }
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
