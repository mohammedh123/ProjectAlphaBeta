using System.Collections.Generic;

namespace CS6613_Final
{
    internal abstract class DisplayDriver
    {
        public abstract void Draw(Board board, IEnumerable<CheckersPiece> pieces, CheckersPiece selectedPiece = null);
    }
}