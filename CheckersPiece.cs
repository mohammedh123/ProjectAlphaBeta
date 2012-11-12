using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    class CheckersPiece
    {
        public bool InPlay { get; set; }
        public PieceColor Color { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }

        public PieceDirection Forward { get; private set; }

        public CheckersPiece(int x, int y, PieceColor color, PieceDirection forward)
        {
            X = x;
            Y = y;

            Color = color;
            InPlay = true;
            Forward = forward;
        }
    }
}
