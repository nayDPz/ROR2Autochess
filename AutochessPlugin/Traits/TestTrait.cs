using System;
using System.Collections.Generic;
using System.Text;
using RoR2;

namespace RORAutochess.Traits
{
    class TestTrait : TraitBehavior
    {
        private void Awake()
        {
            Chat.AddMessage("TestTrait added " + this.Level.ToString());
        }

        public override void OnLevelChanged(int oldLevel, int newLevel)
        {
            Chat.AddMessage("TestTrait leveled from " + oldLevel + " to " + newLevel);
        }

        private void OnDestroy()
        {
            Chat.AddMessage("TestTrait lost");
        }

        public override int[] GetBreakpoints()
        {
            return new int[] { 2, 4, 6 };
        }
    }
}
