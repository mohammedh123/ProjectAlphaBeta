using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    class InvalidMoveException : Exception
    {
        public CheckersPiece MovingPiece { get; set; }
        public Location AttemptedLocation { get; set; }

        public InvalidMoveException(CheckersPiece piece, int newX, int newY)
        {
            MovingPiece = piece;

            AttemptedLocation = new Location(newX, newY);
        }
    }
}
