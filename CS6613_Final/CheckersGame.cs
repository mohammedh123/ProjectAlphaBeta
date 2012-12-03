using System;
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
        WaitingForComputer
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

        public Vector2 GhostSelectedPosition { get; set; }
        public Location InitialMouseClick { get; set; }

        private int BoardWidthInPixels
        {
            get { return _cgame.Board.TileBoard.Width*TileSize; }
        }

        private int BoardHeightInPixels
        {
            get { return _cgame.Board.TileBoard.Height*TileSize; }
        }

        public bool ShouldDrawGhostPiece
        {
            get
            {
                return CurrentGameState == GameState.MovingPiece &&
                       Mouse.GetState().LeftButton == ButtonState.Pressed &&
                       (Mouse.GetState().X != InitialMouseClick.X || Mouse.GetState().Y != InitialMouseClick.Y);
            }
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

            _cgame = new CheckersBoardGame();

            LogicDriver pOne = new PlayerLogicDriver(this);
            LogicDriver pTwo = new ComputerLogicDriver();

            bool playerWantsToGoFirst = false;

            if (pOne.IsPlayer || pTwo.IsPlayer)
            {
                CurrentGameState = GameState.WaitingForComputer;
                string whichLogicIsPlayer = pOne.IsPlayer ? "Player One" : "Player Two";

                WinForms.DialogResult result =
                    WinForms.MessageBox.Show(String.Format("Would you ({0}) like to go first?", whichLogicIsPlayer),
                                             "Choice", WinForms.MessageBoxButtons.YesNo);

                if (result == WinForms.DialogResult.Yes)
                {
                    playerWantsToGoFirst = true;

                    CurrentGameState = GameState.SelectingPiece;
                }
            }

            _cgame.Start(6, pOne,
                        pTwo,
                        new GuiDrawer(_spriteBatch, Content, this),
                        //new ConsoleDrawer()
                        playerWantsToGoFirst);

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

                _cgame.AttemptTurn();

                InputManager.PostUpdate();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);
            _spriteBatch.Begin();

            _cgame.Draw();

            string finalStr = "";
            finalStr += String.Format("It is {0}'s turn{1}.",
                                      _cgame.CurrentPlayer.Color,
                                      _cgame.CurrentPlayer.IsPlayer ? " (YOUR TURN)" : " (COMPUTER's TURN)");

            finalStr += Environment.NewLine + Environment.NewLine;

            if (_cgame.SelectedPiece != null)
                finalStr += String.Format("Selected: {0}",
                                          _cgame.Board.TileBoard.GetNameForLocation(_cgame.SelectedPiece.X,
                                                                                   _cgame.SelectedPiece.Y));

            finalStr += Environment.NewLine + Environment.NewLine;
            finalStr += "Current state: " + CurrentGameState;
            finalStr += Environment.NewLine + Environment.NewLine;
            int mx = Mouse.GetState().X;
            int my = Mouse.GetState().Y;
            int ix = mx/TileSize, iy = my/TileSize;
            finalStr += String.Format("({0},{1})", ix, iy);

            Utility.DrawStringToFitBox(_spriteBatch,
                                       _turnFont,
                                       new Rectangle(BoardWidthInPixels, 0, SideSize, BoardHeightInPixels),
                                       finalStr,
                                       TextAlignment.Center, 0,
                                       Color.White,
                                       Color.Black);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}