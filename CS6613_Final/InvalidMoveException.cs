// Mohammed Hossain 12/12/12

using System;

namespace CS6613_Final
{
    //an exception that signifies an invalid move
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