using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CS6613_Final
{
    internal class CheckersBoard
    {
        private int _internalTurnBookKeeping = 0;

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

        public CheckersPiece GetCapturedPieceAtPositionForColor(Location loc, PieceColor col)
        {
            return CapturedPieces.Where(c => c.X == loc.X && c.Y == loc.Y && c.Color == col).OrderByDescending(cp => cp.Timestamp).FirstOrDefault();
        }

        public IEnumerable<CheckersPiece> GetPiecesForColor(PieceColor c)
        {
            return InPlayPieces.Where(cp => cp.Color == c);
        }

        public CheckersPiece GetPieceForColorAtLocation(Location loc, PieceColor col)
        {
            return InPlayPieces.SingleOrDefault(c => c.X == loc.X && c.Y == loc.Y && c.Color == col);
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
        public IEnumerable<Jump> GetAvailableJumps(int pieceX, int pieceY, PieceColor pieceColor, Location? jumpedLoc = null)
        {
            var possibleEndValues = new List<Jump>();
            //check 4 corners
            var potentialJumpedLocs = new List<Location>
                                        {
                                            new Location(pieceX - 1, pieceY - 1),
                                            new Location(pieceX + 1, pieceY - 1),
                                            new Location(pieceX - 1, pieceY + 1),
                                            new Location(pieceX + 1, pieceY + 1)
                                        };

            //remove locations that were jumped
            if(jumpedLoc != null)
                potentialJumpedLocs.RemoveAll(p => p == jumpedLoc);

            //remove all locations that are invalid
            potentialJumpedLocs.RemoveAll(p => !TileBoard.IsValidLocation(p));

            //remove all locations that do not land on a piece [we want to jump over that piece]
            potentialJumpedLocs.RemoveAll(p => GetPieceAtPosition(p) == null);

            //remove all locations that land on a piece of the same color
            potentialJumpedLocs.RemoveAll(p => GetPieceAtPosition(p).Color == pieceColor);

            //now all the locations in possibleEndValues are pieces that we can jump over - advance them all forward
            //check if the final location is now valid (within game board)
            for (var i = potentialJumpedLocs.Count-1; i >= 0; i--)
            {
                var jump = new Jump();
                var pos = potentialJumpedLocs[i];
                var recentlyJumpedLoc = new Location();

                var dirX = pos.X - pieceX;
                var dirY = pos.Y - pieceY;

                var newFinalLoc = recentlyJumpedLoc = pos;
                newFinalLoc.X += dirX;
                newFinalLoc.Y += dirY;

                if (TileBoard.IsValidLocation(newFinalLoc) && GetPieceAtPosition(newFinalLoc) == null)
                {
                    jump.AddJumpedLocation(new JumpResult(recentlyJumpedLoc, newFinalLoc));

                    var jumpsAvailableAgain =
                        GetAvailableJumps(newFinalLoc.X, newFinalLoc.Y, pieceColor, recentlyJumpedLoc).
                            ToList();

                    if (jumpsAvailableAgain.Any())
                    {
                        foreach (var newJump in jumpsAvailableAgain)
                        {
                            var branchOfJump = jump.Clone();
                            //skip any further jumps that jump back to where we started
                            if (newJump.LocationsJumped.Any(jres => jres.JumpedLocation == new Location(pieceX, pieceY)))
                                break;

                            branchOfJump.LocationsJumped.AddRange(newJump.LocationsJumped);

                            possibleEndValues.Add(branchOfJump);
                        }
                    }
                    else
                    {
                        possibleEndValues.Add(jump);
                    }
                }
            }
            //at this point possibleEndValues has the final location of the jump

            return possibleEndValues;
        }

        public IEnumerable<MoveResult> GetAvailableMoves(CheckersPiece piece, bool jumpsOnly)
        {
            var moveResults = new List<MoveResult>();
            var jumpsAvailable = GetAvailableJumps(piece.X, piece.Y, piece.Color).ToList();

            if (jumpsAvailable.Any() || jumpsOnly)
            {
                foreach (var jump in jumpsAvailable)
                {
                    var mr =new MoveResult(MoveType.Jump, piece, jump.FinalLocation.X, jump.FinalLocation.Y)
                                {JumpResults = jump.LocationsJumped};
                    moveResults.Add(mr);
                }

                return moveResults; //user must make a jummp
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
                moveResults.Add(new MoveResult(MoveType.Forward, piece, possibleEndValue.X, possibleEndValue.Y));
            }

            return moveResults;
        }

        public IEnumerable<MoveResult> GetAllAvailableMoves(PieceColor color)
        {
            var availables = new List<MoveResult>();

            foreach (var piece in InPlayPieces.Where(cp => cp.Color == color))
            {
                availables.AddRange(GetAvailableMoves(piece, availables.Any(mr => mr.Type == MoveType.Jump)));
            }

            // if there are any jumps, you must take a jump
            if (availables.Any(m => m.Type == MoveType.Jump))
                availables.RemoveAll(m => m.Type != MoveType.Jump);

            return availables;
        }

        public MoveType IsMovePossible(CheckersPiece piece, int nx, int ny, IEnumerable<MoveResult> possibleMoves)
        {
            var sx = Math.Abs(nx - piece.X);
            var sy = Math.Abs(ny - piece.Y);

            var isMovePossible = true;
            foreach(var possibleMove in possibleMoves)
            {
                if (possibleMove.Type == MoveType.Forward)
                {
                    if (possibleMove.FinalPieceLocation.X == nx && possibleMove.FinalPieceLocation.Y == ny)
                        return MoveType.Forward;
                }
                else if(possibleMove.Type == MoveType.Jump)
                {
                    if (possibleMove.JumpResults.Any(jr => jr.FinalLocation.X == nx && jr.FinalLocation.Y == ny))
                        return MoveType.Jump;
                }
            }
        
            throw new InvalidMoveException(piece, nx, ny);
        }

        private Jump IsJumpAvailable(CheckersPiece piece, int nx, int ny)
        {
            return GetAvailableJumps(piece.X, piece.Y, piece.Color).FirstOrDefault(j => j.FinalLocation.X == nx && j.FinalLocation.Y == ny);
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
            return res == GameResult.RedWins || res == GameResult.BlackWins;
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

                if (GetAvailableJumps(piece.X, piece.Y, piece.Color).Any())
                {
                    return TurnResult.NotDone;
                }
            }

            return TurnResult.Finished;
        }

        public TurnResult MovePiece(MoveResult move, PieceColor color)
        {
            var piece =
                GetPiecesForColor(color).SingleOrDefault(
                    p => p.X == move.OriginalPieceLocation.X && p.Y == move.OriginalPieceLocation.Y);

            Debug.Assert(piece != null);

            piece.X = move.FinalPieceLocation.X;
            piece.Y = move.FinalPieceLocation.Y;

            if (move.Type == MoveType.Jump)
            {
                foreach (var jumpResult in move.JumpResults)
                {
                    var killedPiece = GetPieceForColorAtLocation(jumpResult.JumpedLocation, color == PieceColor.Black ? PieceColor.Red : PieceColor.Black);

                    killedPiece.InPlay = false;
                    killedPiece.Timestamp = _internalTurnBookKeeping;
                }
            }

            _internalTurnBookKeeping++;
            return TurnResult.Finished;
        }

        public void RevertMove(MoveResult move, PieceColor color)
        {
            if(move.Type == MoveType.Jump)
            {
                foreach(var jumpResult in move.JumpResults)
                {
                    var resPiece = GetCapturedPieceAtPositionForColor(jumpResult.JumpedLocation,
                                                                      color == PieceColor.Black
                                                                          ? PieceColor.Red
                                                                          : PieceColor.Black);
                    resPiece.InPlay = true;
                    resPiece.Timestamp = 0;
                }
            }

            var piece = GetPieceAtPosition(move.FinalPieceLocation);
            piece.X = move.OriginalPieceLocation.X;
            piece.Y = move.OriginalPieceLocation.Y;

            _internalTurnBookKeeping--;
        }

        public bool ArePiecesInSamePosition(CheckersBoard board)
        {
            return InPlayPieces.Count() == board.InPlayPieces.Count() &&
                   CapturedPieces.Count() == board.CapturedPieces.Count() &&
                   InPlayPieces.All(cp => board.InPlayPieces.SingleOrDefault(cp2 => cp2.Equals(cp)) != null) &&
                   CapturedPieces.All(cp => board.CapturedPieces.SingleOrDefault(cp2 => cp2.Equals(cp)) != null);
        }
    }
}