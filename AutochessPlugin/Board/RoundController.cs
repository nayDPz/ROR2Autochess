using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using UnityEditor;

namespace RORAutochess.Board
{
    public class RoundController : MonoBehaviour
    {
        public static RoundController instance;

        public static GameObject prefab;
        public static GameObject basicRound;
        public static GameObject teleporterRound;
        public static void Init()
        {
            prefab = AutochessPlugin.assetbundle.LoadAsset<GameObject>("RoundController");
            basicRound = AutochessPlugin.assetbundle.LoadAsset<GameObject>("acrBasic");
            teleporterRound = AutochessPlugin.assetbundle.LoadAsset<GameObject>("acrTeleporter");

            //EntityStateMachine e = prefab.GetComponent<EntityStateMachine>();
            //e.mainStateType = new EntityStates.SerializableEntityStateType(typeof(PrepPhase));
            //e.initialStateType = new EntityStates.SerializableEntityStateType(typeof(PrepPhase));
        }

        public List<ChessBoard> boards;


        public int roundCount;
        public int stageCount;

        public AutochessRound currentRound;

        public int stageCredits = 300; // L
        public int roundCredits;
        public int roundsInStage = 6;



        private void Awake()
        {
            if (!instance) instance = this;
            else Destroy(this);
            

            this.boards = ChessBoard.instancesList;

            
        }

        public void CreateRound()
        {
            if (this.currentRound) GameObject.Destroy(this.currentRound.gameObject);

            this.roundCount++;
            foreach (ChessBoard board in ChessBoard.instancesList) // bad
            {

                Log.LogInfo("Creating round for " + board.name);
                board.ResetBoard();

                if (board.onPrepPhase != null)
                    board.onPrepPhase.Invoke();

            }

            this.currentRound = GameObject.Instantiate(basicRound, base.transform).GetComponent<AutochessRound>();
            if (this.roundCount == this.roundsInStage)
                this.currentRound.shouldSpawnTeleporter = true;
            if (this.roundCount == 2)
                this.currentRound.shouldSpawnSurvivorPod = true;

            this.currentRound.Initialize(50); /////////////////?

        }

        private void OnBoardReady(ChessBoard board)
        {
            foreach (ChessBoard b in ChessBoard.instancesList)
            {
                if (!b.ReadyForCombat)
                    return;
            }

            Chat.AddMessage("All boards ready");
            this.StartCombat();

        }

        private void StartCombat()
        {

            foreach (ChessBoard board in ChessBoard.instancesList)
            {
                board.SetCombat(true);

                if (board.onCombatPhase != null)
                    board.onCombatPhase.Invoke();
            }

            if (this.currentRound)
                this.currentRound.StartCombat(75);
            else
                Log.LogError("No Round :(");
        }

        private void OnEnable()
        {
            AutochessRound.onRoundCombatFinished += CreateRound;
            ChessBoard.onBoardReady += OnBoardReady;
            RoR2.Stage.onStageStartGlobal += AddOneToStageCount;
        }

        private void OnDisable()
        {
            AutochessRound.onRoundCombatFinished -= CreateRound;
            ChessBoard.onBoardReady -= OnBoardReady;
            RoR2.Stage.onStageStartGlobal -= AddOneToStageCount;
        }

        private void AddOneToStageCount(RoR2.Stage obj)
        {
            this.stageCount++;
        }

        
    }
    
}
