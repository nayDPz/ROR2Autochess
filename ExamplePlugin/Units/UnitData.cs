using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;

namespace RORAutochess.Units
{
    public class UnitData : MonoBehaviour
    {
        public string unitName = "Unit";
        public float cost = 25;
        public float sellPrice = 17;
        public int level = 1;


        private void Awake()
        {
            //this.master = base.GetComponent<CharacterMaster>();
            //this.bodyObject = this.master.bodyPrefab;
            //this.unitName = this.bodyObject.GetComponent<CharacterBody>().baseNameToken;

        }

        public CharacterMaster master;
        public GameObject bodyObject;

        //list of traits (somehow)
    }
}
