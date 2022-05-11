using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using RORAutochess.Units;

namespace RORAutochess.AI
{
    public class TileNavigator : MonoBehaviour
    {
        private static List<TileNavigator> instances = new List<TileNavigator>(); // list probably doesnt need to exist
        private static float timeSinceLastUpdate;
        public static float updateInterval = 0.75f;
        public static event Action beforeTileUpdate;
        public static event Action afterTileUpdate;

        public CharacterMaster owner;
        public CharacterBody ownerBody;
        public int attackRange = 1; 

        public Vector3 moveVector;
        public GenericBoard.Tile previousTile;
        public GenericBoard.Tile currentTile;
        public GenericBoard.Tile nextTile;

        public bool navigationEnabled;
        public bool benched;
        public bool inCombat;

        public GenericBoard currentBoard;

        static TileNavigator()
        {
            RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
        }

        private static void RoR2Application_onFixedUpdate() // non-static is probably a good idea
        {
            timeSinceLastUpdate += Time.fixedDeltaTime;
            if (timeSinceLastUpdate >= updateInterval)
            {
                timeSinceLastUpdate -= updateInterval;
                UpdateInstances();
            }
        }

        public static void UpdateInstances()
        {
            if (beforeTileUpdate != null)
                beforeTileUpdate.Invoke(); // probably want large units to move first ( if they exist )

            if (instances.Count > 0)
            {
                foreach (TileNavigator tn in instances) // for movement
                {
                    if (tn.navigationEnabled && tn.inCombat)
                        tn.MoveTowardsTile();
                }
            }

            if (afterTileUpdate != null)
                afterTileUpdate.Invoke(); // check attack range
        }

        private void Awake()
        {
            if (!this.owner)
                this.owner = base.GetComponent<CharacterMaster>();

            UnitData data = base.GetComponent<UnitData>();
            if(data)
            {
                this.attackRange = data.attackRange;
            }

            TileNavigator.instances.Add(this);
        }

        private void Start()
        {
            if(this.owner.minionOwnership.group != null)
                this.currentBoard = GenericBoard.GetBoardFromMaster(this.owner.minionOwnership.group.resolvedOwnerMaster);
            else
                this.currentBoard = GenericBoard.instancesList[0]; // for testing remove later


            this.ownerBody = this.owner.GetBody();
        }
        public static TileNavigator FindTileNavigator(CharacterMaster master)
        {
            foreach(TileNavigator tileNavigator in instances)
            {
                if (tileNavigator.owner == master)
                    return tileNavigator;
            }
            return null;
        }

        public void Bench()
        {
            this.benched = true;
            this.navigationEnabled = false;
            //on benched
        }
        public void Unbench()
        {
            this.benched = false;
            this.navigationEnabled = true;
            // on unbenched
        }

        public void SetCurrentTile(GenericBoard.Tile tile) 
        {
            this.nextTile = null;
            this.currentTile = tile;
            this.currentTile.occupied = true;
            bool bench = false;

            for(int i = 0; i < currentBoard.benchTiles.Length; i++) // probably bad
            {
                if (tile == currentBoard.benchTiles[i])
                {
                    bench = true;
                    break;
                }              
            }

            if(!bench && this.benched)
            {
                this.Unbench();
            }
            else if(bench && !this.benched)
            {
                this.Bench();
            }
            
        }
        
        public void Pickup()
        {
            if(this.currentTile != null)
                this.currentTile.occupied = false;

            this.navigationEnabled = false;
        }
        private void OnDestroy()
        {
            if(this.nextTile != null)
                this.nextTile.occupied = false;
            if(this.currentTile != null)
                this.currentTile.occupied = false;

            TileNavigator.instances.Remove(this);
        }

        public GenericBoard.Tile RequestTileTowards(Vector3 position)
        {
            if (this.currentTile == null) this.currentTile = this.currentBoard.GetClosestTile(this.ownerBody.footPosition);

            this.nextTile = this.currentTile.GetTileIgnore(position, this.previousTile);
            this.nextTile.occupied = true;
            this.currentTile.occupied = false;

            return this.nextTile;

        }

        private void MoveTowardsTile()
        {
            if(this.nextTile != null)
            {
                this.moveVector = (this.nextTile.worldPosition - this.ownerBody.footPosition);
                this.previousTile = this.currentTile;
                this.currentTile = this.nextTile;
                this.nextTile = null;
            }
            else
            {
                this.moveVector = Vector3.zero;
            }
            
        }


    }
}
