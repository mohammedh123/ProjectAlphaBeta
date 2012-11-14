using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    abstract class IDisplayDriver
    {
        public abstract void Draw(Board board, IEnumerable<CheckersPiece> pieces);
        public abstract void DrawGhostPiece(Board board, CheckersPiece ghostPiece, Location pixelCoords);
    }
}
