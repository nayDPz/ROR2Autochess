using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
namespace RORAutochess.Traits
{
    public class TraitBehavior : MonoBehaviour
    {
        public CharacterMaster ownerMaster;
        public MinionOwnership.MinionGroup unitGroup;

        private int _level;
        public int Level
        {
            get => _level;
            set
            {
                if (_level != value)
                    OnLevelChanged(_level, value);
                _level = value;
                
            }
        }


        public int GetTraitLevel(int count)
        {
            int level = 0;
            int[] breakpoints = GetBreakpoints();

            if (breakpoints.Length == 0) return 1;
            for (int i = 0; i < breakpoints.Length; i++)
                if (count >= breakpoints[i]) level++;

            

            return level;
        }

        public virtual void OnLevelChanged(int oldLevel, int newLevel)
        {
        }

        public virtual int[] GetBreakpoints()
        {
            return Array.Empty<int>();
        }
    }
}
