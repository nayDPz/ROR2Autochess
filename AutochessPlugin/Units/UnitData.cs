using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using EntityStates;

namespace RORAutochess.Units
{
    public class UnitData : MonoBehaviour
    {
        public string unitName = "Unit";
        public float cost = 25;
        public float sellPrice = 17;
        public int level = 1;
        public int attackRange = 1;
        public float movementDuration = 0.5f; // time it takes to move between tiles
        public EntityState attackState;

        //list of traits (somehow)

        private void Awake()
        {
            //this.master = base.GetComponent<CharacterMaster>();
            //this.bodyObject = this.master.bodyPrefab;
            //this.unitName = this.bodyObject.GetComponent<CharacterBody>().baseNameToken;

        }

        public CharacterMaster master;
        public GameObject bodyObject;

        
    }
}
