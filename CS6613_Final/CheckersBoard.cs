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

        public CheckersPiece GetPieceAtPosition(Location loc)
        {
            return InPlayPieces.SingleOrDefault(c => c.X == loc.X && c.Y == loc.Y);
        }

        public IEnumerable<Location> LocationsBetweenDiagonals(Location one, Location two)
        {
            var locs = new List<Location>();

            var dispX = Math.Abs(two.X - one.X);
            var signX = Math.Sign(two.X - one.X);
            var dispY = Math.Abs(two.Y - one.Y);
            var signY = Math.Sign(two.Y - one.Y);

            if (dispX != dispY)
                throw new Exception("Parameters are not diagonal to one another.");

            for (var i = 1; i != dispX; i++)
            {
                locs.Add(new Location(one.X + signX*i, one.Y + signY*i));
            }

            return locs;
        }

        //returns IEnumerable<Final Location, List of Locations jumped
        public IEnumerable<Jump> GetAvailableJumps(CheckersPiece piece)
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
            possibleEndValues.RemoveAll(p => !TileBoard.IsValidLocation(p.FinalLocation.X, p.FinalLocation.Y));

            //remove all locations that do not land on a piece [we want to jump over that piece]
            possibleEndValues.RemoveAll(p => GetPieceAtPosition(p.FinalLocation.X, p.FinalLocation.Y) == null);

            //remove all locations that land on a piece of the same color
            possibleEndValues.RemoveAll(p => GetPieceAtPosition(p.FinalLocation.X, p.FinalLocation.Y).Color == piece.Color);

            //now all the locations in possibleEndValues are pieces that we can jump over - advance them all forward
            //check if the final location is now valid (within game board)
            for (var i = possibleEndValues.Count-1; i >= 0; i--)
            {
                var pos = possibleEndValues[i];

                var dirX = pos.FinalLocation.X - piece.X;
                var dirY = pos.FinalLocation.Y - piece.Y;

                var newFinalLoc = pos.FinalLocation;
                newFinalLoc.X += dirX;
                newFinalLoc.Y += dirY;

                if (TileBoard.IsValidLocation(newFinalLoc) && GetPieceAtPosition(newFinalLoc) == null)
                {
                    pos.FinalLocation = newFinalLoc;
                }
                else
                    possibleEndValues.RemoveAt(i);
            }
            //at this point possibleEndValues has the final location of the jump

            return possibleEndValues;
        }
        
        public IEnumerable<MoveResult> GetAvailableMoves(CheckersPiece piece)
        {
            var boards = new List<MoveResult>();
            var jumpsAvailable = GetAvailableJumps(piece).ToList();

            if (jumpsAvailable.Any())
            {
                foreach (var jump in jumpsAvailable)
                {
                    boards.Add(new MoveResult(MoveType.Jump, piece, jump.FinalLocation.X, jump.FinalLocation.Y));
                }

                return boards; //user must make a jummp
            }

            var possibleEndValues = new List<Location>
                                        {
                                            new Location(piece.X - 1, piece.Y + (int)piece.Forward),
                                            new Location(piece.X + 1, piece.Y + (int)piece.Forward)
                                        };

            //remove all locations that are invalid
            possibleEndValues.RemoveAll(p => !TileBoard.IsValidLocation(p));

            //remove all locations that land on a piece
            possibleEndValues.RemoveAll(p => GetPieceAtPosition(p) != null);

            foreach (var possibleEndValue in possibleEndValues)
            {
                boards.Add(new MoveResult(MoveType.Forward, piece, possibleEndValue.X, possibleEndValue.Y));
            }

            return boards;
        }

        public IEnumerable<MoveResult> GetAllAvailableMoves(PieceColor color)
        {
            var availables = new List<MoveResult>();

            foreach (var piece in InPlayPieces.Where(cp => cp.Color == color))
            {
                availables.AddRange(GetAvailableMoves(piece));
            }

            // if there are any jumps, you must take a jump
            if (availables.Any(m => m.TypeOfMove == MoveType.Jump))
                availables.RemoveAll(m => m.TypeOfMove != MoveType.Jump);

            return availables;
        }

        public MoveType IsMovePossible(CheckersPiece piece, int nx, int ny)
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
                    return MoveType.Forward;
                }
            }
            else if (totalDisplacement == 4) //a jump
            {
                var jump = IsJumpAvailable(piece, nx, ny);

                if (jump != null)
                    return MoveType.Jump;
                return MoveType.None;
            }

            return MoveType.None;
        }

        private Jump IsJumpAvailable(CheckersPiece piece, int nx, int ny)
        {
            return GetAvailableJumps(piece).FirstOrDefault(j => j.FinalLocation.X == nx && j.FinalLocation.Y == ny);
        }

        public GameResult GetGameResultState(PieceColor currentPlayer)
        {
            var result = GameResult.StillGoing;

            if (InPlayPieces.All(cp => cp.Color == PieceColor.Black))
                result = GameResult.BlackWins;
            else if (InPlayPieces.All(cp => cp.Color == PieceColor.Red))
                result = GameResult.RedWins;

            var blackCanMove = GetAllAvailableMoves(PieceColor.Black).Any();
            var redCanMove = GetAllAvailableMoves(PieceColor.Red).Any();

            if (blackCanMove && !redCanMove)
                result = GameResult.BlackWins;
            if (!blackCanMove && redCanMove)
                result = GameResult.RedWins;
            if (!blackCanMove && !redCanMove)
                result = (currentPlayer == PieceColor.Black ? GameResult.RedWins : GameResult.BlackWins);

            return result;
        }

        public bool IsGameOver(PieceColor currentPlayer)
        {
            var res = GetGameResultState(currentPlayer);

            return IsGameOver(res);
        }

        public bool IsGameOver(GameResult res)
        {
            return res == GameResult.RedWins || res == GameResult.BlackWins || res == GameResult.Tie;
        }

        public TurnResult MovePiece(MoveType type, CheckersPiece piece, int nx, int ny)
        {
            var oldX = piece.X;
            var oldY = piece.Y;
            piece.X = nx;
            piece.Y = ny;

            if (type == MoveType.Jump)
            {
                var loc = LocationsBetweenDiagonals(new Location(oldX, oldY), new Location(nx, ny)).Single();

                GetPieceAtPosition(loc).InPlay = false;

                if (GetAvailableJumps(piece).Any())
                {
                    return TurnResult.NotDone;
                }
            }

            return TurnResult.Finished;
        }
    }
}