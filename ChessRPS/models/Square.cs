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

        public Square(int row, int col)
        {
            Position = (row, col);
            Type = SquareType.Empty;
        }

    }

    public enum SquareType
    {
        Empty, Flag, Rock, Paper, Scissors, UnknownOpponent, Trap
    }
}
