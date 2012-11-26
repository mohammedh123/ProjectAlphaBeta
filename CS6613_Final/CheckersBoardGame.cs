﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CS6613_Final
{
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

    internal class CheckersBoardGame
    {
        private DisplayDriver _displayer;
        private bool _isBlackTurn = true;
        private LogicDriver _playerOne, _playerTwo;
        public CheckersBoard Board { get; set; }

        public LogicDriver CurrentPlayer
        {
            get
            {
                if (_isBlackTurn)
                    return _playerOne.Color == PieceColor.Black ? _playerOne : _playerTwo;

                return _playerOne.Color == PieceColor.Red ? _playerOne : _playerTwo;
            }
        }

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

        public void Start(int numPieces, LogicDriver pOne, LogicDriver pTwo, DisplayDriver displayer,
                          bool playerWantsToGoFirst)
        {
            var r = new Random();
            Board = new CheckersBoard
                        {
                            TileBoard = new Board(6, 6),
                            PlayerOnePieces = new List<CheckersPiece>(),
                            PlayerTwoPieces = new List<CheckersPiece>()
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

            _playerOne = pOne;
            _playerOne.Color = pOneColor == 0 ? PieceColor.Black : PieceColor.Red;

            for (int j = 0; j < Board.TileBoard.Height; j++)
            {
                for (int i = 0; i < Board.TileBoard.Width; i++)
                {
                    if (Board.TileBoard.GetTile(i, j).Color == TileColor.Black)
                        Board.PlayerOnePieces.Add(new CheckersPiece(i, j, _playerOne.Color, PieceDirection.Down));

                    if (Board.PlayerOnePieces.Count == numPieces)
                        break;
                }

                if (Board.PlayerOnePieces.Count == numPieces)
                    break;
            }

            _playerTwo = pTwo;
            _playerTwo.Color = _playerOne.Color == PieceColor.Red ? PieceColor.Black : PieceColor.Red;

            for (int j = Board.TileBoard.Height - 1; j >= 0; j--)
            {
                for (int i = 0; i < Board.TileBoard.Width; i++)
                {
                    if (Board.TileBoard.GetTile(i, j).Color == TileColor.Black)
                        Board.PlayerTwoPieces.Add(new CheckersPiece(i, j, _playerTwo.Color, PieceDirection.Up));

                    if (Board.PlayerTwoPieces.Count == numPieces)
                        break;
                }

                if (Board.PlayerTwoPieces.Count == numPieces)
                    break;
            }


            _displayer = displayer;
        }

        public void Draw()
        {
            _displayer.Draw(Board.TileBoard, Board.InPlayPieces, SelectedPiece);
        }

        public void AttemptTurn()
        {
            TurnResult result = CurrentPlayer.GetNextMove(this);

            if (result == TurnResult.Finished)
                SwitchPlayer();
        }

        public void SwitchPlayer()
        {
            _isBlackTurn = !_isBlackTurn;
        }
    }
}