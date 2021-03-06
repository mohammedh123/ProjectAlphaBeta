﻿// Mohammed Hossain 12/12/12

using System;
using System.Collections.Generic;

namespace CS6613_Final
{
    //MoveResult: an abstraction of all the information needed to perform a move in Checkers: the type of move, the jump result's (if its a jump), and the original/final piece location
    internal class MoveResult
    {
        public MoveType Type { get; set; }
        public List<JumpResult> JumpResults { get; set; } 
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
