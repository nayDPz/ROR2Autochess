using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;

namespace RORAutochess.Board
{
    public class RoundController : MonoBehaviour
    {
        public static RoundController instance;

        public static GameObject prefab;
        public static void Init()
        {
            prefab = AutochessPlugin.hud.LoadAsset<GameObject>("RoundController");
            EntityStateMachine e = prefab.GetComponent<EntityStateMachine>();
            e.mainStateType = new EntityStates.SerializableEntityStateType(typeof(PrepPhase));
            e.initialStateType = new EntityStates.SerializableEntityStateType(typeof(PrepPhase));
        }

        public List<GenericBoard> boards;

        public float currentPhaseDuration;
        public float currentPhaseTime;

        public float prepDuration = 30f;
        public float combatDuration = 60f;
        public float preCombatDuration = 5f;
        public float postCombatDuration = 3f;


        private void FixedUpdate()
        {
            if (this.currentPhaseDuration > 0f)
                this.currentPhaseTime += Time.fixedDeltaTime; // probably can do state.fixedage
        }

        private void Awake()
        {
            if (!instance) instance = this;
            else Destroy(this);

            this.boards = GenericBoard.instancesList;
        }
    }
}
