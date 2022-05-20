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
        public CharacterMaster owner;
        public CharacterBody ownerBody;
        public ChessBoard.Tile currentTile;

        public bool navigationEnabled;
        public bool benched;
        public bool inCombat;

        public ChessBoard currentBoard;


        private void Awake()
        {
            if (!this.owner)
                this.owner = base.GetComponent<CharacterMaster>();
        }

        private void Start()
        {
            if(this.owner.minionOwnership.group != null)
                this.currentBoard = ChessBoard.GetBoardFromMaster(this.owner.minionOwnership.group.resolvedOwnerMaster);
            else
                this.currentBoard = ChessBoard.instancesList[0]; // for testing remove later


            this.ownerBody = this.owner.GetBody();
        }

        public void Bench()
        {
            this.benched = true;
            this.navigationEnabled = false;
        }
        public void Unbench()
        {
            this.benched = false;
            this.navigationEnabled = true;
        }


        public void SetCurrentTile(ChessBoard.Tile tile) 
        {
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
            if(this.currentTile != null)
                this.currentTile.occupied = false;


        }


    }
}
