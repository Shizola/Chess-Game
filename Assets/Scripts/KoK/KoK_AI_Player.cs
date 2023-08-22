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
        private KoK_WorldKingAttributes _kok_kingAttributes;
        private Board _board;
        private Move _move;
        private bool _moveFound;

        private KoK_AI_Player_CurrentThinkingTimes _currentThinkingTimes;
        private float _thinkingTimer;
        private bool _needToUpdateThinkingTime;

        private int _materialAfterLastMove;
        private bool _winningBackMaterial;

        public KoK_AI_Player(Board board, MadChessController madChessController, KoK_WorldKingAttributes attributes, KoK_GameManager gameManager)
        {
            _board = board;
            _madChessController = madChessController;
            _madChessController.onSearchComplete += OnSearchComplete;
            _gameManager = gameManager;
            _kok_kingAttributes = attributes;
            _currentThinkingTimes = new KoK_AI_Player_CurrentThinkingTimes();            
            RandomiseThinkingTime();
            _materialAfterLastMove = _gameManager.blackMaterial;
        }

        public override void NotifyTurnToMove()
        {
            Debug.Log("AI Player's turn to move");
            _madChessController.SendPosition(FenUtility.CurrentFen(_board));
        }

        public override void Update()
        {
            if (_needToUpdateThinkingTime)
            {
                _needToUpdateThinkingTime = false;
                RandomiseThinkingTime();
            }

            if (_moveFound)
            {
                if (_thinkingTime > _thinkingTimer)
                {
                    _thinkingTimer += Time.deltaTime;
                    return;
                }

                _thinkingTimer = 0f;
                _moveFound = false;
                ChoseMove(_move);
                _needToUpdateThinkingTime = true;
                _materialAfterLastMove = _gameManager.blackMaterial;
            }
        }

        void OnSearchComplete(string moveString)
        {
            // get move ready
            Move move = MoveUtility.MoveFromName(moveString, _board);
            this._move = move;
            _moveFound = true;


            // get thinking time

            _thinkingTime = 0f;
            _thinkingTime = GetSimulatedThinkingTime();

            // Update AI's last thinking time
            lastMoveThinkingTime = _thinkingTime;

            Debug.Log("Adjusted Thinking Time: " + _thinkingTime);
            Debug.Log("MadChess search complete, move is: " + moveString);

        }
        

        private float GetSimulatedThinkingTime()
        {
            float adjustedThinkingTime;

            int centipawnScore = _madChessController.centipawnScore;
            float playerLastThinkingTime = opponent.lastMoveThinkingTime;

            if (centipawnScore <= -1000) 
            {
                adjustedThinkingTime = Mathf.Clamp(playerLastThinkingTime +_currentThinkingTimes.ExtremelyWorse, 1f, 10f);
                Debug.Log("Extremely Worse Position");
            }
            else if (centipawnScore < -500) 
            {
                adjustedThinkingTime = Mathf.Clamp(playerLastThinkingTime + _currentThinkingTimes.MuchWorse, 1f, 10f);
                Debug.Log("Much Worse Position");
            }
            else if (centipawnScore < -350)
            {
                adjustedThinkingTime = Mathf.Clamp(playerLastThinkingTime + _currentThinkingTimes.Worse, 1f, 10f);
                Debug.Log("Worse Position");
            }
            else if (centipawnScore < -150)
            {
                adjustedThinkingTime = Mathf.Clamp(playerLastThinkingTime + _currentThinkingTimes.SlightlyWorse, 1f, 10f);
                Debug.Log("Slightly Worse Position");
            }
            else if (centipawnScore < 50 && centipawnScore > -50) 
            {
                adjustedThinkingTime = Mathf.Clamp(playerLastThinkingTime + _currentThinkingTimes.Neutral, 1f, 10f);
                Debug.Log("Neutral Position");
            }
            else if (centipawnScore >= 50 && centipawnScore < 150) 
            {
                adjustedThinkingTime = Mathf.Clamp(playerLastThinkingTime + _currentThinkingTimes.Better, 1f, 10f);
                Debug.Log("Slightly Better Position");
            }
            else if (centipawnScore >= 150 && centipawnScore < 350) 
            {
                adjustedThinkingTime = Mathf.Clamp(playerLastThinkingTime + _currentThinkingTimes.SlightlyBetter, 1f, 10f);
                Debug.Log("Better Position");
            }  
            else if (centipawnScore >= 350 && centipawnScore < 500)
            {
                adjustedThinkingTime = Mathf.Clamp(playerLastThinkingTime + _currentThinkingTimes.SlightlyBetter, 1f, 10f);
                Debug.Log("Much Better Position");
            }
            else if (centipawnScore >= 500 && centipawnScore < 1000) 
            {
                adjustedThinkingTime = Mathf.Clamp(playerLastThinkingTime + _currentThinkingTimes.MuchBetter, 1f, 10f);
                Debug.Log("Much Better Position");
            }
            else
            {
                adjustedThinkingTime = Mathf.Clamp(playerLastThinkingTime + _currentThinkingTimes.ExtremelyBetter, 1f, 10f);
                Debug.Log("Extremely Better Position");
            }

            //has lost material since last move
            if (_materialAfterLastMove > _gameManager.blackMaterial)
            {
                _winningBackMaterial = true;

                // simulate the upcoming move before it's made to see if it wins back material
                Board tempBoard = new Board();
                tempBoard.LoadPosition(FenUtility.CurrentFen(_board));
                tempBoard.MakeMove(_move);
                int tempMaterial = _gameManager.evaluation.CountMaterial(1, tempBoard);

                if (tempMaterial > _materialAfterLastMove)
                {
                    // if it does win back material, reduce thinking time
                    adjustedThinkingTime = Mathf.Clamp(adjustedThinkingTime / 2f, 1f, 10f);
                    Debug.Log("Winning back material");
                }
            }
           

            return adjustedThinkingTime;
        }

        private void RandomiseThinkingTime()
        {
            switch (_kok_kingAttributes.battleStyle)
            {
                case BattleStyle.Aggressive:
                case BattleStyle.Balanced:
                case BattleStyle.Cautious:
                    _currentThinkingTimes.ExtremelyWorse = Random.Range(KoK_AI_Player_ThinkingTimes.balanced_ExtremelyWorse[0], KoK_AI_Player_ThinkingTimes.balanced_ExtremelyWorse[1]);
                    _currentThinkingTimes.MuchWorse = Random.Range(KoK_AI_Player_ThinkingTimes.balanced_MuchWorse[0], KoK_AI_Player_ThinkingTimes.balanced_MuchWorse[1]);
                    _currentThinkingTimes.Worse = Random.Range(KoK_AI_Player_ThinkingTimes.balanced_Worse[0], KoK_AI_Player_ThinkingTimes.balanced_Worse[1]);
                    _currentThinkingTimes.SlightlyWorse = Random.Range(KoK_AI_Player_ThinkingTimes.balanced_SlightlyWorse[0], KoK_AI_Player_ThinkingTimes.balanced_SlightlyWorse[1]);
                    _currentThinkingTimes.Neutral = Random.Range(KoK_AI_Player_ThinkingTimes.balanced_Neutral[0], KoK_AI_Player_ThinkingTimes.balanced_Neutral[1]);
                    _currentThinkingTimes.SlightlyBetter = Random.Range(KoK_AI_Player_ThinkingTimes.balanced_SlightlyBetter[0], KoK_AI_Player_ThinkingTimes.balanced_SlightlyBetter[1]);
                    _currentThinkingTimes.Better = Random.Range(KoK_AI_Player_ThinkingTimes.balanced_Better[0], KoK_AI_Player_ThinkingTimes.balanced_Better[1]);
                    _currentThinkingTimes.MuchBetter = Random.Range(KoK_AI_Player_ThinkingTimes.balanced_MuchBetter[0], KoK_AI_Player_ThinkingTimes.balanced_MuchBetter[1]);
                    _currentThinkingTimes.ExtremelyBetter = Random.Range(KoK_AI_Player_ThinkingTimes.balanced_ExtremelyBetter[0], KoK_AI_Player_ThinkingTimes.balanced_ExtremelyBetter[1]);
                    break;
                default:
                    break;
            }
        }
    }
}