using System;
using System.Collections.Generic;
using System.Diagnostics;

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

            for (int i = potentialJumpedLocs.Count - 1; i >= 0; i--)
            {
                var j = potentialJumpedLocs[i];

                if (jumpedLoc != null)
                    if (j == jumpedLoc)
                    {
                        potentialJumpedLocs.RemoveAt(i);
                        continue;
                    }

                if(!TileBoard.IsValidLocation(j) || Pieces.GetPieceAtPosition(j) == null || Pieces.GetPieceAtPosition(j).Color == pieceColor)
                    potentialJumpedLocs.RemoveAt(i);
            }

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
                        GetAvailableJumps(newFinalLoc.X, newFinalLoc.Y, pieceColor, recentlyJumpedLoc);

                    if (jumpsAvailableAgain.Count > 0)
                    {
                        for (int index = 0; index < jumpsAvailableAgain.Count; index++)
                        {
                            var newJump = jumpsAvailableAgain[index];
                            var branchOfJump = jump.Clone();
                            
                            //skip any further jumps that jump back to where we started

                            bool anyJumpBackwards = false;
                            for (int j = 0; j < newJump.LocationsJumped.Count; j++)
                            {
                                var locJumped = newJump.LocationsJumped[j];
                                if (locJumped.JumpedLocation == new Location(pieceX, pieceY))
                                {
                                    anyJumpBackwards = true;
                                    break;
                                }
                            }

                            if (anyJumpBackwards)
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

        public List<MoveResult> GetAvailableMovesForPiece(CheckersPiece piece, bool jumpsOnly)
        {
            var moveResults = new List<MoveResult>();
            var jumpsAvailable = GetAvailableJumps(piece.X, piece.Y, piece.Color);

            if (jumpsAvailable.Count > 0 || jumpsOnly)
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

            for (int i = possibleEndValues.Count - 1; i >= 0; i--)
            {
                var p = possibleEndValues[i];

                if (!TileBoard.IsValidLocation(p) || Pieces.GetPieceAtPosition(p) != null)
                    possibleEndValues.RemoveAt(i);
            }
            
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

            bool anyJumps = false;
            for (int i = 0; i < listToCheck.Count; i++)
            {
                var piece = listToCheck[i];
                anyJumps = false;
                for (int j = 0; j < availables.Count; j++)
                {
                    var m = availables[j];
                    if (m.Type == MoveType.Jump)
                    {
                        anyJumps = true;
                        break;
                    }
                }

                availables.AddRange(GetAvailableMovesForPiece(piece, anyJumps));
            }

            anyJumps = false;
            for (int j = 0; j < availables.Count; j++)
            {
                var m = availables[j];
                if (m.Type == MoveType.Jump)
                {
                    anyJumps = true;
                    break;
                }
            }

            // if there are any jumps, you must take a jump
            if (anyJumps)
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
                    for (int j = 0; j < possibleMove.JumpResults.Count; j++)
                    {
                        var jumpResult = possibleMove.JumpResults[j];
                        if (jumpResult.FinalLocation.X == nx && jumpResult.FinalLocation.Y == ny)
                            return MoveType.Jump;
                    }
                }
            }

            throw new InvalidMoveException(piece, nx, ny);
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
                var loc = LocationsBetweenDiagonals(new Location(oldX, oldY), new Location(nx, ny))[0];

                Pieces.GetPieceAtPosition(loc).InPlay = false;

                if (GetAvailableJumps(piece.X, piece.Y, piece.Color).Count > 0)
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