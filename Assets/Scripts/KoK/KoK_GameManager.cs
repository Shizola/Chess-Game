using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.UI;
using Chess.Core;
using Chess.Players;
using Chess.Game;
using System;
using UnityEngine.InputSystem;

public class KoK_GameManager : MonoBehaviour
{
    private MadChessController _madChessController;

    private BoardUI _boardUI;
    public Board board { get; private set; }

    [Header("Start Position")]
    public bool loadCustomPosition;
    public string customPosition = "1rbq1r1k/2pp2pp/p1n3p1/2b1p3/R3P3/1BP2N2/1P3PPP/1NBQ1RK1 w - - 0 1";

    public enum PlayerType { Human, AI }
    [Header("Players")]
    public PlayerType whitePlayerType = PlayerType.Human;
    public PlayerType blackPlayerType = PlayerType.AI;

    public KoK_WorldKingAttributes opponentKingAttributes;

    private Player _humanPlayer;
    private Player _aiPlayer;
    private Player _playerToMove;

    public event System.Action onPositionLoaded;
    public event System.Action<Move> onMoveMade;

    // Internal stuff
    GameResult.Result gameResult;

    [Header("Debug")]
    public string currentFen;

    // Start is called before the first frame update
    void Start()
    {
        _madChessController = FindFirstObjectByType<MadChessController>();

        _boardUI = FindFirstObjectByType<BoardUI>();
        board = new Board();

        _madChessController.onUCIok += OnUCIok;
        _madChessController.onIsReady += OnMadChessIsReady;

        NewGame(whitePlayerType, blackPlayerType);
    }


    private void NewGame(PlayerType whitePlayerType, PlayerType blackPlayerType)
    {
        if (loadCustomPosition)
        {
            currentFen = customPosition;
            board.LoadPosition(customPosition);
            // searchBoard.LoadPosition(customPosition);
        }
        else
        {
            currentFen = FenUtility.StartPositionFEN;
            board.LoadStartPosition();
            //searchBoard.LoadStartPosition();
        }

        onPositionLoaded?.Invoke();
        _boardUI.UpdatePosition(board);
        _boardUI.ResetSquareColours();

        CreatePlayer(ref _humanPlayer, whitePlayerType);
        CreatePlayer(ref _aiPlayer, blackPlayerType);

        _humanPlayer.opponent = _aiPlayer;
        _aiPlayer.opponent = _humanPlayer;

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
            player = new HumanPlayer(board);
        }
        else
        {
            player = new KoK_AI_Player(board, _madChessController, opponentKingAttributes);
        }
        player.onMoveChosen += OnMoveChosen;
    }

    private void OnMoveChosen(Move move)
    {
        //PlayMoveSound(move);

        // bool animateMove = _playerToMove is AIPlayer;

        board.MakeMove(move);

        // searchBoard.MakeMove(move);

        currentFen = FenUtility.CurrentFen(board);
        onMoveMade?.Invoke(move);

        _boardUI.UpdatePosition(board, move, true);

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
        _madChessController.NewGame(opponentKingAttributes.skillElo);
        _madChessController.CheckIsReady();
    }

    private void OnMadChessIsReady()
    {
        Debug.Log("MadChess is ready");

        if (loadCustomPosition)
            _madChessController.SendCommand("position fen " + customPosition);

        NotifyPlayerToMove();
        gameResult = GameResult.Result.Playing;
    }
}