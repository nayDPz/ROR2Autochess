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
        public CharacterMaster unitMaster;

        public string nameString;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI cost;
        public TextMeshProUGUI[] traits;
        public RawImage icon;
        public CharacterMaster source;
        public ShopSlot slot;
        public HGButton button;
        

        private void Awake()
        {
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new UnityAction(OnButtonClicked));

        }
        private void Start()
        {
            if (!nameText) nameText = base.transform.Find("UnitName").GetComponent<HGTextMeshProUGUI>(); // temp fix idk why it becomes null

            if (nameText) nameText.text = Language.GetString(this.unitMaster.bodyPrefab.GetComponent<CharacterBody>().baseNameToken);
            if (icon) icon.texture = this.unitMaster.bodyPrefab.GetComponent<CharacterBody>().portraitIcon;

            this.source = slot.source;
            
        }

        public virtual void OnButtonClicked()
        {
            if (!this.source) this.source = this.slot.source; // ???????????????????????????????????????????????????????????????????????????



           

            ChessBoard board = ChessBoard.GetBoardFromMaster(this.source);
            if(board != null)
            {
                PodShop p = this.slot.shop.GetComponent<PodShop>();
                if (p)
                {
                    board.DeployUnit(this.unitMaster, board.GetClosestTile(p.podObject.transform.position)); // ?????????IM SO FUCKING STUPID????????????????
                    Destroy(this.slot.shop.gameObject);
                }
                else
                    board.DeployUnit(this.unitMaster, board.GetLowestUnoccupiedTile(board.tiles));            
            }
            

            Destroy(base.gameObject);
        }
    }
}
