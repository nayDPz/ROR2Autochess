using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;

namespace RORAutochess.Traits
{
    public class UnitOwnership : MonoBehaviour
    {
        private CharacterMaster master;
        public List<TraitBehavior> activeTraits = new List<TraitBehavior>();
        public MinionOwnership.MinionGroup unitGroup;
        private void Awake()
        {
            this.master = base.GetComponent<CharacterMaster>();
            this.unitGroup = MinionOwnership.MinionGroup.FindGroup(this.master.netId);
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


            this.AddTraitBehavior<TestTrait>(unitGroup.memberCount);
            
        }

        public T AddTraitBehavior<T>(int count) where T : TraitBehavior
        {
            T t = master.gameObject.GetComponent<T>();
            Debug.Log("g");
            if (count > 0)
            {
                if (!t)
                {
                    t = master.gameObject.AddComponent<T>();
                    Debug.Log("g");
                    this.activeTraits.Add(t);
                    t.ownerMaster = this.master;
                    t.unitGroup = this.unitGroup;
                    t.enabled = true;
                    Debug.Log("g");
                }
                
                t.Level = t.GetTraitLevel(count);
                Debug.Log("g");
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
