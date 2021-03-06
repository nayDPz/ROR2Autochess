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
        public CharacterMaster master;
        public CharacterBody body;
        public ChessBoard.Tile currentTile;
        public ChessBoard currentBoard;
        public bool inCombat;

        private void Awake()
        {
            if (!this.master)
                this.master = base.GetComponent<CharacterMaster>();
        }

        private void Start()
        {
            if(this.master)
            {
                if (this.master.minionOwnership.group != null)
                    this.currentBoard = ChessBoard.GetBoardFromMaster(this.master.minionOwnership.group.resolvedOwnerMaster);
                else
                    this.currentBoard = ChessBoard.instancesList[0]; // for testing remove later


                this.body = this.master.GetBody();
            }
            
        }

        private void FixedUpdate()
        {
            foreach(RoR2.CharacterAI.BaseAI ai in this.master.aiComponents)
            {
                if (ai) ai.enabled = this.currentBoard.inCombat;
            }
        }

        public void SetCurrentTile(ChessBoard.Tile tile) 
        {
            this.currentTile = tile;
            this.currentTile.occupant = this;
        }
        
        public void Pickup()
        {
            if(this.currentTile != null)
            {
                this.currentTile.occupant = null;
                this.currentTile = null;
            }             
        }
        private void OnDestroy()
        {
            if(this.currentTile != null)
                this.currentTile.occupant = null;


        }


    }
}
