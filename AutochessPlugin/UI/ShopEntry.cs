using System;
using System.Collections.Generic;
using System.Text;
using RORAutochess.Units;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoR2.UI;
using UnityEngine.Events;
using RoR2;
using UnityEngine.Networking;

namespace RORAutochess.UI
{
    public class ShopEntry : MonoBehaviour
    {
        public UnitData unitData;
        public CharacterMaster unitMaster;

        public string nameString;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI cost;
        public TextMeshProUGUI[] traits;
        public RawImage icon;
        public CharacterMaster source;

        public HGButton button;

        private void Awake()
        {
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new UnityAction(OnButtonClicked));
        }
        private void Start()
        {
            if (!nameText) nameText = base.transform.Find("UnitName").GetComponent<TextMeshProUGUI>(); // temp fix idk why it becomes null
            if(unitData)
            {
                unitMaster = unitData.master;
                if (nameText) nameText.text = Language.GetString(unitData.unitName);
                if (cost) cost.text = unitData.cost.ToString();
                if (icon) icon.texture = unitData.master.bodyPrefab.GetComponent<CharacterBody>().portraitIcon;
            }
            
        }

        private void Update()
        {

        }

        private void OnButtonClicked()
        {
            if(!unitMaster) unitMaster = unitData.master;


            CharacterMaster player = source;

            ChessBoard board = ChessBoard.GetBoardFromMaster(player);
            if(board != null)
            {
                ChessBoard.Tile tile = board.GetLowestUnoccupiedTile(board.tiles);
                if(tile != null)
                {
                    CharacterMaster m = new MasterSummon
                    {
                        masterPrefab = unitMaster.gameObject,
                        summonerBodyObject = player.GetBodyObject(),
                        ignoreTeamMemberLimit = true,
                        inventoryToCopy = null,
                        useAmbientLevel = new bool?(true),
                        position = tile.worldPosition + Vector3.up,
                        rotation = Quaternion.identity,
                        
                    }.Perform();
                    m.destroyOnBodyDeath = false;

                    m.GetComponent<RoR2.CharacterAI.BaseAI>().enabled = false;
                    AI.TileNavigator t = m.GetComponent<AI.TileNavigator>();

                    if (!t) // probably shouldnt do this
                    {
                        t = m.gameObject.AddComponent<AI.TileNavigator>();
                    }

                    t.currentBoard = board;
                    t.SetCurrentTile(tile);
                }
                else
                {
                    return;
                }               
            }
            

            Destroy(base.gameObject);
        }
    }
}
