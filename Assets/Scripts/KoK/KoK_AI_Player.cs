using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;
using Chess.Game;
using Chess.UI;
using UnityEngine.InputSystem;

namespace Chess.Players
{
    public class KoK_AI_Player : Player
    {
        private Board _board;

        public KoK_AI_Player(Board board)
        {
           _board = board;
        }

        public override void NotifyTurnToMove()
        {
            Debug.Log("AI Player's turn to move");
        }

        public override void Update()
        {
            //throw new System.NotImplementedException();
        }
    }

}