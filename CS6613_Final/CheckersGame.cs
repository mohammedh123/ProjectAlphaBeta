using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WinForms = System.Windows.Forms;

namespace CS6613_Final
{
    public enum GameState
    {
        PlayerQuery,
        SelectingPiece,
        MovingPiece,
        WaitingForComputer,
        GameOver
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CheckersGame : Game
    {
        public const int TileSize = 64;
        private const int SideSize = 200;
        public static Texture2D BlankTexture;

        private readonly GraphicsDeviceManager _graphics;
        public GameState CurrentGameState = GameState.WaitingForComputer;
        private CheckersBoardGame _cgame;
        private SpriteBatch _spriteBatch;
        private SpriteFont _turnFont;

        public CheckersGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;

            Content.RootDirectory = "Content";
        }

        private int BoardWidthInPixels
        {
            get { return _cgame.Board.TileBoard.Width*TileSize; }
        }

        private int BoardHeightInPixels
        {
            get { return _cgame.Board.TileBoard.Height*TileSize; }
        }
        
        public bool IsWithinBoard(int x, int y)
        {
            var rect = new Rectangle(0, 0, BoardWidthInPixels, BoardHeightInPixels);

            return rect.Contains(x, y);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
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

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                // Allows the game to exit
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Exit();

                if (_cgame != null)
                {
                    if (_cgame.Board.IsGameOver(_cgame.CurrentPlayer.Color))
                    {
                        //Draw(gameTime);
                        CurrentGameState = GameState.GameOver;

                        var winningPlayerState = _cgame.Board.GetGameResultState(_cgame.CurrentPlayer.Color);
                        var winningPlayer = winningPlayerState == GameResult.BlackWins ? "One (Black)" : "Two (Red)";

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
                    else
                    {
                        _cgame.AttemptTurn();
                    }
                }

                InputManager.PostUpdate();
            }

            base.Update(gameTime);
        }

        private void RestartGame()
        {
            Console.Clear();
            _cgame = null;
            LogicDriver pOne = null, pTwo = null;
            bool playerWantsToGoFirst = false;

            var newForm = new SplashScreen();
            newForm.ShowDialog();
            var result = newForm.Result;

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
                var humanPlayerText = "Player One";

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

            _cgame = new CheckersBoardGame();
            _cgame.Start(6, pOne,
                        pTwo,
                        new GuiDrawer(_spriteBatch, Content, this),
                        playerWantsToGoFirst);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);
            if (_cgame != null)
            {
                _spriteBatch.Begin();

                _cgame.Draw();

                var finalStr = String.Format("{0}'s turn.", _cgame.CurrentPlayer.Color);

                Utility.DrawStringToFitBox(_spriteBatch,
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