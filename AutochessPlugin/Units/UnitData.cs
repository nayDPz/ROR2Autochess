using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using EntityStates;
using RORAutochess.AI;

namespace RORAutochess.Units
{
    public class UnitData : MonoBehaviour
    {
        public string unitName = "Unit";
        public float cost = 25;
        public float sellPrice = 17;
        public int level = 1;
        public int attackRange = 1;
        public int tileIndex;


        //list of traits (somehow)

        private void Start()
        {
            this.navigator = base.GetComponent<TileNavigator>();
            //this.master = base.GetComponent<CharacterMaster>();
            //this.bodyObject = this.master.bodyPrefab;
            //this.unitName = this.bodyObject.GetComponent<CharacterBody>().baseNameToken;

        }

        public TileNavigator navigator;
        public CharacterMaster master;
        public GameObject bodyObject;

        
    }
}
