using System;
using System.Collections.Generic;
using System.Linq;

namespace CS6613_Final
{
    internal class CheckersBoard
    {
        public Board TileBoard { get; set; }
        public List<CheckersPiece> PlayerOnePieces { get; set; }
        public List<CheckersPiece> PlayerTwoPieces { get; set; }

        public IEnumerable<CheckersPiece> CapturedPieces
        {
            get { return PlayerOnePieces.Where(p => !p.InPlay).Union(PlayerTwoPieces.Where(p => !p.InPlay)); }
        }

        public IEnumerable<CheckersPiece> InPlayPieces
        {
            get { return PlayerOnePieces.Where(p => p.InPlay).Union(PlayerTwoPieces.Where(p => p.InPlay)); }
        }

        public CheckersBoard Clone()
        {
            var clone = new CheckersBoard
                            {
                                TileBoard = TileBoard,
                                PlayerOnePieces = new List<CheckersPiece>(),
                                PlayerTwoPieces = new List<CheckersPiece>()
                            };

            foreach (var piece in PlayerOnePieces)
            {
                clone.PlayerOnePieces.Add(new CheckersPiece(piece.X, piece.Y, piece.Color, piece.Forward) { InPlay = piece.InPlay });
            }

            foreach (var piece in PlayerTwoPieces)
            {
                clone.PlayerTwoPieces.Add(new CheckersPiece(piece.X, piece.Y, piece.Color, piece.Forward) { InPlay = piece.InPlay });
            }

            return clone;
        }

        public CheckersPiece GetPieceAtPosition(int x, int y)
        {
            return InPlayPieces.SingleOrDefault(c => c.X == x && c.Y == y);
        }

        public IEnumerable<Location> LocationsBetweenDiagonals(Location one, Location two)
        {
            var locs = new List<Location>();

            var dispX = Math.Abs(two.X - one.X);
            var signX = Math.Sign(dispX);
            var dispY = Math.Abs(two.Y - one.Y);

            if (dispX != dispY)
                throw new Exception("Parameters are not diagonal to one another.");

            for (var i = 1 * signX; i != dispX; i += signX)
            {
                locs.Add(new Location(dispX + i, dispY + i));
            }

            return locs;
        }

        //returns IEnumerable<Final Location, List of Locations jumped
        public IEnumerable<Jump> GetAvailableJumps(Board board, CheckersPiece piece)
        {
            //check 4 corners
            var possibleEndValues = new List<Jump>
                                        {
                                            new Jump(new Location(piece.X - 1, piece.Y - 1)),
                                            new Jump(new Location(piece.X + 1, piece.Y - 1)),
                                            new Jump(new Location(piece.X - 1, piece.Y + 1)),
                                            new Jump(new Location(piece.X + 1, piece.Y + 1))
                                        };

            //remove all locations that are invalid
            possibleEndValues.RemoveAll(p => !board.IsValidLocation(p.FinalLocation.X, p.FinalLocation.Y));

            //remove all locations that do not land on a piece [we want to jump over that piece]
            possibleEndValues.RemoveAll(p => GetPieceAtPosition(p.FinalLocation.X, p.FinalLocation.Y) != null);

            //remove all locations that land on a piece of the same color
            possibleEndValues.RemoveAll(p => GetPieceAtPosition(p.FinalLocation.X, p.FinalLocation.Y).Color == piece.Color);

            //now all the locations in possibleEndValues are pieces that we can jump over - advance them all forward
            //check if the final location is now valid (within game board)
            possibleEndValues.RemoveAll(p =>
            {
                var dirX = p.FinalLocation.X - piece.X;
                var dirY = p.FinalLocation.Y - piece.Y;

                var newFinalLoc = p.FinalLocation;
                newFinalLoc.X += dirX;
                newFinalLoc.Y += dirY;

                if (board.IsValidLocation(newFinalLoc))
                {
                    p.LocationsJumpedOver.Add(p.FinalLocation);
                    p.FinalLocation = newFinalLoc;

                    return false;
                }

                return true;
            });
            //at this point possibleEndValues has the final location of the jump

            return possibleEndValues;
        }
        
        public IEnumerable<CheckersBoard> GetAvailableMoves(CheckersPiece piece)
        {
            var boards = new List<CheckersBoard>();
            var jumpsAvailable = GetAvailableJumps(TileBoard, piece).ToList();

            if (jumpsAvailable.Any())
            {
                foreach (var jump in jumpsAvailable)
                {
                    var newBoard = Clone();
                    var newBoardPiece = newBoard.GetPieceAtPosition(piece.X, piece.Y);

                    newBoardPiece.X = jump.FinalLocation.X;
                    newBoardPiece.Y = jump.FinalLocation.Y;

                    boards.Add(newBoard);
                }

                return boards; //user must make a jummp
            }

            var possibleEndValues = new List<Location>
                                        {
                                            new Location(piece.X - 1, piece.Y + (int)piece.Forward),
                                            new Location(piece.X + 1, piece.Y + (int)piece.Forward)
                                        };

            foreach (var possibleEndValue in possibleEndValues)
            {
                if (GetPieceAtPosition(possibleEndValue.X, possibleEndValue.Y) != null) continue;

                var newBoard = Clone();
                var newBoardPiece = newBoard.GetPieceAtPosition(piece.X, piece.Y);

                newBoardPiece.X = possibleEndValue.X;
                newBoardPiece.Y = possibleEndValue.Y;

                boards.Add(newBoard);
            }

            return boards;
        }

        public IEnumerable<CheckersBoard> GetAllAvailableMoves(PieceColor color)
        {
            var availables = new List<CheckersBoard>();

            foreach (var piece in InPlayPieces.Where(cp => cp.Color == color))
            {
                availables.AddRange(GetAvailableMoves(piece));
            }

            return availables;
        }

        public AvailableMove IsMovePossible(CheckersPiece piece, int nx, int ny)
        {
            var sx = Math.Abs(nx - piece.X);
            var sy = Math.Abs(ny - piece.Y);

            if (sx != sy) //pieces can only move diagonally
                throw new InvalidMoveException(piece, nx, ny);

            var totalDisplacement = sx + sy;

            if (totalDisplacement < 2) //if you are trying to move directly 1 space [up, down, left, right], no-go
                throw new InvalidMoveException(piece, nx, ny);

            if (totalDisplacement == 2) //if diagonal move 1 space
            {
                //forward movement can only be forward
                if (ny - piece.Y != (int)piece.Forward)
                    throw new InvalidMoveException(piece, nx, ny);

                if (TileBoard.IsValidLocation(nx, ny) && GetPieceAtPosition(nx, ny) == null)
                {
                    return AvailableMove.Forward;
                }
            }
            else if (totalDisplacement == 4) //a jump
            {
                var jump = IsJumpAvailable(piece, nx, ny);

                if (jump != null)
                    return AvailableMove.Jump;
                return AvailableMove.None;
            }

            return AvailableMove.None;
        }

        private Jump IsJumpAvailable(CheckersPiece piece, int nx, int ny)
        {
            return GetAvailableJumps(TileBoard, piece).FirstOrDefault(j => j.FinalLocation.X == nx && j.FinalLocation.Y == ny);
        }
    }
}