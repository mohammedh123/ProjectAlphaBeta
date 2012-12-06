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

        public SetOfPieces Pieces { get; set; }
        
        public CheckersBoard Clone()
        {
            var clone = new CheckersBoard
                            {
                                TileBoard = TileBoard,
                                Pieces = new SetOfPieces(Pieces)
                            };
            
            return clone;
        }

        public List<Location> LocationsBetweenDiagonals(Location one, Location two)
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

        //returns List<Final Location, List of Locations jumped
        public List<Jump> GetAvailableJumps(int pieceX, int pieceY, PieceColor pieceColor, Location? jumpedLoc = null)
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
            potentialJumpedLocs.RemoveAll(p => Pieces.GetPieceAtPosition(p) == null);

            //remove all locations that land on a piece of the same color
            potentialJumpedLocs.RemoveAll(p => Pieces.GetPieceAtPosition(p).Color == pieceColor);

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

                if (TileBoard.IsValidLocation(newFinalLoc) && Pieces.GetPieceAtPosition(newFinalLoc) == null)
                {
                    jump.AddJumpedLocation(new JumpResult(recentlyJumpedLoc, newFinalLoc));

                    var jumpsAvailableAgain =
                        GetAvailableJumps(newFinalLoc.X, newFinalLoc.Y, pieceColor, recentlyJumpedLoc).
                            ToList();

                    if (jumpsAvailableAgain.Any())
                    {
                        for (int index = 0; index < jumpsAvailableAgain.Count; index++)
                        {
                            var newJump = jumpsAvailableAgain[index];
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

        public List<MoveResult> GetAvailableMoves(CheckersPiece piece, bool jumpsOnly)
        {
            var moveResults = new List<MoveResult>();
            var jumpsAvailable = GetAvailableJumps(piece.X, piece.Y, piece.Color).ToList();

            if (jumpsAvailable.Any() || jumpsOnly)
            {
                for (int i = 0; i < jumpsAvailable.Count; i++)
                {
                    var jump = jumpsAvailable[i];
                    var mr = new MoveResult(MoveType.Jump, piece, jump.FinalLocation.X, jump.FinalLocation.Y)
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
            possibleEndValues.RemoveAll(p => Pieces.GetPieceAtPosition(p) != null);

            for (int i = 0; i < possibleEndValues.Count; i++)
            {
                var possibleEndValue = possibleEndValues[i];
                moveResults.Add(new MoveResult(MoveType.Forward, piece, possibleEndValue.X, possibleEndValue.Y));
            }

            return moveResults;
        }

        public List<MoveResult> GetAllAvailableMoves(PieceColor color)
        {
            var availables = new List<MoveResult>();
            var listToCheck = color == PieceColor.Black ? Pieces.AlivePlayerOnePieces : Pieces.AlivePlayerTwoPieces;

            for (int i = 0; i < listToCheck.Count; i++)
            {
                var piece = listToCheck[i];
                availables.AddRange(GetAvailableMoves(piece, availables.Any(mr => mr.Type == MoveType.Jump)));
            }

            // if there are any jumps, you must take a jump
            if (availables.Any(m => m.Type == MoveType.Jump))
                availables.RemoveAll(m => m.Type != MoveType.Jump);

            return availables;
        }

        public MoveType IsMovePossible(CheckersPiece piece, int nx, int ny, List<MoveResult> possibleMoves)
        {
            var sx = Math.Abs(nx - piece.X);
            var sy = Math.Abs(ny - piece.Y);

            var isMovePossible = true;
            for (int i = 0; i < possibleMoves.Count; i++)
            {
                var possibleMove = possibleMoves[i];
                if (possibleMove.Type == MoveType.Forward)
                {
                    if (possibleMove.FinalPieceLocation.X == nx && possibleMove.FinalPieceLocation.Y == ny)
                        return MoveType.Forward;
                }
                else if (possibleMove.Type == MoveType.Jump)
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

            bool allAreBlack = true, allAreRed = true;

            allAreBlack = Pieces.AlivePlayerOnePieces.Count > 0 && Pieces.AlivePlayerTwoPieces.Count == 0;
            allAreRed   = Pieces.AlivePlayerTwoPieces.Count > 0 && Pieces.AlivePlayerOnePieces.Count == 0;

            if (allAreBlack)
                result = GameResult.BlackWins;
            else if (allAreRed)
                result = GameResult.RedWins;

            var blackCanMove = GetAllAvailableMoves(PieceColor.Black).Count > 0;
            var redCanMove = GetAllAvailableMoves(PieceColor.Red).Count > 0;

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

                Pieces.GetPieceAtPosition(loc).InPlay = false;

                if (GetAvailableJumps(piece.X, piece.Y, piece.Color).Any())
                {
                    return TurnResult.NotDone;
                }
            }

            return TurnResult.Finished;
        }

        public TurnResult MovePiece(MoveResult move, PieceColor color)
        {
            var piece = Pieces.GetPieceForColorAtLocation(move.OriginalPieceLocation, color);

            Debug.Assert(piece != null);

            piece.X = move.FinalPieceLocation.X;
            piece.Y = move.FinalPieceLocation.Y;

            if (move.Type == MoveType.Jump)
            {
                for (int i = 0; i < move.JumpResults.Count; i++)
                {
                    var jumpResult = move.JumpResults[i];
                    var killedPiece = Pieces.GetPieceForColorAtLocation(jumpResult.JumpedLocation,
                                                                 color == PieceColor.Black
                                                                     ? PieceColor.Red
                                                                     : PieceColor.Black);

                    Pieces.KillPiece(killedPiece, _internalTurnBookKeeping);
                }
            }

            _internalTurnBookKeeping++;
            return TurnResult.Finished;
        }

        public void RevertMove(MoveResult move, PieceColor color)
        {
            if(move.Type == MoveType.Jump)
            {
                for (int i = 0; i < move.JumpResults.Count; i++)
                {
                    var jumpResult = move.JumpResults[i];
                    var resPiece = Pieces.GetCapturedPieceForColorAtLocation(jumpResult.JumpedLocation,
                                                                      color == PieceColor.Black
                                                                          ? PieceColor.Red
                                                                          : PieceColor.Black);

                    Pieces.RevivePiece(resPiece);
                }
            }

            var piece = Pieces.GetPieceAtPosition(move.FinalPieceLocation);
            piece.X = move.OriginalPieceLocation.X;
            piece.Y = move.OriginalPieceLocation.Y;

            _internalTurnBookKeeping--;
        }
    }
}