using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;

namespace RORAutochess.AI
{
    public class TileNavigator : IDisposable
    {
        private static List<TileNavigator> instances = new List<TileNavigator>();
        private static float timeSinceLastUpdate;
        public static float updateInterval = 0.75f;
        public static event Action beforeTileUpdate;
        public static event Action afterTileUpdate;

        private CharacterBody owner; // probs should be master
        public Vector3 moveVector;
        public GenericBoard.Tile currentTile;
        public GenericBoard.Tile nextTile;
        public bool navigationEnabled = true;

        static TileNavigator()
        {
            RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
        }

        public TileNavigator(CharacterBody owner)
        {
            this.owner = owner;
            currentTile = GenericBoard.GetClosestTile(owner.footPosition);
            TileNavigator.instances.Add(this);
        }

        public static TileNavigator FindTileNavigatorByMaster(CharacterMaster master)
        {
            foreach(TileNavigator tileNavigator in instances)
            {
                if (tileNavigator.owner.master == master)
                    return tileNavigator;
            }
            return null;
        }

        public void PlaceOnTile(GenericBoard.Tile tile)
        {
            this.currentTile = tile;
            this.navigationEnabled = true;
        }
        
        public void Pickup()
        {
            this.currentTile.occupied = false;
            this.navigationEnabled = false;
        }
        public void Dispose()
        {
            if(this.nextTile != null)
                this.nextTile.occupied = false;
            if(this.currentTile != null)
                this.currentTile.occupied = false;


            TileNavigator.instances.Remove(this);
        }

        private static void RoR2Application_onFixedUpdate()
        {
            timeSinceLastUpdate += Time.fixedDeltaTime;
            if(timeSinceLastUpdate >= updateInterval)
            {
                timeSinceLastUpdate -= updateInterval;
                UpdateInstances();
            }
        }

        public static void UpdateInstances()
        {
            if(beforeTileUpdate != null)
                beforeTileUpdate.Invoke();

            if(instances.Count > 0)
            {
                foreach (TileNavigator tileNavigationSystem in instances)
                {
                    if(tileNavigationSystem.navigationEnabled)
                        tileNavigationSystem.MoveTowardsTile();
                }
            }       
            
            if(afterTileUpdate != null)
                afterTileUpdate.Invoke();
        }

        public bool RequestTileTowardsPosition(Vector3 position)
        {
            this.nextTile = this.currentTile.GetClosestConnectedTile(position);
            this.nextTile.occupied = true;
            this.currentTile.occupied = false;

            return false;
        }

        private void MoveTowardsTile()
        {
            if(this.nextTile != null)
            {
                //this.owner.GetComponent<CharacterMotor>().AddDisplacement(this.nextTile.worldPosition - this.owner.footPosition);
                this.moveVector = (this.nextTile.worldPosition - this.owner.footPosition);//this.currentTile.worldPosition);//.normalized;
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
