// Mohammed Hossain 12/12/12

using System.Collections.Generic;

namespace CS6613_Final
{
    // an abstract class that represents a class that draws a Checkers game to some form of output, be it to a GUI or to the console
    internal abstract class DisplayDriver
    {
        public abstract void Draw(Board board, List<CheckersPiece> playerOnePieces, List<CheckersPiece> playerTwoPieces, List<MoveResult> availableMoves, CheckersPiece selectedPiece = null);
    }
}