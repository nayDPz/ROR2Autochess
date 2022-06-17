using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using RORAutochess.Traits;

namespace RORAutochess.Units
{
    public class UnitOwnership : MonoBehaviour
    {
        private CharacterMaster master;
        public List<TraitBehavior> activeTraits = new List<TraitBehavior>();
        public MinionOwnership.MinionGroup unitGroup;
        public ChessBoard ownerBoard;

        private void Awake()
        {
            this.master = base.GetComponent<CharacterMaster>();
            this.unitGroup = MinionOwnership.MinionGroup.FindGroup(this.master.netId);
            this.ownerBoard = ChessBoard.GetBoardFromMaster(this.master);

            RoR2.MinionOwnership.onMinionGroupChangedGlobal += MinionOwnership_onMinionGroupChangedGlobal; // FUCKED PLEASE FIX DOINT FORGET
        }

        private void OnDestroy()
        {
            RoR2.MinionOwnership.onMinionGroupChangedGlobal -= MinionOwnership_onMinionGroupChangedGlobal;
        }
        private void MinionOwnership_onMinionGroupChangedGlobal(MinionOwnership minionOwnership)
        {
            
            if (this.unitGroup == null) this.unitGroup = MinionOwnership.MinionGroup.FindGroup(this.master.netId);
            
            if (this.unitGroup == null) return;

            this.ownerBoard.ownerUnitsOnBoard = new List<CharacterMaster>();
            foreach(MinionOwnership mo in this.unitGroup.members)
            {
                if(mo)
                {
                    CharacterMaster m = mo.GetComponent<CharacterMaster>();
                    if (m)
                    {
                        this.ownerBoard.ownerUnitsOnBoard.Add(m);
                    }
                        
                }
                
            }
        }

        public T AddTraitBehavior<T>(int count) where T : TraitBehavior
        {
            T t = master.gameObject.GetComponent<T>();
            if (count > 0)
            {
                if (!t)
                {
                    t = master.gameObject.AddComponent<T>();
                    this.activeTraits.Add(t);
                    t.ownerMaster = this.master;
                    t.unitGroup = this.unitGroup;
                    t.enabled = true;
                }
                
                t.Level = t.GetTraitLevel(count);
                return t;
            }
            if (t)
            {
                this.activeTraits.Remove(t);
                UnityEngine.Object.Destroy(t);
            }
            return default(T);
        }

    }
}
