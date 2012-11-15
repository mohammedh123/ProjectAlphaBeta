using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    public enum PieceColor
    {
        BLACK,
        RED
    }

    public enum PieceDirection
    {
        UP = -1,
        DOWN = 1
    }

    public enum AvailableMove
    {
        NONE,
        JUMP,
        FORWARD
    }

    class CheckersBoard
    {
        public Board Board { get; set; }
        public List<CheckersPiece> PlayerOnePieces { get; set; }
        public List<CheckersPiece> PlayerTwoPieces { get; set; }

        public IEnumerable<CheckersPiece> CapturedPieces
        {
            get
            {
                return PlayerOnePieces.Where(p => !p.InPlay).Union(PlayerTwoPieces.Where(p => !p.InPlay));
            }
        }

        public IEnumerable<CheckersPiece> InPlayPieces
        {
            get
            {
                return PlayerOnePieces.Where(p => p.InPlay).Union(PlayerTwoPieces.Where(p => p.InPlay));
            }
        }
    }

    class CheckersBoardGame
    {
        public CheckersBoard Board { get; set; }

        public ILogicDriver CurrentPlayer
        {
            get
            {
                if (isBlackTurn)
                    return playerOne;
                else
                    return playerTwo;
            }
        }
        
        public CheckersPiece SelectedPiece
        {
            get
            {
                CheckersPiece selectedPiece = null;

                if(playerOne.IsPlayer || playerTwo.IsPlayer)
                {
                    if (playerOne.IsPlayer) 
                        selectedPiece = (playerOne as PlayerLogicDriver).CurrentSelectedPiece;
                    if(selectedPiece == null && playerTwo.IsPlayer)
                        selectedPiece = (playerTwo as PlayerLogicDriver).CurrentSelectedPiece;

                    return selectedPiece;
                }

                return null;
            }
        }

        bool isBlackTurn = true;
        ILogicDriver playerOne, playerTwo;
        IDisplayDriver displayer;

        public void Start(int numPieces, ILogicDriver pOne, ILogicDriver pTwo, IDisplayDriver displayer, bool playerWantsToGoFirst)
        {
            var r = new Random();
            Board = new CheckersBoard
                        {
                            Board = new Board(6, 6),
                            PlayerOnePieces = new List<CheckersPiece>(),
                            PlayerTwoPieces = new List<CheckersPiece>()
                        };

            var pOneColor = r.Next(0, 2); // 0 = black, 1 = red

            if (playerWantsToGoFirst)
            {
                if (pOne.IsPlayer)
                {
                    pOneColor = 0;
                }
                else if (pTwo.IsPlayer)
                {
                    pOneColor = 1;
                }
            }
            
            playerOne = pOne;
            playerOne.Color = pOneColor == 0 ? PieceColor.BLACK : PieceColor.RED;
            PlayerOnePieces = new List<CheckersPiece>();

            for (int j = 0; j < CurrentBoard.Height; j++)
            {
                for (int i = 0; i < CurrentBoard.Width; i++)
                {
                    if (CurrentBoard.GetTile(i, j).Color == TileColor.BLACK)
                        PlayerOnePieces.Add(new CheckersPiece(i, j, playerOne.Color, PieceDirection.DOWN));

                    if (PlayerOnePieces.Count == numPieces)
                        break;
                }

                if (PlayerOnePieces.Count == numPieces)
                    break;
            }

            playerTwo = pTwo;
            playerTwo.Color = playerOne.Color == PieceColor.RED ? PieceColor.BLACK : PieceColor.RED;
            PlayerTwoPieces = new List<CheckersPiece>();

            for (int j = CurrentBoard.Height - 1; j >= 0; j--)
            {
                for (int i = 0; i < CurrentBoard.Width; i++)
                {
                    if (CurrentBoard.GetTile(i, j).Color == TileColor.BLACK)
                        PlayerTwoPieces.Add(new CheckersPiece(i, j, playerTwo.Color, PieceDirection.UP));

                    if (PlayerTwoPieces.Count == numPieces)
                        break;
                }

                if (PlayerTwoPieces.Count == numPieces)
                    break;
            }


            this.displayer = displayer;
        }

        public CheckersPiece GetPieceAtPosition(int x, int y)
        {
            return InPlayPieces.SingleOrDefault(c => c.X == x && c.Y == y);
        }

        public IEnumerable<Location> LocationsBetweenDiagonals(Location one, Location two)
        {
            List<Location> locs = new List<Location>();

            int dispX = Math.Abs(two.X - one.X);
            int signX = Math.Sign(dispX);
            int dispY = Math.Abs(two.Y - one.Y);
            int signY = Math.Sign(dispY);
            
            if (dispX != dispY)
                throw new Exception("Parameters are not diagonal to one another.");

            for (int i = 1 * signX; i != dispX; i += signX)
            {
                locs.Add(new Location(dispX + i, dispY + i));
            }

            return locs;
        }

        //returns IEnumerable<Final Location, List of Locations jumped
        public IEnumerable<Jump> JumpsAvailable(Board board, int x, int y, PieceColor color, PieceDirection forward, int oldX, int oldY, 
            List<Location> jumpedLocations = null)
        {
            jumpedLocations = jumpedLocations ?? new List<Location>();

            //check 4 corners
            var possibleEndValues = new List<Jump> 
            {
                new Jump(new Location(x-1, y-1)),
                new Jump(new Location(x+1, y-1)),
                new Jump(new Location(x-1, y+1)),
                new Jump(new Location(x+1, y+1))
            };

            //remove locations that we have already jumped
            possibleEndValues.RemoveAll(p => jumpedLocations.Contains(p.FinalLocation));

            //remove the location that we jumped from
            possibleEndValues.RemoveAll(p => p.FinalLocation.X == oldX && p.FinalLocation.Y == oldY);

            //remove all locations that are invalid
            possibleEndValues.RemoveAll(p => !board.IsValidLocation(p.FinalLocation.X, p.FinalLocation.Y));

            //remove all locations that do not land on a piece [we want to jump over that piece]
            possibleEndValues.RemoveAll(p => GetPieceAtPosition(p.FinalLocation.X, p.FinalLocation.Y) != null);

            //remove all locations that land on a piece of the same color
            possibleEndValues.RemoveAll(p => GetPieceAtPosition(p.FinalLocation.X, p.FinalLocation.Y).Color == color);
            
            //now all the locations in possibleEndValues are pieces that we can jump over - advance them all forward
            //check if the final location is now valid (within game board)
            possibleEndValues.RemoveAll(p => {
                int dirX = p.FinalLocation.X - x;
                int dirY = p.FinalLocation.Y - y;

                var newFinalLoc = p.FinalLocation;
                newFinalLoc.X += dirX;
                newFinalLoc.Y += dirY;

                if (board.IsValidLocation(newFinalLoc))
                {
                    jumpedLocations.Add(p.FinalLocation);
                    p.LocationsJumpedOver.Add(p.FinalLocation);
                    p.FinalLocation = newFinalLoc;

                    return false;
                }
                
                return true;
            });
            //at this point possibleEndValues has the final location of the jump

            var i = 0;
            while(i != possibleEndValues.Count)
            {
                var loc = possibleEndValues[i];

                //we finally have all valid pieces that we can jump over - see if we can jump over them even further

                var jumpsAvailable = JumpsAvailable(board, loc.FinalLocation.X, loc.FinalLocation.Y, color, forward, x, y, jumpedLocations);
                if (jumpsAvailable.Any())
                {
                    //if you can jump further, add them on to possibleEndValues and remove the old jump (the new ones encompass it)
                    possibleEndValues.AddRange(jumpsAvailable);

                    possibleEndValues.RemoveAt(i);
                }
                else
                    i++;
            }

            return possibleEndValues;
        }

        public Jump IsJumpAvailable(Board board, int x, int y, int destX, int destY, PieceColor color, PieceDirection forward, int oldX, int oldY,
            List<Location> jumpedLocations = null)
        {
            jumpedLocations = jumpedLocations ?? new List<Location>();

            int sx = Math.Abs(destX - x);
            int sy = Math.Abs(destY - y);

            if (sx != sy)
                return null;
            if (sx + sy != 2)
                return null;
            
            //check 1 corners
            var possibleEndValues = new List<Jump> 
            {
                new Jump(new Location(destX, destY))
            };

            //remove locations that we have already jumped
            possibleEndValues.RemoveAll(p => jumpedLocations.Contains(p.FinalLocation));

            //remove the location that we jumped from
            possibleEndValues.RemoveAll(p => p.FinalLocation.X == oldX && p.FinalLocation.Y == oldY);

            //remove all locations that are invalid
            possibleEndValues.RemoveAll(p => !board.IsValidLocation(p.FinalLocation.X, p.FinalLocation.Y));

            //remove all locations that do not land on a piece [we want to jump over that piece]
            possibleEndValues.RemoveAll(p => GetPieceAtPosition(p.FinalLocation.X, p.FinalLocation.Y) != null);

            //remove all locations that land on a piece of the same color
            possibleEndValues.RemoveAll(p => GetPieceAtPosition(p.FinalLocation.X, p.FinalLocation.Y).Color == color);

            //now all the locations in possibleEndValues are pieces that we can jump over - advance them all forward
            //check if the final location is now valid (within game board)
            possibleEndValues.RemoveAll(p =>
            {
                int dirX = p.FinalLocation.X - x;
                int dirY = p.FinalLocation.Y - y;

                var newFinalLoc = p.FinalLocation;
                newFinalLoc.X += dirX;
                newFinalLoc.Y += dirY;

                if (board.IsValidLocation(newFinalLoc))
                {
                    jumpedLocations.Add(p.FinalLocation);
                    p.LocationsJumpedOver.Add(p.FinalLocation);
                    p.FinalLocation = newFinalLoc;

                    return false;
                }

                return true;
            });
            //at this point possibleEndValues has the final location of the jump

            var i = 0;
            while (i != possibleEndValues.Count)
            {
                var loc = possibleEndValues[i];

                //we finally have all valid pieces that we can jump over - see if we can jump over them even further

                var jumpsAvailable = JumpsAvailable(board, loc.FinalLocation.X, loc.FinalLocation.Y, color, forward, x, y, jumpedLocations);
                if (jumpsAvailable.Any())
                {
                    //if you can jump further, add them on to possibleEndValues and remove the old jump (the new ones encompass it)
                    possibleEndValues.AddRange(jumpsAvailable);

                    possibleEndValues.RemoveAt(i);
                }
                else
                    i++;
            }

            return possibleEndValues.SingleOrDefault();
        }

        public IEnumerable<Board> GetAvailableMoves(Board board, int x, int y, PieceDirection forward)
        {
            var boards = new List<Board>();
            
            return boards;
        }

        public IEnumerable<Board> GetAllAvailableMoves(Board board, PieceColor color)
        {
            var availables = new List<Board>();

            foreach(var piece in InPlayPieces.Where(cp => cp.Color == color))
            {
                availables.AddRange(GetAvailableMoves(board, piece.X, piece.Y, piece.Forward));
            }

            return availables;
        }

        public AvailableMove IsMovePossible(Board board, int x, int y, int nx, int ny, PieceColor color, PieceDirection forward)
        {
            var piece = GetPieceAtPosition(x, y);
            int sx = Math.Abs(nx - x);
            int sy = Math.Abs(ny - y);

            if (sx != sy) //pieces can only move diagonally
                throw new InvalidMoveException(piece, nx, ny);

            int totalDisplacement = sx + sy;

            if (totalDisplacement < 2) //if you are trying to move directly 1 space [up, down, left, right], no-go
                throw new InvalidMoveException(piece, nx, ny);

            if (totalDisplacement == 2) //if diagonal move 1 space
            {
                //forward movement can only be forward
                if (ny - y != (int)forward)
                    throw new InvalidMoveException(piece, nx, ny);

                if (board.IsValidLocation(nx, ny) && GetPieceAtPosition(nx, ny) == null)
                {
                    return AvailableMove.FORWARD;
                }
            }
            else if(totalDisplacement == 4) //a jump
            {
                var jump = IsJumpAvailable(board, x, y, nx, ny, color, forward, x, y);

                if (jump != null)
                    return AvailableMove.JUMP;
                else
                    return AvailableMove.NONE;
            }

            return AvailableMove.NONE;
        }

        public TurnResult MovePiece(CheckersPiece piece, int nx, int ny)
        {
            piece.X = nx;
            piece.Y = ny;

            return TurnResult.Finished;
        }

        public void Draw()
        {
            displayer.Draw(CurrentBoard, InPlayPieces, SelectedPiece);
        }

        public bool IsGameOver()
        {
            return InPlayPieces.All(cp => cp.Color == PieceColor.BLACK) ||
                   InPlayPieces.All(cp => cp.Color == PieceColor.RED);

        }

        public void AttemptTurn()
        {
            var result = CurrentPlayer.GetNextMove(this);

            if(result == TurnResult.Finished)
                SwitchPlayer();
        }

        public void SwitchPlayer()
        {
            isBlackTurn = !isBlackTurn;
        }
    }
}
