﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;

namespace Chess.Players
{
	public abstract class Player
	{
		public event System.Action<Move> onMoveChosen;

		public Player opponent;
		protected float _thinkingTime;
		public float lastMoveThinkingTime;
		public abstract void Update();
		public abstract void NotifyTurnToMove();
		protected virtual void ChoseMove(Move move)
		{
			onMoveChosen?.Invoke(move);
		}
	}
}