// Mohammed Hossain 12/12/12

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using WinForms = System.Windows.Forms;

namespace CS6613_Final
{
    //PlayerLogicDriver: an implementation of LogicDriver that listens to the user's input (clicking on tiles, clicking valid moves, etc)
    internal class PlayerLogicDriver : LogicDriver
    {
        private readonly XnaCheckersDriver _xnaCheckersDriver;
        private List<MoveResult> _allAvailableMoves, _selectedAvailableMoves; 

        public PlayerLogicDriver(XnaCheckersDriver driver)
        {
            _xnaCheckersDriver = driver;
        }

        public CheckersPiece CurrentSelectedPiece { get; set; }

        public override bool IsPlayer
        {
            get { return true; }
        }

        private void SetSelectedPiece(CheckersGameDriver gameDriver, CheckersPiece piece)
        {
            _xnaCheckersDriver.CurrentGameState = GameState.MovingPiece;
            CurrentSelectedPiece = piece;
            _allAvailableMoves = gameDriver.Board.GetAllAvailableMoves(Color);
            _selectedAvailableMoves = gameDriver.Board.GetAvailableMovesForPiece(piece, false);
        }

        public override TurnResult GetNextMove(CheckersGameDriver gameDriver)
        {
            if(_xnaCheckersDriver.CurrentGameState == GameState.WaitingForComputer)
                _xnaCheckersDriver.CurrentGameState = GameState.SelectingPiece;

            var result = TurnResult.NotDone;

            //let the player continue doing what hes doing until he makes a move
            // e.g. clicking a piece and then clicking a new place
            if (_xnaCheckersDriver.CurrentGameState == GameState.SelectingPiece || _xnaCheckersDriver.CurrentGameState == GameState.MovingPiece)
            {
                if (InputManager.LeftMouseClick())
                {
                    int mx = Mouse.GetState().X, my = Mouse.GetState().Y;
                    int ix = mx/XnaCheckersDriver.TileSize, iy = my/XnaCheckersDriver.TileSize;

                    if (_xnaCheckersDriver.IsWithinBoard(mx, my))
                    {
                        //see if mouse clicked on a piece
                        var piece = gameDriver.Board.Pieces.GetPieceAtPosition(ix, iy);

                        //see if piece is for the right player
                        if (piece != null && piece.Color == Color)
                        {
                            SetSelectedPiece(gameDriver, piece);
                        }
                        else
                        {
                        } //ignore the player trying to move a piece that is not his
                    }
                    else
                    {
                    } //ignore a click on the gameDriver that isnt on a piece
                }
            }
            if (_xnaCheckersDriver.CurrentGameState == GameState.MovingPiece)
            {
                Debug.Assert(CurrentSelectedPiece != null);

                int mx = Mouse.GetState().X, my = Mouse.GetState().Y;
                int ix = mx/XnaCheckersDriver.TileSize, iy = my/XnaCheckersDriver.TileSize;

                //user dragged it to a new slot
                if (ix != CurrentSelectedPiece.X || iy != CurrentSelectedPiece.Y)
                {
                    if (InputManager.LeftMouseClick()) //user clicked a new slot
                    {
                        try
                        {
                            //check if the clicked location matches one of the available moves for the currently selected piece
                            var clickedPiece = gameDriver.Board.Pieces.GetPieceAtPosition(ix, iy);
                            var appropriateMove =
                                _selectedAvailableMoves.FirstOrDefault(mr => mr.FinalPieceLocation.X == ix && mr.FinalPieceLocation.Y == iy);
                            var anyJumpsAvailable = _allAvailableMoves.Any(mr => mr.Type == MoveType.Jump);
                            
                            if (clickedPiece != null && clickedPiece.Color == Color)
                                SetSelectedPiece(gameDriver, clickedPiece);
                            else if (appropriateMove != null)
                            {
                                if (!(anyJumpsAvailable && appropriateMove.Type == MoveType.Forward))
                                {
                                    result = gameDriver.Board.MovePiece(appropriateMove, Color);
                                    _xnaCheckersDriver.CurrentGameState = GameState.SelectingPiece;
                                    CurrentSelectedPiece = null;
                                }
                                else
                                    throw new InvalidMoveException(CurrentSelectedPiece, ix, iy);
                            }
                        }
                        catch (InvalidMoveException ex)
                        {
                            WinForms.MessageBox.Show(String.Format("Error: Invalid move (cannot move {0} to {1}).",
                                                                   gameDriver.Board.TileBoard.GetNameForLocation(
                                                                       ex.MovingPiece.X, ex.MovingPiece.Y),
                                                                   gameDriver.Board.TileBoard.GetNameForLocation(
                                                                       ex.AttemptedLocation)), "Error",
                                                     WinForms.MessageBoxButtons.OK);

                            CurrentSelectedPiece = null;
                            _xnaCheckersDriver.CurrentGameState = GameState.SelectingPiece;
                        }
                    }
                }
            }

            return result;
        }
    }
}