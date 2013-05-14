// Mohammed Hossain 12/12/12

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CS6613_Final
{
    // CheckersBoard: a class that represents a board of the game of Checkers: a board, and its pieces. Has helper methods to assist in playing the game of Checkers
    internal class CheckersBoard
    {
        //an internal timestamp must be kept to make sure that we 'revive' the right piece in MovePiece/RevertMove
        private int _internalTurnBookKeeping;

        public Board TileBoard { get; set; }
        public SetOfPieces Pieces { get; set; }
        
        // a helper method to deep clone a CheckersBoard
        public CheckersBoard Clone()
        {
            var clone = new CheckersBoard
                            {
                                TileBoard = TileBoard,
                                Pieces = new SetOfPieces(Pieces)
                            };
            
            return clone;
        }

        // returns a list of locations between two tiles that are diagonal to each other, throws if they aren't diagonal
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

        //returns a list of all availables jumps from a target location, given a piece color, and makes sure it doesn't jump over a jumped location
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

            //remove locations that are invalid
            for (var i = potentialJumpedLocs.Count - 1; i >= 0; i--)
            {
                var j = potentialJumpedLocs[i];

                if (jumpedLoc != null)
                    if (j == jumpedLoc)
                    {
                        potentialJumpedLocs.RemoveAt(i);
                        continue;
                    }

                //if the jumped location is invalid, if the jump would not jump over a piece, or if the jump would jump over a friendly piece, remove it
                if(!TileBoard.IsValidLocation(j) || Pieces.GetPieceAtPosition(j) == null || Pieces.GetPieceAtPosition(j).Color == pieceColor)
                    potentialJumpedLocs.RemoveAt(i);
            }

            //now all the locations in possibleEndValues are pieces that we can jump over - advance them all forward
            //check if the final location is now valid (within game board)
            for (var i = potentialJumpedLocs.Count-1; i >= 0; i--)
            {
                var jump = new Jump();
                var pos = potentialJumpedLocs[i];
                Location recentlyJumpedLoc;

                var dirX = pos.X - pieceX;
                var dirY = pos.Y - pieceY;

                var newFinalLoc = recentlyJumpedLoc = pos;
                newFinalLoc.X += dirX;
                newFinalLoc.Y += dirY;

                if (TileBoard.IsValidLocation(newFinalLoc) && Pieces.GetPieceAtPosition(newFinalLoc) == null)
                {
                    jump.AddJumpedLocation(new JumpResult(recentlyJumpedLoc, newFinalLoc));

                    //see if we can make even more jumps
                    var jumpsAvailableAgain =
                        GetAvailableJumps(newFinalLoc.X, newFinalLoc.Y, pieceColor, recentlyJumpedLoc);

                    if (jumpsAvailableAgain.Count > 0)
                    {
                        //for each jump, make a new permutation of that jump and added it to the list
                        for (var index = 0; index < jumpsAvailableAgain.Count; index++)
                        {
                            var newJump = jumpsAvailableAgain[index];
                            var branchOfJump = jump.Clone();
                            
                            //skip any further jumps that jump back to where we started

                            var anyJumpBackwards = false;
                            for (var j = 0; j < newJump.LocationsJumped.Count; j++)
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

            return possibleEndValues;
        }

        // returns the possible moves for a given piece, if jumpsOnly is set, then only jumps will be considered
        public List<MoveResult> GetAvailableMovesForPiece(CheckersPiece piece, bool jumpsOnly)
        {
            var moveResults = new List<MoveResult>();
            var jumpsAvailable = GetAvailableJumps(piece.X, piece.Y, piece.Color);

            if (jumpsAvailable.Count > 0 || jumpsOnly)
            {
                for (var i = 0; i < jumpsAvailable.Count; i++)
                {
                    var jump = jumpsAvailable[i];
                    var mr = new MoveResult(MoveType.Jump, piece, jump.FinalLocation.X, jump.FinalLocation.Y)
                                 {JumpResults = jump.LocationsJumped};
                    moveResults.Add(mr);
                }

                return moveResults; //user must make a jummp
            }

            //possible forward moves must be considered too
            var possibleEndValues = new List<Location>
                                        {
                                            new Location(piece.X - 1, piece.Y + (int)piece.Forward),
                                            new Location(piece.X + 1, piece.Y + (int)piece.Forward)
                                        };

            for (var i = possibleEndValues.Count - 1; i >= 0; i--)
            {
                var p = possibleEndValues[i];

                //remove invalid final locations (piece is there or the location is invalid)
                if (!TileBoard.IsValidLocation(p) || Pieces.GetPieceAtPosition(p) != null)
                    possibleEndValues.RemoveAt(i);
            }
            
            for (var i = 0; i < possibleEndValues.Count; i++)
            {
                var possibleEndValue = possibleEndValues[i];
                moveResults.Add(new MoveResult(MoveType.Forward, piece, possibleEndValue.X, possibleEndValue.Y));
            }

            return moveResults;
        }

        // returns all the available moves for a given color
        public List<MoveResult> GetAllAvailableMoves(PieceColor color)
        {
            var availables = new List<MoveResult>();
            var listToCheck = color == PieceColor.Black ? Pieces.AlivePlayerOnePieces : Pieces.AlivePlayerTwoPieces;

            bool anyJumps;
            //for every living piece of said color
            for (var i = 0; i < listToCheck.Count; i++)
            {
                var piece = listToCheck[i];
                anyJumps = false;
                for (var j = 0; j < availables.Count; j++)
                {
                    //if any current available moves are a jump, then all future moves are invalid unless they are a jump - force only jump moves
                    var m = availables[j];
                    if (m.Type == MoveType.Jump) //if the
                    {
                        anyJumps = true;
                        break;
                    }
                }

                //append the available moves onto the list
                availables.AddRange(GetAvailableMovesForPiece(piece, anyJumps));
            }

            // if there are any jumps, you must take a jump
            if(availables.Any(m => m.Type == MoveType.Jump))
               availables.RemoveAll(m => m.Type != MoveType.Jump);

            return availables;
        }
        
        // returns the state of the game given a player color
        public GameResult GetGameResultState(PieceColor currentPlayer)
        {
            var result = GameResult.StillGoing;

            var allAreBlack = Pieces.AlivePlayerOnePieces.Count > 0 && Pieces.AlivePlayerTwoPieces.Count == 0;
            var allAreRed = Pieces.AlivePlayerTwoPieces.Count > 0 && Pieces.AlivePlayerOnePieces.Count == 0;

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

        // returns if the game is over for a given player
        public bool IsGameOver(PieceColor currentPlayer)
        {
            var res = GetGameResultState(currentPlayer);

            return IsGameOver(res);
        }

        public bool IsGameOver(GameResult res)
        {
            return res == GameResult.RedWins || res == GameResult.BlackWins;
        }
        
        // moves a piece given a MoveResult and color
        public TurnResult MovePiece(MoveResult move, PieceColor color)
        {
            var piece = Pieces.GetPieceForColorAtLocation(move.OriginalPieceLocation, color);

            Debug.Assert(piece != null);

            piece.X = move.FinalPieceLocation.X;
            piece.Y = move.FinalPieceLocation.Y;

            if (move.Type == MoveType.Jump)
            {
                for (var i = 0; i < move.JumpResults.Count; i++)
                {
                    //kills any jumped pieces
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

        // reverts a given move and returns to the state that it was before that move
        public void RevertMove(MoveResult move, PieceColor color)
        {
            if(move.Type == MoveType.Jump)
            {
                for (var i = 0; i < move.JumpResults.Count; i++)
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