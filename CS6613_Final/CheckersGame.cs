using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using WinForms = System.Windows.Forms;

namespace CS6613_Final
{
    public enum GameState
    {
        PLAYER_QUERY,
        SELECTING_PIECE,
        MOVING_PIECE,
        WAITING_FOR_AI
    }
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CheckersGame : Microsoft.Xna.Framework.Game
    {
        public static Texture2D BlankTexture;
        public const int TILE_SIZE = 64;
        public GameState CurrentGameState = GameState.WAITING_FOR_AI;
        public Vector2 GhostSelectedPosition { get; set; }
        public Location InitialMouseClick { get; set; }

        GraphicsDeviceManager graphics;
        CheckersBoardGame cgame;
        SpriteBatch spriteBatch;
        SpriteFont turnFont;
        
        private int BoardWidthInPixels
        {
            get { return cgame.CurrentBoard.Width * TILE_SIZE; }
        }

        private int BoardHeightInPixels
        {
            get { return cgame.CurrentBoard.Height * TILE_SIZE; }
        }

        private const int SIDE_SIZE = 200;

        public CheckersGame()
        {            
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;

            Content.RootDirectory = "Content";
        }

        public bool IsWithinBoard(int x, int y)
        {
            Rectangle rect = new Rectangle(0, 0, BoardWidthInPixels, BoardHeightInPixels);

            return rect.Contains(x, y);
        }

        public bool ShouldDrawGhostPiece
        {
            get
            {
                return CurrentGameState == GameState.MOVING_PIECE &&
                        Mouse.GetState().LeftButton == ButtonState.Pressed &&
                        (Mouse.GetState().X != InitialMouseClick.X || Mouse.GetState().Y != InitialMouseClick.Y);
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            turnFont = Content.Load<SpriteFont>("turnFont");

            CheckersGame.BlankTexture = new Texture2D(GraphicsDevice, 1, 1);
            spriteBatch = new SpriteBatch(GraphicsDevice);

            cgame = new CheckersBoardGame();

            ILogicDriver pOne = new PlayerLogicDriver(this);
            ILogicDriver pTwo = new AILogicDriver();

            bool playerWantsToGoFirst = false;

            if (pOne.IsPlayer || pTwo.IsPlayer)
            {
                CurrentGameState = GameState.WAITING_FOR_AI;
                var whichLogicIsPlayer = pOne.IsPlayer ? "Player One" : "Player Two";

                var result = WinForms.MessageBox.Show(String.Format("Would you ({0}) like to go first?", whichLogicIsPlayer), 
                    "Choice", WinForms.MessageBoxButtons.YesNo);

                if (result == WinForms.DialogResult.Yes)
                {
                    playerWantsToGoFirst = true;

                    CurrentGameState = GameState.SELECTING_PIECE;
                }            
            }

            cgame.Start(6, pOne,
                pTwo,
                new GUIDrawer(spriteBatch, Content, this),
                //new ConsoleDrawer()
                playerWantsToGoFirst);

            graphics.PreferredBackBufferWidth = BoardWidthInPixels + SIDE_SIZE;
            graphics.PreferredBackBufferHeight = BoardHeightInPixels;
            graphics.ApplyChanges();

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

                cgame.AttemptTurn();

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
            spriteBatch.Begin();

            cgame.Draw();
            
            var finalStr = "";
            finalStr += String.Format("It is {0}'s turn{1}.",
                cgame.CurrentPlayer.Color,
                cgame.CurrentPlayer.IsPlayer ? " (YOUR TURN)" : " (COMPUTER's TURN)");

            finalStr += Environment.NewLine + Environment.NewLine;

            if (cgame.SelectedPiece != null)
                finalStr += String.Format("Selected: {0}", cgame.CurrentBoard.GetNameForLocation(cgame.SelectedPiece.X, cgame.SelectedPiece.Y));

            finalStr += Environment.NewLine + Environment.NewLine;
            finalStr += "Current state: " + CurrentGameState;
            finalStr += Environment.NewLine + Environment.NewLine;
            var mx = Mouse.GetState().X;
            var my = Mouse.GetState().Y;
            int ix = mx / TILE_SIZE, iy = my / TILE_SIZE;
            finalStr += String.Format("({0},{1})", ix, iy);

            Utility.DrawStringToFitBox(spriteBatch, 
                turnFont, 
                new Rectangle(BoardWidthInPixels, 0, SIDE_SIZE, BoardHeightInPixels), 
                finalStr,
                TextAlignment.H_CENTER, 0, 
                Color.White, 
                Color.Black);
            
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
