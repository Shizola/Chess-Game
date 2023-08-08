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
        private MadChessController _madChessController;
        private Board _board;
        private Move _move;
        private bool _moveFound;

        public KoK_AI_Player(Board board, MadChessController madChessController)
        {
            _board = board;
            _madChessController = madChessController;
            _madChessController.onSearchComplete += OnSearchComplete;
        }


        public override void NotifyTurnToMove()
        {
            Debug.Log("AI Player's turn to move");

            _madChessController.SendPosition(FenUtility.CurrentFen(_board));

        }

        public override void Update()
        {
            if (_moveFound)
            {
                _moveFound = false;
                ChoseMove(_move);
            }
        }

        void OnSearchComplete(string moveString)
        {
            Debug.Log("MadChess search complete, attempting move: " + moveString);
            Move move = MoveUtility.MoveFromName(moveString, _board);
            this._move = move;
            _moveFound = true;
        }
    }
}