using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    class SetOfPieces
    {
        public List<CheckersPiece> AlivePlayerOnePieces { get; set; }
        public List<CheckersPiece> AlivePlayerTwoPieces { get; set; }

        public List<CheckersPiece> CapturedPlayerOnePieces { get; set; }
        public List<CheckersPiece> CapturedPlayerTwoPieces { get; set; }

        public SetOfPieces(SetOfPieces obj)
        {
            AlivePlayerOnePieces = new List<CheckersPiece>();
            AlivePlayerTwoPieces = new List<CheckersPiece>();

            CapturedPlayerOnePieces = new List<CheckersPiece>();
            CapturedPlayerTwoPieces = new List<CheckersPiece>();

            for (var i = 0; i < obj.AlivePlayerOnePieces.Count; i++)
            {
                var p = obj.AlivePlayerOnePieces[i];

                AlivePlayerOnePieces.Add(new CheckersPiece(p.X, p.Y, p.Color, p.Forward));
            }

            for (var i = 0; i < obj.AlivePlayerTwoPieces.Count; i++)
            {
                var p = obj.AlivePlayerTwoPieces[i];

                AlivePlayerTwoPieces.Add(new CheckersPiece(p.X, p.Y, p.Color, p.Forward));
            }

            for (var i = 0; i < obj.CapturedPlayerOnePieces.Count; i++)
            {
                var p = obj.CapturedPlayerOnePieces[i];

                CapturedPlayerOnePieces.Add(new CheckersPiece(p.X, p.Y, p.Color, p.Forward));
            }

            for (var i = 0; i < obj.CapturedPlayerTwoPieces.Count; i++)
            {
                var p = obj.CapturedPlayerTwoPieces[i];

                CapturedPlayerTwoPieces.Add(new CheckersPiece(p.X, p.Y, p.Color, p.Forward));
            }
        }

        public SetOfPieces()
        {
            AlivePlayerOnePieces = new List<CheckersPiece>();
            AlivePlayerTwoPieces = new List<CheckersPiece>();
            CapturedPlayerOnePieces = new List<CheckersPiece>();
            CapturedPlayerTwoPieces = new List<CheckersPiece>();
        }

        public void AddPiece(PieceColor color, int x, int y, PieceDirection forward)
        {
            var listToAdd = color == PieceColor.Black ? AlivePlayerOnePieces : AlivePlayerTwoPieces;

            listToAdd.Add(new CheckersPiece(x, y, color, forward));
        }

        public CheckersPiece GetPieceAtPosition(int x, int y)
        {
            return GetPieceAtPosition(new Location(x, y));
        }

        public CheckersPiece GetPieceAtPosition(Location loc)
        {
            CheckersPiece retVal;

            for (var i = 0; i < AlivePlayerOnePieces.Count; i++)
            {
                retVal = AlivePlayerOnePieces[i];
                if (retVal.X == loc.X && retVal.Y == loc.Y)
                    return retVal;
            }

            for (var i = 0; i < AlivePlayerTwoPieces.Count; i++)
            {
                retVal = AlivePlayerTwoPieces[i];
                if (retVal.X == loc.X && retVal.Y == loc.Y)
                    return retVal;
            }

            return null;
        }

        public CheckersPiece GetCapturedPieceForColorAtLocation(Location loc, PieceColor col)
        {
            var listToCheck = col == PieceColor.Black ? CapturedPlayerOnePieces : CapturedPlayerTwoPieces;

            CheckersPiece p;
            for (int i = 0; i < listToCheck.Count; i++)
            {
                p = listToCheck[i];

                if (p.Color == col && p.X == loc.X && p.Y == loc.Y)
                    return p;
            }

            return null;
        }

        public List<CheckersPiece> GetPiecesForColor(PieceColor col)
        {
            var listToCheck = col == PieceColor.Black ? AlivePlayerOnePieces : AlivePlayerTwoPieces;
            var retList = new List<CheckersPiece>();

            for (int i = 0; i < listToCheck.Count; i++)
            {
                var p = listToCheck[i];
                retList.Add(p);
            }

            return retList;
        }

        public CheckersPiece GetPieceForColorAtLocation(Location loc, PieceColor col)
        {
            var listToCheck = col == PieceColor.Black ? AlivePlayerOnePieces : AlivePlayerTwoPieces;

            CheckersPiece p;
            for (int i = 0; i < listToCheck.Count; i++)
            {
                p = listToCheck[i];

                if (p.Color == col && p.X == loc.X && p.Y == loc.Y)
                    return p;
            }

            return null;
        }

        public void KillPiece(CheckersPiece killedPiece, int timestamp)
        {
            killedPiece.InPlay = false;
            killedPiece.Timestamp = timestamp;
            
            var listToKill = (killedPiece.Color == PieceColor.Black ? AlivePlayerOnePieces : AlivePlayerTwoPieces);
            var listToAdd = (killedPiece.Color == PieceColor.Black ? CapturedPlayerOnePieces : CapturedPlayerTwoPieces);

            for (int i = 0; i < listToKill.Count; i++)
            {
                var p = listToKill[i];
                if (p.X == killedPiece.X && p.Y == killedPiece.Y)
                {
                    listToKill.RemoveAt(i);
                    break;
                }
            }

            listToAdd.Add(killedPiece);
        }

        public void RevivePiece(CheckersPiece ressedPiece)
        {
            ressedPiece.InPlay = true;
            ressedPiece.Timestamp = 0;

            var listToKill = (ressedPiece.Color == PieceColor.Black ? CapturedPlayerOnePieces : CapturedPlayerTwoPieces);
            var listToAdd = (ressedPiece.Color == PieceColor.Black ? AlivePlayerOnePieces : AlivePlayerTwoPieces);

            for (int i = 0; i < listToKill.Count; i++)
            {
                var p = listToKill[i];
                if (p.X == ressedPiece.X && p.Y == ressedPiece.Y)
                {
                    listToKill.RemoveAt(i);
                    break;
                }
            }

            listToAdd.Add(ressedPiece);
        }
    }
}
