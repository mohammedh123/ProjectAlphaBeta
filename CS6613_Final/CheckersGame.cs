using System;
using System.Collections.Generic;
using System.Linq;
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

        GraphicsDeviceManager graphics;
        CheckersBoardGame cgame;
        SpriteBatch spriteBatch;
        SpriteFont turnFont;
        CheckersPiece selectedPiece = null;
        GameState state = GameState.PLAYER_QUERY;
        MouseState oldMouseState = Mouse.GetState();
        
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

        public bool LeftMouseClick()
        {
            var kdlsf = Mouse.GetState();
            return Mouse.GetState().LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released;
        }

        public bool LeftMouseDown()
        {
            return Mouse.GetState().LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool LeftMouseReleased()
        {
            return Mouse.GetState().LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool IsWithinBoard(int x, int y)
        {
            Rectangle rect = new Rectangle(0, 0, BoardWidthInPixels, BoardHeightInPixels);

            return rect.Contains(x, y);
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

            ILogicDriver pOne = new PlayerLogicDriver();
            ILogicDriver pTwo = new AILogicDriver();

            bool playerWantsToGoFirst = false;

            if (state == GameState.PLAYER_QUERY)
            {
                state = GameState.WAITING_FOR_AI;

                if (pOne is PlayerLogicDriver || pTwo is PlayerLogicDriver)
                {
                    var result = WinForms.MessageBox.Show("Would you like to go first?", "Choice", WinForms.MessageBoxButtons.YesNo);
                    if (result == WinForms.DialogResult.Yes)
                    {
                        playerWantsToGoFirst = true;

                        state = GameState.SELECTING_PIECE;
                    }
                }                
            }

            cgame.Start(6, new PlayerLogicDriver(),
                new AILogicDriver(),
                new GUIDrawer(spriteBatch, Content),
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
                if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                    this.Exit();

                //let the player continue doing what hes doing until he makes a move
                // e.g. clicking a piece and then clicking a new place
                if (state == GameState.SELECTING_PIECE)
                {
                    if (LeftMouseClick())
                    {
                        int mx = Mouse.GetState().X, my = Mouse.GetState().Y;
                        int ix = mx / TILE_SIZE, iy = my / TILE_SIZE;

                        if (IsWithinBoard(mx, my))
                        {
                            //see if mouse clicked on a piece
                            var piece = cgame.GetPieceAtPosition(ix, iy);

                            //see if piece is for the right player
                            if (piece != null && piece.Color == cgame.CurrentPlayer.Color)
                            {
                                state = GameState.MOVING_PIECE;
                                selectedPiece = piece;
                            }
                            else { } //ignore the player trying to move a piece that is not his
                        }
                        else { } //ignore a click on the board that isnt on a piece
                    }
                }
                else if (state == GameState.MOVING_PIECE)
                {
                    int mx = Mouse.GetState().X, my = Mouse.GetState().Y;
                    int ix = mx / TILE_SIZE, iy = my / TILE_SIZE;

                    //user dragged it to a new slot
                    if (ix != selectedPiece.X && iy != selectedPiece.Y && cgame.CurrentBoard.IsValidLocation(ix, iy))
                    {
                        if (LeftMouseClick() || LeftMouseReleased()) //user clicked a new slot
                        {
                            try
                            {
                                //user potentially clicked a new space for the selectedPiece
                                var returnVal = cgame.IsMovePossible(cgame.CurrentBoard, selectedPiece.X, selectedPiece.Y, ix, iy, selectedPiece.Color, selectedPiece.Forward);

                                if (returnVal != AvailableMove.NONE)
                                {
                                    cgame.MovePiece(selectedPiece, ix, iy);
                                }
                                else
                                    throw new InvalidMoveException(selectedPiece, ix, iy);
                            }
                            catch (InvalidMoveException ex)
                            {
                                var result = WinForms.MessageBox.Show(String.Format("Error: Invalid move (cannot move {0} to {1}).",
                                    cgame.CurrentBoard.GetNameForLocation(ex.MovingPiece.X, ex.MovingPiece.Y),
                                    cgame.CurrentBoard.GetNameForLocation(ex.AttemptedLocation)), "Error", WinForms.MessageBoxButtons.OK);

                                selectedPiece = null;
                                state = GameState.SELECTING_PIECE;
                            }
                        }
                    }
                }

                oldMouseState = Mouse.GetState();
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

            Utility.DrawStringToFitBox(spriteBatch, 
                turnFont, 
                new Rectangle(BoardWidthInPixels, 0, SIDE_SIZE, BoardHeightInPixels), 
                String.Format("It is {0}'s turn{1}.", 
                    cgame.CurrentPlayer.Color, 
                    cgame.CurrentPlayer.IsPlayer ? " (YOUR TURN)" : " (COMPUTER's TURN)"), 
                TextAlignment.H_CENTER, 0, 
                Color.White, 
                Color.Black);
            
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
