using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    class InvalidMoveException : Exception
    {
        CheckersPiece movingPiece;
        int attemptedNewX, attemptedNewY;

        public InvalidMoveException(CheckersPiece piece, int newX, int newY)
        {
            movingPiece = piece;

            attemptedNewX = newX;
            attemptedNewY = newY;
        }
    }
}
