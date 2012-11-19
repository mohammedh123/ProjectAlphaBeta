using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using WinForms = System.Windows.Forms;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace CS6613_Final
{
    class PlayerLogicDriver : ILogicDriver
    {
        readonly CheckersGame checkersGame;
        public CheckersPiece CurrentSelectedPiece { get; set; }
        
        public override bool IsPlayer
        {
            get
            {
                return true;
            }
        }

        public PlayerLogicDriver(CheckersGame game)
        {
            checkersGame = game;
        }

        public override TurnResult GetNextMove(CheckersBoardGame game)
        {
            var result = TurnResult.NotDone;

            //let the player continue doing what hes doing until he makes a move
            // e.g. clicking a piece and then clicking a new place
            if (checkersGame.CurrentGameState == GameState.SELECTING_PIECE)
            {
                if (InputManager.LeftMouseClick())
                {
                    int mx = Mouse.GetState().X, my = Mouse.GetState().Y;
                    int ix = mx / CheckersGame.TileSize, iy = my / CheckersGame.TileSize;

                    if (checkersGame.IsWithinBoard(mx, my))
                    {
                        //see if mouse clicked on a piece
                        var piece = game.GetPieceAtPosition(ix, iy);

                        //see if piece is for the right player
                        if (piece != null && piece.Color == Color)
                        {
                            checkersGame.CurrentGameState = GameState.MOVING_PIECE;
                            checkersGame.InitialMouseClick = new Location(mx, my);
                            CurrentSelectedPiece = piece;
                        }
                        else { } //ignore the player trying to move a piece that is not his
                    }
                    else { } //ignore a click on the board that isnt on a piece
                }
            }
            else if (checkersGame.CurrentGameState == GameState.MOVING_PIECE)
            {
                Debug.Assert(CurrentSelectedPiece != null);

                int mx = Mouse.GetState().X, my = Mouse.GetState().Y;
                checkersGame.GhostSelectedPosition = new Vector2(mx, my);
                int ix = mx / CheckersGame.TileSize, iy = my / CheckersGame.TileSize;

                //user dragged it to a new slot
                if (ix != CurrentSelectedPiece.X || iy != CurrentSelectedPiece.Y)
                {
                    if (InputManager.LeftMouseClick() || InputManager.LeftMouseReleased()) //user clicked a new slot
                    {
                        try
                        {
                            var clickedPiece = game.GetPieceAtPosition(ix, iy);

                            if (clickedPiece != null && clickedPiece.Color == Color)
                                CurrentSelectedPiece = clickedPiece;
                            else if (game.Board.TileBoard.IsValidLocation(ix, iy))
                            {
                                //user potentially clicked a new space for the CurrentSelectedPiece
                                var returnVal = game.IsMovePossible(game.Board.TileBoard, CurrentSelectedPiece.X, CurrentSelectedPiece.Y, ix, iy, CurrentSelectedPiece.Color, CurrentSelectedPiece.Forward);

                                if (returnVal != AvailableMove.None)
                                {
                                    result = game.MovePiece(CurrentSelectedPiece, ix, iy);
                                    checkersGame.CurrentGameState = GameState.SELECTING_PIECE;
                                    CurrentSelectedPiece = null;
                                }
                                else
                                    throw new InvalidMoveException(CurrentSelectedPiece, ix, iy);
                            }
                        }
                        catch (InvalidMoveException ex)
                        {
                            WinForms.MessageBox.Show(String.Format("Error: Invalid move (cannot move {0} to {1}).",
                                game.Board.TileBoard.GetNameForLocation(ex.MovingPiece.X, ex.MovingPiece.Y),
                                game.Board.TileBoard.GetNameForLocation(ex.AttemptedLocation)), "Error", WinForms.MessageBoxButtons.OK);

                            CurrentSelectedPiece = null;
                            checkersGame.CurrentGameState = GameState.SELECTING_PIECE;
                        }
                    }
                }
            }

            return result;
        }
    }
}
