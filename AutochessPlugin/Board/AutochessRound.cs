using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;

namespace RORAutochess.Board
{
    public class AutochessRound : MonoBehaviour
    {
        public static Action onRoundCombatFinished;

        public SceneDirector sceneDirector;
        public CombatDirector combatDirector;
        public CombatSquad combatSquad;

        public RoundController roundController;

        public float secondsRemaining
        {
            get
            {
                if (Run.instance && this.timerStart > 0f)
                {
                    return Mathf.Max(0f, this.secondsAfterRound - (Run.instance.GetRunStopwatch() - this.timerStart));
                }
                return this.secondsAfterRound;
            }
        }

        public float secondsAfterRound = 2f;

        public float baseRoundDuration = 15f;
        private float timerStart;
        private bool timerStarted;
        private bool combatStarted;

        private float squadDefeatTimer;
        private float squadDefeatGracePeriod = 1f;

        public float initialCreditFraction = 0.5f;
        public float bonusMonsterCredits;
        public float bonusSceneCredits;
        private float totalMonsterCredits;
        private float totalSceneCredits;

        private float creditsPerSecond;


        private float totalCreditsSpent;

        public bool shouldSpawnTeleporter;
        public bool shouldSpawnSurvivorPod;

        public bool isFinished;
        public void Initialize(int sceneCredits)
        {
            Log.LogInfo("Initializing round with " + sceneCredits + " credits");
            this.roundController = RoundController.instance;

            this.totalSceneCredits = (sceneCredits + this.bonusSceneCredits);
            if(this.sceneDirector)
            {
                
                this.sceneDirector.enabled = true;
                
                SceneDirector.onPrePopulateSceneServer += SceneDirector_onPrePopulateSceneServer;
                
            }
        }

        private void SceneDirector_onPrePopulateSceneServer(SceneDirector director)
        {
            if(!director.gameObject.GetComponent<AutochessRound>())
            {
                Log.LogInfo("Blocking bad SceneDirector on " + director.gameObject.name);
                director.interactableCredit = 0;
                director.monsterCredit = 0;
                director.teleporterSpawnCard = null;
            }
            else
            {

                if(this.shouldSpawnSurvivorPod)
                {
                    foreach (ChessBoard board in ChessBoard.instancesList) // bad
                    {

                        Log.LogInfo("Spawning survivor pod for " + board.name);
                        int i = UnityEngine.Random.RandomRangeInt(0, board.tiles.Length / 2); // bad bad

                        while (board.tiles[i].occupied)
                            i++;

                        board.CreatePodShop(board.tiles[i]);
                    }
                }
                

                director.interactableCredit = (int)this.totalSceneCredits;
                director.monsterCredit = 0;
                director.teleporterSpawnCard = this.shouldSpawnTeleporter ? Stuff.teleporterSpawnCard : null;

                Log.LogInfo("Spawning interactables with " + director.interactableCredit + " credits");
            }          
        }


        public void StartCombat(int monsterCredits)
        {
            Log.LogInfo("Starting combat with " + monsterCredits + " credits");
            this.combatStarted = true;
            this.totalMonsterCredits = (monsterCredits + this.bonusMonsterCredits) * Run.instance.difficultyCoefficient;
            this.creditsPerSecond = Mathf.Max(0.1f, 1f - this.initialCreditFraction) * this.totalMonsterCredits / this.baseRoundDuration;
            if (this.combatDirector)
            {
                this.combatDirector.enabled = true;
                this.combatDirector.monsterSpawnTimer = 0f;

                this.combatDirector.monsterCredit += this.initialCreditFraction * this.totalMonsterCredits;
                this.combatDirector.currentSpawnTarget = ChessBoard.instancesList[0].gameObject; // spawntarget ??
            }
        }

        private void FixedUpdate()
        {
            if(this.combatDirector)
            {
                this.totalCreditsSpent = this.combatDirector.totalCreditsSpent;
            }
            if(!this.isFinished && this.combatStarted)
            {
                if(this.combatDirector)
                {
                    if(this.combatDirector.totalCreditsSpent < this.totalMonsterCredits)
                    {
                        this.combatDirector.monsterCredit += Time.fixedDeltaTime * this.creditsPerSecond;
                    }
                    else
                    {
                        if (this.combatSquad.memberCount == 0)
                        {
                            if (this.squadDefeatTimer <= 0f && !this.timerStarted)
                            {
                                this.StartTimer();
                            }
                            else
                            {
                                this.squadDefeatTimer -= Time.fixedDeltaTime;
                            }
                        }
                    }
                }
            }
            if (this.timerStarted && this.secondsRemaining <= 0) this.OnRoundFinished();

        }

        private void OnEnable()
        {
            On.RoR2.CombatDirector.Spawn += CombatDirector_Spawn;
            if (this.combatSquad)
            {
                this.combatSquad.onMemberDiscovered += this.OnCombatSquadMemberDiscovered;
            }
            this.squadDefeatTimer = this.squadDefeatGracePeriod;
        }

        // kinda cringe i guess but it works
        private bool CombatDirector_Spawn(On.RoR2.CombatDirector.orig_Spawn orig, CombatDirector self, SpawnCard spawnCard, EliteDef eliteDef, Transform spawnTarget, DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, float valueMultiplier, DirectorPlacementRule.PlacementMode placementMode)
        {
            placementMode = DirectorPlacementRule.PlacementMode.Random;
            return orig(self, spawnCard, eliteDef, spawnTarget, spawnDistance, preventOverhead, valueMultiplier, placementMode);
        }

        private void OnDisable()
        {
            On.RoR2.CombatDirector.Spawn -= CombatDirector_Spawn;
            if (this.combatSquad)
            {
                this.combatSquad.onMemberDiscovered -= this.OnCombatSquadMemberDiscovered;
            }
        }



        private void OnCombatSquadMemberDiscovered(CharacterMaster master)
        {
            this.squadDefeatTimer = this.squadDefeatGracePeriod;
        }

        private void ForceFinish()
        {
            if (this.combatDirector) this.combatDirector.monsterCredit = 0f;
            this.OnRoundFinished();
            if (this.combatSquad)
            {
                foreach (CharacterMaster characterMaster in new List<CharacterMaster>(this.combatSquad.readOnlyMembersList))
                {
                    characterMaster.TrueKill();
                }
            }
        }
        private void StartTimer()
        {
            Log.LogInfo("Enemies dead, timer started");
            this.timerStarted = true;
            this.timerStart = Run.instance.GetRunStopwatch();
        }
        private void OnRoundFinished()
        {
            this.isFinished = true;

            Log.LogInfo("Finished Round");

            if (AutochessRound.onRoundCombatFinished != null)
                AutochessRound.onRoundCombatFinished.Invoke();
            // rewards ?
        }
        public void OnDestroy()
        {
            SceneDirector.onPrePopulateSceneServer -= SceneDirector_onPrePopulateSceneServer;
        }

    }
}
