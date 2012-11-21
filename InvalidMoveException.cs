using System;

namespace CS6613_Final
{
    internal class InvalidMoveException : Exception
    {
        public InvalidMoveException(CheckersPiece piece, int newX, int newY)
        {
            MovingPiece = piece;

            AttemptedLocation = new Location(newX, newY);
        }

        public CheckersPiece MovingPiece { get; set; }
        public Location AttemptedLocation { get; set; }
    }
}