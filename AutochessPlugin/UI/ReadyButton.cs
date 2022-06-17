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

namespace RORAutochess.UI
{
    public class ReadyButton : MonoBehaviour
    {
        public HGButton button;
        public ChessBoard board;
        private void Start()
        {
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new UnityAction(OnClicked));

            this.board.onPrepPhase += () =>
            {
                base.gameObject.SetActive(true);
            };

            this.board.onCombatPhase += () =>
            {
                base.gameObject.SetActive(false);
            };
        }

        private void OnClicked() 
        {
            base.gameObject.SetActive(false);
            board.ReadyUp();
        }
    }
}
