using System.Collections.Generic;

namespace CS6613_Final
{
    internal abstract class DisplayDriver
    {
        public abstract void Draw(Board board, List<CheckersPiece> playerOnePieces, List<CheckersPiece> playerTwoPieces, CheckersPiece selectedPiece = null);
    }
}