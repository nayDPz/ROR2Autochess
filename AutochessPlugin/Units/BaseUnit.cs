using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace RORAutochess.Units
{
    public class BaseUnit
    {
        private string unitName; 
        private string unitToClone = ""; 
        private static void CreateUnit(string unitName, string unitToClone)
        {
            if (unitToClone == null || unitToClone == "")
                unitToClone = unitName;

            GameObject masterClone = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/" + unitToClone + "/" + unitToClone + "Master.prefab").WaitForCompletion();
            if(!masterClone)
            {
                Log.LogError(unitToClone + " is null, " + unitName + " will not be created.");
                return;
            }

            CharacterMaster master = masterClone.GetComponent<CharacterMaster>();
            Units.UnitData unitData = master.gameObject.AddComponent<Units.UnitData>();
            GameObject bodyObject = master.bodyPrefab;
            CharacterBody body = bodyObject.GetComponent<CharacterBody>();

            unitData.master = master;
            unitData.bodyObject = master.bodyPrefab;
            unitData.unitName = body.baseNameToken;
            Debug.Log(unitData.bodyObject.name);

            master.gameObject.AddComponent<AI.TileNavigator>();

            if (master.GetComponent<RoR2.CharacterAI.BaseAI>())
                master.GetComponent<RoR2.CharacterAI.BaseAI>().fullVision = true;

            if (master.GetComponent<EntityStateMachine>())
            {
                master.GetComponent<EntityStateMachine>().initialStateType = new EntityStates.SerializableEntityStateType(typeof(RORAutochess.AI.BaseTileAIState));
                master.GetComponent<EntityStateMachine>().mainStateType = new EntityStates.SerializableEntityStateType(typeof(RORAutochess.AI.BaseTileAIState));
            }


            if (bodyObject && !bodyObject.GetComponent<Units.UnitPickupInteraction>())
            {
                bodyObject.AddComponent<Units.UnitPickupInteraction>();
                body.baseAcceleration = 999;
                body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            }

        }

        public enum Traits : uint
        {

        }
    }


    
}
