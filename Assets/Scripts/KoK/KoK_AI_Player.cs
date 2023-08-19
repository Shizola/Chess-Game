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
        private float _thinkingTimer;

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
                if (_thinkingTimer < _thinkingTime)
                {
                    _thinkingTimer += Time.deltaTime;
                    return;
                }

                _moveFound = false;
                ChoseMove(_move);
            }
        }

        void OnSearchComplete(string moveString)
        {
            _thinkingTimer = 0f;
         
            // Adjust thinking time based on centipawn score and player's thinking time
            float adjustedThinkingTime = Mathf.Clamp(opponent.lastMoveThinkingTime - _madChessController.centipawnScore * 0.05f, 2f, 10f);

            // Update AI's last thinking time
            _thinkingTime = adjustedThinkingTime;
            lastMoveThinkingTime = adjustedThinkingTime;

            Debug.Log("Adjusted Thinking Time: " + adjustedThinkingTime);

            Debug.Log("MadChess search complete, move is: " + moveString);
            Move move = MoveUtility.MoveFromName(moveString, _board);
            this._move = move;
            _moveFound = true;
        }


    }
}