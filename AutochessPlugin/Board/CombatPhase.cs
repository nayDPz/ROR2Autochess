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

            foreach (GenericBoard board in this.roundController.boards)
            {
                board.SetCombat(true); 
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.roundController.combatDuration)
            {
                foreach (GenericBoard board in this.roundController.boards)
                {
                    board.SetCombat(false);
                }
                this.outer.SetNextState(new PostCombatPhase());
            }
        }

        public override void OnExit()
        {
            
        }


    }
}
