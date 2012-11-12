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

    class CheckersBoardGame
    {
        public Board CurrentBoard { get; private set; }
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

        bool isBlackTurn = true;
        ILogicDriver playerOne, playerTwo;
        IDisplayDriver displayer;

        public void Start(int numPieces, ILogicDriver pOne, ILogicDriver pTwo, IDisplayDriver displayer, bool playerWantsToGoFirst)
        {
            Random r = new Random();
            CurrentBoard = new Board(6, 6);
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

        public IEnumerable<Location> IsJumpAvailable(Board board, int x, int y, PieceColor color, PieceDirection forward, int oldX, int oldY)
        {
            //check 4 corners
            var possibleEndValues = new List<Location> 
            {
                new Location(x-1, y-1),
                new Location(x+1, y-1),
                new Location(x-1, y+1),
                new Location(x+1, y+1)
            };

            //remove the location that we jumped from
            possibleEndValues.RemoveAll(p => p.X == oldX && p.Y == oldY);

            //remove all locations that are invalid
            possibleEndValues.RemoveAll(p => !board.IsValidLocation(p.X, p.Y));

            //remove all locations that do not land on a piece [we want to jump over that piece]
            possibleEndValues.RemoveAll(p => GetPieceAtPosition(p.X, p.Y) != null);

            //remove all locations that land on a piece of the same color
            possibleEndValues.RemoveAll(p => GetPieceAtPosition(p.X, p.Y).Color == color);

            //we finally have all valid pieces that we can jump over - see if we can jump over them even further
            foreach (var loc in possibleEndValues)
            {
                IsJumpAvailable(board, loc.X, loc.Y, color, forward, x, y);
            }

            return possibleEndValues;
        }

        public IEnumerable<Board> GetAvailableMoves(Board board, int x, int y, PieceDirection forward)
        {
            var boards = new List<Board>();



            return boards;
        }

        public bool IsMovingPossible(Board board, int x, int y, int nx, int ny, PieceDirection forward)
        {
            int sx = Math.Abs(nx - x);
            int sy = Math.Abs(ny - y);
            int totalDisplacement = sx + sy;

            if (totalDisplacement < 2) //if you are trying to move directly 1 space [up, down, left, right], no-go
                return false;

            if (totalDisplacement == 2) //if diagonal move 1 space
            {

            }
            else //a jump
            {

            }

            return false;
        }

        public void Draw()
        {
            displayer.Draw(CurrentBoard, InPlayPieces);
        }

        public void Turn()
        {
            CurrentPlayer.GetNextMove(CurrentBoard);

            isBlackTurn = !isBlackTurn;
        }
    }
}
