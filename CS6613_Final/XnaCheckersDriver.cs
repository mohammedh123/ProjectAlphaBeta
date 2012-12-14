// Mohammed Hossain 12/12/12

using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WinForms = System.Windows.Forms;

namespace CS6613_Final
{
    //GameState: the possible states of the game
    public enum GameState
    {
        PlayerQuery,
        SelectingPiece,
        MovingPiece,
        WaitingForComputer,
        GameOver
    }

    //XnaCheckersDriver: the implemented functions of an XNA game that allow it to open a window, process input, etc
    public class XnaCheckersDriver : Game
    {
        public GameState CurrentGameState = GameState.WaitingForComputer;
        public const int TileSize = 64;
        public static Texture2D BlankTexture;

        private const int SideSize = 200;
        private readonly GraphicsDeviceManager _graphics;
        private CheckersGameDriver _cgame;
        private SpriteBatch _spriteBatch;
        private SpriteFont _turnFont;

        public XnaCheckersDriver()
        {
            _graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;

            Content.RootDirectory = "Content";
        }

        //a quick way of grabbing the number of pixels a board takes up in width
        private int BoardWidthInPixels
        {
            get { return _cgame.Board.TileBoard.Width*TileSize; }
        }

        //a quick way of grabbing the number of pixels a board takes up in width
        private int BoardHeightInPixels
        {
            get { return _cgame.Board.TileBoard.Height*TileSize; }
        }

        //a quick way of telling whether the given coordinates is within the Checkers board
        public bool IsWithinBoard(int x, int y)
        {
            var rect = new Rectangle(0, 0, BoardWidthInPixels, BoardHeightInPixels);

            return rect.Contains(x, y);
        }

        //loads content/loads textures
        protected override void LoadContent()
        {
            _turnFont = Content.Load<SpriteFont>("turnFont");

            BlankTexture = new Texture2D(GraphicsDevice, 1, 1);
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            RestartGame();

            _graphics.PreferredBackBufferWidth = BoardWidthInPixels + SideSize;
            _graphics.PreferredBackBufferHeight = BoardHeightInPixels;
            _graphics.ApplyChanges();
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        //the main update function of the Xna driver; it consists of exiting the game if the user presses Escape, and also consists of polling the player for his turn.
        //if the game is over, then it will handle that as well and call the appropriate function
        //also updates the InputManager
        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                // Allows the gameDriver to exit
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Exit();

                if (_cgame != null)
                {
                    if (_cgame.Board.IsGameOver(_cgame.CurrentPlayer.Color))
                    {
                        HandleGameOver();
                    }
                    else
                    {
                        _cgame.AttemptTurn();
                    }
                }

                InputManager.PostUpdate();
            }

            base.Update(gameTime);
        }

        private void HandleGameOver()
        {
            CurrentGameState = GameState.GameOver;

            var winningPlayerState = _cgame.Board.GetGameResultState(_cgame.CurrentPlayer.Color);
            var winningPlayer = winningPlayerState == GameResult.BlackWins ? "One (Black)" : "Two (Red)";

            //if the game is over, then show a dialog and ask the user if he wants to play again, or exit the game
            var dialogThread = new Thread(() =>
                                              {
                                                  var dialogResult = WinForms.MessageBox.Show(
                                                      String.Format(
                                                          "Player {0} has won. Would you like to play again?",
                                                          winningPlayer),
                                                      "Game Over",
                                                      WinForms.MessageBoxButtons.YesNo,
                                                      WinForms.MessageBoxIcon.Question,
                                                      WinForms.MessageBoxDefaultButton.Button1);

                                                  if (dialogResult == WinForms.DialogResult.Yes)
                                                  {
                                                      RestartGame();
                                                  }
                                                  else if (dialogResult == WinForms.DialogResult.No)
                                                  {
                                                      Exit();
                                                  }
                                              });

            //a quick hack to make sure that the graphics thread draws the final state
            //this only matters when the ai is computing a move, because it is computing a move in a different thread and might miss a draw call after finding one
            dialogThread.Start();
            dialogThread.Join(1);
        }

        //a function to take in the input and process it
        private void RestartGame()
        {
            Console.Clear();
            _cgame = null;
            LogicDriver pOne = null, pTwo = null;
            var playerWantsToGoFirst = false;

            var newForm = new SplashScreen();
            newForm.ShowDialog();
            var result = newForm.Result;

            //depending on the TypeOfMatch, the new CheckersGame must have appropriate logic drivers
            if(result.TypeOfMatch == MatchType.PvP)
            {
                pOne = new PlayerLogicDriver(this);
                pTwo = new PlayerLogicDriver(this);

                CurrentGameState = GameState.SelectingPiece;
            }
            else if (result.TypeOfMatch == MatchType.PvC)
            {
                pOne = new PlayerLogicDriver(this);
                pTwo = new ComputerLogicDriver(result.ComputerOneDifficulty);

                CurrentGameState = GameState.WaitingForComputer;
                const string humanPlayerText = "Player One";

                var goFirstResult =
                    WinForms.MessageBox.Show(String.Format("Would you ({0}) like to go first?", humanPlayerText),
                                             "Choice", WinForms.MessageBoxButtons.YesNo);

                if (goFirstResult == WinForms.DialogResult.Yes)
                {
                    playerWantsToGoFirst = true;

                    CurrentGameState = GameState.SelectingPiece;
                }
            }
            else if (result.TypeOfMatch == MatchType.CvC)
            {
                pOne = new ComputerLogicDriver(result.ComputerOneDifficulty);
                pTwo = new ComputerLogicDriver(result.ComputerTwoDifficulty);
            }

            _cgame = new CheckersGameDriver();
            _cgame.Start(6, pOne,
                        pTwo,
                        new GuiDrawer(_spriteBatch, Content),
                        playerWantsToGoFirst);
        }

        //all it will do is tells the CheckersGame to draw itself and draw whoever's turn it is on the right panel
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);
            if (_cgame != null)
            {
                _spriteBatch.Begin();

                _cgame.Draw();

                var finalStr = String.Format("{0}'s turn.", _cgame.CurrentPlayer.Color);

                ExTextDrawer.DrawStringToFitBox(_spriteBatch,
                                           _turnFont,
                                           new Rectangle(BoardWidthInPixels, 0, SideSize, BoardHeightInPixels),
                                           finalStr,
                                           TextAlignment.Center, 0,
                                           Color.White,
                                           Color.Black);

                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}