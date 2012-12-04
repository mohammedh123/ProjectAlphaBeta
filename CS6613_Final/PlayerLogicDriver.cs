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
        private IEnumerable<MoveResult> possibleMoves = null; 

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
            possibleMoves = game.Board.GetAvailableMoves(piece, false);

            foreach (var move in possibleMoves)
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
                        var piece = game.Board.GetPieceAtPosition(ix, iy);

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
                            var clickedPiece = game.Board.GetPieceAtPosition(ix, iy);

                            if (clickedPiece != null && clickedPiece.Color == Color)
                                SetSelectedPiece(game, clickedPiece);
                            else if (game.Board.TileBoard.IsValidLocation(ix, iy))
                            {
                                //user potentially clicked a new space for the CurrentSelectedPiece
                                var returnVal = game.Board.IsMovePossible(CurrentSelectedPiece, ix, iy, possibleMoves);

                                //move is valid - now see if there are any jumps
                                //if there are, and the move isn't a jump, then it is not valid

                                var anyJumpPossible =
                                    game.Board.GetAllAvailableMoves(Color).Any(m => m.Type == MoveType.Jump);
                                
                                if ((anyJumpPossible && returnVal == MoveType.Jump) || (!anyJumpPossible && returnVal != MoveType.None))
                                {
                                    result = game.Board.MovePiece(returnVal, CurrentSelectedPiece, ix, iy);
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