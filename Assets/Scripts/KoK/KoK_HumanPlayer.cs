using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;
using Chess.UI;
using UnityEngine.InputSystem;

namespace Chess.Players
{
	public class KoK_HumanPlayer : Player
	{
		public enum InputState
		{
			None,
			PieceSelected,
			DraggingPiece
		}

		private InputState currentState;

		private BoardUI _boardUI;
		private Camera _cam;
		private Coord _selectedPieceSquare;
		private Board _board;


		public KoK_HumanPlayer(Board board, KoK_GameManager gameManager)
		{
			_boardUI = GameObject.FindFirstObjectByType<BoardUI>();
			_gameManager = gameManager;
			_cam = Camera.main;
			this._board = board;
		}

		public override void NotifyTurnToMove()
		{
			_thinkingTime = 0f;
		}

		public override void Update()
		{
			_thinkingTime += Time.deltaTime;
			HandleInput();
		}

		void HandleInput()
		{
			Mouse mouse = Mouse.current;
			Vector2 mousePos = _cam.ScreenToWorldPoint(mouse.position.ReadValue());

			if (currentState == InputState.None)
			{
				HandlePieceSelection(mousePos);
			}
			else if (currentState == InputState.DraggingPiece)
			{
				HandleDragMovement(mousePos);
			}
			else if (currentState == InputState.PieceSelected)
			{
				HandlePointAndClickMovement(mousePos);
			}

			if (mouse.rightButton.wasPressedThisFrame)
			{
				CancelPieceSelection();
			}
		}

		void HandlePointAndClickMovement(Vector2 mousePos)
		{
			if (Mouse.current.leftButton.isPressed)
			{
				HandlePiecePlacement(mousePos);
			}
		}

		void HandleDragMovement(Vector2 mousePos)
		{
			_boardUI.DragPiece(_selectedPieceSquare, mousePos);
			// If mouse is released, then try place the piece
			if (Mouse.current.leftButton.wasReleasedThisFrame)
			{
				HandlePiecePlacement(mousePos);
			}
		}

		void HandlePiecePlacement(Vector2 mousePos)
		{
			Coord targetSquare;
			if (_boardUI.TryGetCoordFromPosition(mousePos, out targetSquare))
			{
				if (targetSquare.Equals(_selectedPieceSquare))
				{
					_boardUI.ResetPiecePosition(_selectedPieceSquare);
					if (currentState == InputState.DraggingPiece)
					{
						currentState = InputState.PieceSelected;
					}
					else
					{
						currentState = InputState.None;
						_boardUI.ResetSquareColours();
						_boardUI.HighlightLastMadeMoveSquares(_board);
					}
				}
				else
				{
					int targetIndex = BoardHelper.IndexFromCoord(targetSquare.fileIndex, targetSquare.rankIndex);
					if (Piece.IsColour(_board.Square[targetIndex], _board.MoveColour) && _board.Square[targetIndex] != 0)
					{
						CancelPieceSelection();
						HandlePieceSelection(mousePos);
					}
					else
					{
						TryMakeMove(_selectedPieceSquare, targetSquare);
					}
				}
			}
			else
			{
				CancelPieceSelection();
			}

		}

		void CancelPieceSelection()
		{
			if (currentState != InputState.None)
			{
				currentState = InputState.None;
				_boardUI.ResetSquareColours();
				_boardUI.HighlightLastMadeMoveSquares(_board);
				_boardUI.ResetPiecePosition(_selectedPieceSquare);
			}
		}

		void TryMakeMove(Coord startSquare, Coord targetSquare)
		{
			int startIndex = BoardHelper.IndexFromCoord(startSquare);
			int targetIndex = BoardHelper.IndexFromCoord(targetSquare);
			bool moveIsLegal = false;
			Move chosenMove = new Move();

			MoveGenerator moveGenerator = new MoveGenerator();
			bool wantsKnightPromotion = Keyboard.current[Key.LeftAlt].isPressed;

			var legalMoves = moveGenerator.GenerateMoves(_board);
			for (int i = 0; i < legalMoves.Length; i++)
			{
				var legalMove = legalMoves[i];

				if (legalMove.StartSquare == startIndex && legalMove.TargetSquare == targetIndex)
				{
					if (legalMove.IsPromotion)
					{
						if (legalMove.MoveFlag == Move.PromoteToQueenFlag && wantsKnightPromotion)
						{
							continue;
						}
						if (legalMove.MoveFlag != Move.PromoteToQueenFlag && !wantsKnightPromotion)
						{
							continue;
						}
					}
					moveIsLegal = true;
					chosenMove = legalMove;
					//	Debug.Log (legalMove.PromotionPieceType);
					break;
				}
			}

			if (moveIsLegal)
			{
				lastMoveThinkingTime = _thinkingTime;
				ChoseMove(chosenMove);
				currentState = InputState.None;
			}
			else
			{
				CancelPieceSelection();
			}
		}

		void HandlePieceSelection(Vector2 mousePos)
		{
			if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				if (_boardUI.TryGetCoordFromPosition(mousePos, out _selectedPieceSquare))
				{
					int index = BoardHelper.IndexFromCoord(_selectedPieceSquare);
					// If square contains a piece, select that piece for dragging
					if (Piece.IsColour(_board.Square[index], _board.MoveColour))
					{
						_boardUI.HighlightLegalMoves(_board, _selectedPieceSquare);
						_boardUI.HighlightSquare(_selectedPieceSquare);
						currentState = InputState.DraggingPiece;
					}
				}
			}
		}
	}
}