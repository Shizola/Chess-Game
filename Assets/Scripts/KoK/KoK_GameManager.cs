using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.UI;
using Chess.Core;
using Chess.Players;
using Chess.Game;
using System;
using UnityEngine.InputSystem;
using NUnit.Framework.Internal;

public class KoK_GameManager : MonoBehaviour
{
    private MadChessController _madChessController;

    private BoardUI _boardUI;
    public Board board { get; private set; }

    public enum PlayerType { Human, AI }
    [Header("Players")]
    public PlayerType whitePlayerType = PlayerType.Human;
    public PlayerType blackPlayerType = PlayerType.AI;

    public KoK_BattleSequence battleSequence;

    private Player _humanPlayer;
    private Player _aiPlayer;
    private Player _playerToMove;

    public event System.Action onPositionLoaded;
    public event System.Action<Move> onMoveMade;

    public int whiteMaterial;
    public int blackMaterial;

    // Internal stuff
    GameResult.Result gameResult;
    public Evaluation evaluation;
    public int moveCount;


    [Header("Debug")]
    public string currentFen;

    // Start is called before the first frame update
    void Start()
    {
        _madChessController = FindFirstObjectByType<MadChessController>();

        _boardUI = FindFirstObjectByType<BoardUI>();
        board = new Board();
        evaluation = new Evaluation();

        _madChessController.onUCIok += OnUCIok;
        _madChessController.onIsReady += OnMadChessIsReady;

        NewGame(whitePlayerType, blackPlayerType, battleSequence.battles[0].startingFenPosition);
    }


    private void NewGame(PlayerType whitePlayerType, PlayerType blackPlayerType, string fenPosition)
    {
        board.LoadPosition(fenPosition);

        onPositionLoaded?.Invoke();
        _boardUI.UpdatePosition(board);
        _boardUI.ResetSquareColours();

        CreatePlayer(ref _humanPlayer, whitePlayerType);
        CreatePlayer(ref _aiPlayer, blackPlayerType);

        _humanPlayer.opponent = _aiPlayer;
        _aiPlayer.opponent = _humanPlayer;

        whiteMaterial = evaluation.CountMaterial(0, board);
        blackMaterial = evaluation.CountMaterial(1, board);

        StartCoroutine(StartUCICheck());
    }

    IEnumerator StartUCICheck()
    {
        // yield end of frame to allow UCI to be ready;
        yield return new WaitForEndOfFrame();
        _madChessController.CheckUCI();
    }

    private void CreatePlayer(ref Player player, PlayerType playerType)
    {
        if (player != null)
        {
            player.onMoveChosen -= OnMoveChosen;
        }

        if (playerType == PlayerType.Human)
        {
            player = new KoK_HumanPlayer(board, this);
        }
        else
        {
            player = new KoK_AI_Player(board, _madChessController, battleSequence.king, this);
        }
        player.onMoveChosen += OnMoveChosen;
    }

    private void OnMoveChosen(Move move)
    {
        //PlayMoveSound(move);

        // bool animateMove = _playerToMove is AIPlayer;
        moveCount++;
        board.MakeMove(move);

        // searchBoard.MakeMove(move);

        currentFen = FenUtility.CurrentFen(board);
        onMoveMade?.Invoke(move);

        _boardUI.UpdatePosition(board, move, true);

        whiteMaterial = evaluation.CountMaterial(0, board);
        blackMaterial = evaluation.CountMaterial(1, board);
        NotifyPlayerToMove();
    }

    private void NotifyPlayerToMove()
    {
        gameResult = GameResult.GetGameState(board);

        if (gameResult == GameResult.Result.Playing)
        {
            _playerToMove = board.IsWhiteToMove ? _humanPlayer : _aiPlayer;
            _playerToMove.NotifyTurnToMove();
        }
        else
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over " + gameResult);
    }

    // Update is called once per frame
    private void Update()
    {
        HandleInput();
        UpdateGame();
    }

    private void HandleInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard[Key.U].wasPressedThisFrame)
        {
            if (board.AllGameMoves.Count > 0)
            {
                Move moveToUndo = board.AllGameMoves[^1];
                board.UnmakeMove(moveToUndo);
                //searchBoard.UnmakeMove(moveToUndo);
                _boardUI.UpdatePosition(board);
                _boardUI.ResetSquareColours();
                _boardUI.HighlightLastMadeMoveSquares(board);

                //PlayMoveSound(moveToUndo);

                gameResult = GameResult.GetGameState(board);
                //PrintGameResult(gameResult);
            }
        }

        if (keyboard[Key.E].wasPressedThisFrame)
        {
            //ExportGame();
        }


        if (keyboard[Key.N].wasPressedThisFrame)
        {
            Debug.Log("Make Null Move");
            board.MakeNullMove();
        }
    }

    private void UpdateGame()
    {
        if (gameResult == GameResult.Result.Playing)
        {
            _playerToMove.Update();
        }
    }

    private void OnUCIok()
    {
        Debug.Log("UCI is ready");
        _madChessController.NewGame(battleSequence.king.baseSkillElo);
        _madChessController.CheckIsReady();
    }

    private void OnMadChessIsReady()
    {
        Debug.Log("MadChess is ready");

        _madChessController.SendCommand("position fen " + battleSequence.battles[0].startingFenPosition);

        NotifyPlayerToMove();
        gameResult = GameResult.Result.Playing;
    }
}