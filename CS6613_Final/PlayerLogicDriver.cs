using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using WinForms = System.Windows.Forms;

namespace CS6613_Final
{
    internal class PlayerLogicDriver : LogicDriver
    {
        private readonly CheckersGame _checkersGame;
        private List<MoveResult> _allAvailableMoves = null, _selectedAvailableMoves = null; 

        public PlayerLogicDriver(CheckersGame game)
        {
            _checkersGame = game;
        }

        public CheckersPiece CurrentSelectedPiece { get; set; }

        public override bool IsPlayer
        {
            get { return true; }
        }

        private void SetSelectedPiece(CheckersBoardGame game, CheckersPiece piece)
        {
            _checkersGame.CurrentGameState = GameState.MovingPiece;
            CurrentSelectedPiece = piece;
            _allAvailableMoves = game.Board.GetAllAvailableMoves(Color);
            _selectedAvailableMoves = game.Board.GetAvailableMoves(piece, false);

            foreach (var move in _allAvailableMoves)
                Console.WriteLine("Possible moves: {0} {1}", move.ToString(),
                                  move.Type == MoveType.Jump
                                      ? String.Format(" over {0}", String.Join(", ", move.JumpResults.Select(jr => jr.JumpedLocation)))
                                      : "");
        }

        public override TurnResult GetNextMove(CheckersBoardGame game)
        {
            if(_checkersGame.CurrentGameState == GameState.WaitingForComputer)
                _checkersGame.CurrentGameState = GameState.SelectingPiece;

            var result = TurnResult.NotDone;

            //let the player continue doing what hes doing until he makes a move
            // e.g. clicking a piece and then clicking a new place
            if (_checkersGame.CurrentGameState == GameState.SelectingPiece || _checkersGame.CurrentGameState == GameState.MovingPiece)
            {
                if (InputManager.LeftMouseClick())
                {
                    int mx = Mouse.GetState().X, my = Mouse.GetState().Y;
                    int ix = mx/CheckersGame.TileSize, iy = my/CheckersGame.TileSize;

                    if (_checkersGame.IsWithinBoard(mx, my))
                    {
                        //see if mouse clicked on a piece
                        var piece = game.Board.Pieces.GetPieceAtPosition(ix, iy);

                        //see if piece is for the right player
                        if (piece != null && piece.Color == Color)
                        {
                            SetSelectedPiece(game, piece);
                        }
                        else
                        {
                        } //ignore the player trying to move a piece that is not his
                    }
                    else
                    {
                    } //ignore a click on the board that isnt on a piece
                }
            }
            if (_checkersGame.CurrentGameState == GameState.MovingPiece)
            {
                Debug.Assert(CurrentSelectedPiece != null);

                int mx = Mouse.GetState().X, my = Mouse.GetState().Y;
                int ix = mx/CheckersGame.TileSize, iy = my/CheckersGame.TileSize;

                //user dragged it to a new slot
                if (ix != CurrentSelectedPiece.X || iy != CurrentSelectedPiece.Y)
                {
                    if (InputManager.LeftMouseClick()) //user clicked a new slot
                    {
                        try
                        {
                            var clickedPiece = game.Board.Pieces.GetPieceAtPosition(ix, iy);
                            var appropriateMove =
                                _selectedAvailableMoves.FirstOrDefault(mr => mr.FinalPieceLocation.X == ix && mr.FinalPieceLocation.Y == iy);
                            var anyJumpsAvailable = _allAvailableMoves.Any(mr => mr.Type == MoveType.Jump);
                            
                            if (clickedPiece != null && clickedPiece.Color == Color)
                                SetSelectedPiece(game, clickedPiece);
                            else if (appropriateMove != null)
                            {
                                if (!(anyJumpsAvailable && appropriateMove.Type == MoveType.Forward))
                                {
                                    result = game.Board.MovePiece(appropriateMove, Color);
                                    _checkersGame.CurrentGameState = GameState.SelectingPiece;
                                    CurrentSelectedPiece = null;
                                }
                                else
                                    throw new InvalidMoveException(CurrentSelectedPiece, ix, iy);
                            }
                        }
                        catch (InvalidMoveException ex)
                        {
                            WinForms.MessageBox.Show(String.Format("Error: Invalid move (cannot move {0} to {1}).",
                                                                   game.Board.TileBoard.GetNameForLocation(
                                                                       ex.MovingPiece.X, ex.MovingPiece.Y),
                                                                   game.Board.TileBoard.GetNameForLocation(
                                                                       ex.AttemptedLocation)), "Error",
                                                     WinForms.MessageBoxButtons.OK);

                            CurrentSelectedPiece = null;
                            _checkersGame.CurrentGameState = GameState.SelectingPiece;
                        }
                    }
                }
            }

            return result;
        }
    }
}