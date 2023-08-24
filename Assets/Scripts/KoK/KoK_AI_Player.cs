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
        private KoK_PieceData_King _kok_pieceData_king;
        private Board _board;
        private Move _move;
        private MoveGenerator _moveGenerator;
        private bool _moveFound;
        private KoK_AI_Player_CurrentThinkingTimes _currentThinkingTimes;
        private float _thinkingTimer;
        private float _randomBlunderChance;
        private bool _needToUpdateRandomValues;

        private int _materialAfterLastMove;

        public KoK_AI_Player(Board board, MadChessController madChessController, KoK_PieceData_King king, KoK_GameManager gameManager)
        {
            _madChessController = madChessController;
            _madChessController.onSearchComplete += OnSearchComplete;
            _board = board;
            _moveGenerator = new MoveGenerator();
            _gameManager = gameManager;
            _kok_pieceData_king = king;
            _currentThinkingTimes = new KoK_AI_Player_CurrentThinkingTimes();
            RandomiseThinkingTime();
            RandomiseBlunderChance();
            _materialAfterLastMove = _gameManager.blackMaterial;
        }

        public override void NotifyTurnToMove()
        {
            Debug.Log("AI Player's turn to move");
            _madChessController.SendPosition(FenUtility.CurrentFen(_board));
        }

        public override void Update()
        {
            if (_needToUpdateRandomValues)
            {
                _needToUpdateRandomValues = false;
                RandomiseThinkingTime();
                RandomiseBlunderChance();
            }

            if (_moveFound)
            {
                if (_thinkingTime > _thinkingTimer)
                {
                    _thinkingTimer += Time.deltaTime;
                    return;
                }

                CheckForBlunder();

                _thinkingTimer = 0f;
                _moveFound = false;
                ChoseMove(_move);
                _needToUpdateRandomValues = true;
                _materialAfterLastMove = _gameManager.blackMaterial;
            }
        }

        void OnSearchComplete(string moveString)
        {
            // get move ready
            Move move = MoveUtility.MoveFromName(moveString, _board);
            this._move = move;

            // get thinking time
            _thinkingTimer = 0f;
            _thinkingTime = 0f;
            _thinkingTime = GetSimulatedThinkingTime();

            // Update AI's last thinking time
            lastMoveThinkingTime = _thinkingTime;

            Debug.Log("Adjusted Thinking Time: " + _thinkingTime);
            Debug.Log("MadChess search complete, move is: " + moveString);

            // set move found to true
            _moveFound = true;
        }


        private float GetSimulatedThinkingTime()
        {
            float adjustedThinkingTime = 0f;

            int centipawnScore = _madChessController.centipawnScore;
            //  float playerLastThinkingTime = opponent.lastMoveThinkingTime;

            if (centipawnScore <= -1000)
            {
                Debug.Log("Extremely Worse Position");
                adjustedThinkingTime = _currentThinkingTimes.ExtremelyWorse;
            }
            else if (centipawnScore <= -500 && centipawnScore > -1000)
            {
                Debug.Log("Much Worse Position");
                adjustedThinkingTime = _currentThinkingTimes.MuchWorse;
            }
            else if (centipawnScore <= -150 && centipawnScore > -500)
            {
                Debug.Log("Worse Position");
                adjustedThinkingTime = _currentThinkingTimes.Worse;
            }
            else if (centipawnScore <= -50 && centipawnScore > -150)
            {
                adjustedThinkingTime = _currentThinkingTimes.SlightlyWorse;
                Debug.Log("Slightly Worse Position");
            }
            else if (centipawnScore < 50 && centipawnScore > -50)
            {
                adjustedThinkingTime = _currentThinkingTimes.Neutral;
                Debug.Log("Neutral Position");
            }
            else if (centipawnScore >= 50 && centipawnScore < 150)
            {
                adjustedThinkingTime = _currentThinkingTimes.SlightlyBetter;
                Debug.Log("Slightly Better Position");
            }
            else if (centipawnScore >= 150 && centipawnScore < 500)
            {
                adjustedThinkingTime = _currentThinkingTimes.Better;
                Debug.Log("Better Position");
            }
            else if (centipawnScore >= 500 && centipawnScore < 1000)
            {
                adjustedThinkingTime = _currentThinkingTimes.MuchBetter;
                Debug.Log("Much Better Position");
            }
            else if (centipawnScore >= 1000)
            {
                adjustedThinkingTime = _currentThinkingTimes.ExtremelyBetter;
                Debug.Log("Extremely Better Position");
            }

            //has lost material since last move
            if (_materialAfterLastMove > _gameManager.blackMaterial)
            {
                // simulate the upcoming move before it's made to see if it wins back material
                Board tempBoard = new Board();
                tempBoard.LoadPosition(FenUtility.CurrentFen(_board));
                tempBoard.MakeMove(_move);

                int tempHumanMaterial = _gameManager.evaluation.CountMaterial(0, tempBoard);
                int tempAIMaterial = _gameManager.evaluation.CountMaterial(1, tempBoard);

                int humanMaterial = _gameManager.evaluation.CountMaterial(0, _board);
                int AIMaterial = _gameManager.evaluation.CountMaterial(1, _board);

                int currentMaterialDifference = humanMaterial - AIMaterial;
                int tempMaterialDifference = tempHumanMaterial - tempAIMaterial;


                if (tempMaterialDifference < 0 || tempMaterialDifference < currentMaterialDifference)
                {
                    Debug.Log("AI has reduced the material difference.");
                    // if it does win back material, reduce thinking time
                    adjustedThinkingTime = _currentThinkingTimes.ExtremelyBetter;
                }
                else
                {
                    Debug.Log("AI's material situation has not changed significantly.");
                }
            }
            return adjustedThinkingTime;
        }

        private void RandomiseThinkingTime()
        {
            switch (_kok_pieceData_king.battleStyle)
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

        private void RandomiseBlunderChance()
        {
            _randomBlunderChance = Random.Range(0, 100);
        }

        private void CheckForBlunder()
        {
            if (_kok_pieceData_king.blunderChance >= _randomBlunderChance)
            {
                Debug.Log("AI has blundered.");

                var moves = _moveGenerator.GenerateMoves(_board);
                if (moves.Length > 0)
                {
                    _move = moves[new System.Random().Next(moves.Length)];
                }
                else
                {
                    Debug.LogWarning("No moves available.");
                }
            }
        }
    }
}