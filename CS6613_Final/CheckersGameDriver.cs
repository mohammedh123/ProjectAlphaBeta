// Mohammed Hossain 12/12/12

using System.Collections.Generic;

namespace CS6613_Final
{
    // a bunch of enums signifying Checkers game elements: the color of the piece, the direction that a piece goes forward, the type of move available for a piece, and the result of the game
    public enum PieceColor
    {
        Black,
        Red
    }

    public enum PieceDirection
    {
        Up = -1,
        Down = 1
    }

    public enum MoveType
    {
        None,
        Jump,
        Forward
    }

    public enum GameResult
    {
        BlackWins,
        RedWins,
        StillGoing
    }

    // the driver of the game - controls the actual game flow of the Checkers game
    internal class CheckersGameDriver
    {
        public CheckersBoard Board { get; set; }

        private DisplayDriver _displayer;
        private bool _isBlackTurn = true;
        private LogicDriver _playerOne, _playerTwo;
        private List<MoveResult> _currentPossibleMoves;  

        // returns the current player (determined by the bool variable _isBlackTurn)
        public LogicDriver CurrentPlayer
        {
            get
            {
                if (_isBlackTurn)
                    return _playerOne.Color == PieceColor.Black ? _playerOne : _playerTwo;

                return _playerOne.Color == PieceColor.Red ? _playerOne : _playerTwo;
            }
        }

        // returns the currently selected piece, only if one of the players is a human player (meaningless in computer player senses)
        public CheckersPiece SelectedPiece
        {
            get
            {
                CheckersPiece selectedPiece = null;

                if (_playerOne.IsPlayer || _playerTwo.IsPlayer)
                {
                    if (_playerOne.IsPlayer)
                    {
                        var playerLogicDriver = _playerOne as PlayerLogicDriver;
                        if (playerLogicDriver != null)
                            selectedPiece = playerLogicDriver.CurrentSelectedPiece;
                    }

                    if (selectedPiece == null && _playerTwo.IsPlayer)
                    {
                        var logicDriver = _playerTwo as PlayerLogicDriver;
                        if (logicDriver != null)
                            selectedPiece = logicDriver.CurrentSelectedPiece;
                    }

                    return selectedPiece;
                }

                return null;
            }
        }

        // begins a new game of Checkers given the players and a displayer
        public void Start(int numPieces, LogicDriver pOne, LogicDriver pTwo, DisplayDriver displayer,
                          bool playerWantsToGoFirst)
        {
            Board = new CheckersBoard
                        {
                            TileBoard = new Board(6, 6),
                            Pieces = new SetOfPieces()
                        };

            var pOneColor = -1;// r.Next(0, 2); // 0 = black, 1 = red

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
            else
            {
                pOneColor = 1;
            }

            if (!pOne.IsPlayer && !pTwo.IsPlayer)
                pOneColor = 0;

            _playerOne = pOne;
            _playerOne.Color = pOneColor == 0 ? PieceColor.Black : PieceColor.Red;

            //set up the board and assign pieces on black tiles
            for (var j = 0; j < Board.TileBoard.Height; j++)
            {
                for (var i = 0; i < Board.TileBoard.Width; i++)
                {
                    if (Board.TileBoard.GetTile(i, j).Color == TileColor.Black)
                        Board.Pieces.AddPiece(PieceColor.Black, i, j, PieceDirection.Down);

                    if (Board.Pieces.AlivePlayerOnePieces.Count == numPieces)
                        break;
                }

                if (Board.Pieces.AlivePlayerOnePieces.Count == numPieces)
                    break;
            }

            _playerTwo = pTwo;
            _playerTwo.Color = _playerOne.Color == PieceColor.Red ? PieceColor.Black : PieceColor.Red;

            //set up the board and assign pieces on black tiles...again
            for (var j = Board.TileBoard.Height - 1; j >= 0; j--)
            {
                for (var i = 0; i < Board.TileBoard.Width; i++)
                {
                    if (Board.TileBoard.GetTile(i, j).Color == TileColor.Black)
                        Board.Pieces.AddPiece(PieceColor.Red, i, j, PieceDirection.Up);

                    if (Board.Pieces.AlivePlayerTwoPieces.Count == numPieces)
                        break;
                }

                if (Board.Pieces.AlivePlayerTwoPieces.Count == numPieces)
                    break;
            }

            _displayer = displayer;
        }

        // calls the displayer's Draw function
        public void Draw()
        {
            _displayer.Draw(Board.TileBoard, Board.Pieces.AlivePlayerOnePieces, Board.Pieces.AlivePlayerTwoPieces, _currentPossibleMoves, SelectedPiece);
        }

        // tells the current player that its his turn; if the player reports that he is not done with his move, then it doesn't switch players until he is
        public void AttemptTurn()
        {
            if (_currentPossibleMoves == null)
                _currentPossibleMoves = Board.GetAllAvailableMoves(CurrentPlayer.Color);

            var result = CurrentPlayer.GetNextMove(this);

            if (result == TurnResult.Finished)
            {
                SwitchPlayer();
                _currentPossibleMoves = null;
            }
        }

        // switches the current player
        public void SwitchPlayer()
        {
            _isBlackTurn = !_isBlackTurn;
        }
    }
}