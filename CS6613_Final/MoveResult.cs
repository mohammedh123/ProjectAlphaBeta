using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    internal class MoveResult
    {
        public MoveType Type { get; set; }
        public IEnumerable<JumpResult> JumpResults { get; set; } 
        public Location OriginalPieceLocation { get; set; }
        public Location FinalPieceLocation { get; set; }

        public MoveResult(MoveType type, CheckersPiece piece, int nx, int ny)
        {
            Type = type;
            OriginalPieceLocation = new Location(piece.X, piece.Y);
            FinalPieceLocation = new Location(nx, ny);

            JumpResults = new List<JumpResult>();
        }

        public override string ToString()
        {
            return String.Format("{0} to {1}, Type: {2}", OriginalPieceLocation, FinalPieceLocation, Type);
        }
    }
}
