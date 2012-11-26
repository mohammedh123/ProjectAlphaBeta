using System;
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

        public PlayerLogicDriver(CheckersGame game)
        {
            _checkersGame = game;
        }

        public CheckersPiece CurrentSelectedPiece { get; set; }

        public override bool IsPlayer
        {
            get { return true; }
        }

        public override TurnResult GetNextMove(CheckersBoardGame game)
        {
            if(_checkersGame.CurrentGameState == GameState.WaitingForComputer)
                _checkersGame.CurrentGameState = GameState.SelectingPiece;

            var result = TurnResult.NotDone;

            //let the player continue doing what hes doing until he makes a move
            // e.g. clicking a piece and then clicking a new place
            if (_checkersGame.CurrentGameState == GameState.SelectingPiece)
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
                            _checkersGame.CurrentGameState = GameState.MovingPiece;
                            _checkersGame.InitialMouseClick = new Location(mx, my);
                            CurrentSelectedPiece = piece;
                            var moves = game.Board.GetAvailableMoves(piece, false).ToList();
                            foreach(var move in moves)
                                Console.WriteLine("Possible move: {0}", move.ToString());
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
            else if (_checkersGame.CurrentGameState == GameState.MovingPiece)
            {
                Debug.Assert(CurrentSelectedPiece != null);

                int mx = Mouse.GetState().X, my = Mouse.GetState().Y;
                _checkersGame.GhostSelectedPosition = new Vector2(mx, my);
                int ix = mx/CheckersGame.TileSize, iy = my/CheckersGame.TileSize;

                //user dragged it to a new slot
                if (ix != CurrentSelectedPiece.X || iy != CurrentSelectedPiece.Y)
                {
                    if (InputManager.LeftMouseClick() || InputManager.LeftMouseReleased()) //user clicked a new slot
                    {
                        try
                        {
                            var clickedPiece = game.Board.GetPieceAtPosition(ix, iy);

                            if (clickedPiece != null && clickedPiece.Color == Color)
                                CurrentSelectedPiece = clickedPiece;
                            else if (game.Board.TileBoard.IsValidLocation(ix, iy))
                            {
                                //user potentially clicked a new space for the CurrentSelectedPiece
                                var returnVal = game.Board.IsMovePossible(CurrentSelectedPiece, ix, iy);

                                if (returnVal != MoveType.None)
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