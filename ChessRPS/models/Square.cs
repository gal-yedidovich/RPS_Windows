using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Client.models
{
	public class Square
	{
		public SquareType Type { get; set; }
		public (int row, int col) Position { get; set; }
		public bool MyRPS { get; set; }
		//public ISqareState State { get; set; }

		public Square(int row, int col)
		{
			Position = (row, col);
			Type = SquareType.Empty;

		}

		/// <summary>
		/// compare two sqaures in battle and return winner or null if draw
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Square Resolve(Square attacker, Square target)
		{
			var a = attacker.Type;
			var t = target.Type;
			if (t == SquareType.Flag) return attacker; //attacker won - game won

			if (t == SquareType.Trap) return target; //attacker lost

			//validation check
			if (t != SquareType.Rock && t != SquareType.Paper && t != SquareType.Scissors) throw new ArgumentException("target's type must be: Flag, Trap, Rock, Paper or Scissors");
			if (a != SquareType.Rock && a != SquareType.Paper && a != SquareType.Scissors) throw new ArgumentException("attacker's type must be: Flag, Trap, Rock, Paper or Scissors");

			//comparing types RPS

			switch (a - t)
			{
				case 1:
				case -2:
					return attacker;
				case -1:
				case 2:
					return target;
				default:
					return null; //draw
			}

		}

		//states - my soldier, empty, opponent soldier, flag, trap, ---selected, highlighted---
		//isMoveToTarget (when state is opponent soldier/empty then true - otherwise false)
		//getMoves (returns list of points (unless is not movable))
		//isMoveable (when self is my soldier)
		//MoveToMe (is result as battle/movement/victory)

		//public bool IsMoveToTarget()
		//{
		//	return State.IsMoveToTarget();
		//}

		//public bool IsMovable()
		//{
		//	return State.IsMovable();
		//}
	}

	//enum MoveResult
	//{
	//	Mevement, Battle, Victory,
	//	Trap
	//}

	//interface ISqareState
	//{
	//	bool IsMoveToTarget();
	//	bool IsMovable();

	//	MoveResult MoveToMe(Square pawn);
	//}

	//class SoldierState : ISqareState
	//{
	//	private Square context;

	//	public bool IsMovable()
	//	{
	//		return true;
	//	}

	//	public bool IsMoveToTarget()
	//	{
	//		return false;
	//	}

	//	public MoveResult MoveToMe(Square pawn)
	//	{

	//		return MoveResult.Battle;
	//	}
	//}

	//class TrapState : ISqareState
	//{
	//	public bool IsMovable() => false;

	//	public bool IsMoveToTarget() => false;

	//	public MoveResult MoveToMe(Square pawn)
	//	{
	//		return MoveResult.Trap;
	//	}
	//}

	public enum SquareType
	{
		Empty, Flag, Rock, Paper, Scissors, UnknownOpponent, Trap
	}
}
