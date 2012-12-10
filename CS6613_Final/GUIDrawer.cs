using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CS6613_Final
{
    internal class GuiDrawer : DisplayDriver
    {
        private const int TileSize = CheckersGame.TileSize;
        private readonly CheckersGame _checkersGame;
        private readonly ContentManager _contentManager;
        private readonly SpriteBatch _spriteBatch;
        private readonly Vector2 _tileCenter = new Vector2(TileSize*0.5f, TileSize*0.5f);
        private Texture2D _blackPiece, _blackTile, _redPiece, _whiteTile, _selectedGlow, _moveForward;

        public GuiDrawer(SpriteBatch sb, ContentManager content, CheckersGame game)
        {
            _spriteBatch = sb;
            _contentManager = content;
            _checkersGame = game;

            LoadImages();
        }

        private void LoadImages()
        {
            _blackPiece = _contentManager.Load<Texture2D>("blackpiece");
            _blackTile = _contentManager.Load<Texture2D>("blacktile");
            _redPiece = _contentManager.Load<Texture2D>("redpiece");
            _whiteTile = _contentManager.Load<Texture2D>("whitetile");
            _selectedGlow = _contentManager.Load<Texture2D>("selectedpiece_glow");
            //_moveForward = _contentManager.Load<Texture2D>("move_forward");
        }

        public override void Draw(Board board, List<CheckersPiece> playerOnePieces, List<CheckersPiece> playerTwoPieces, CheckersPiece selectedPiece = null)
        {
            for (int i = 0; i < board.Width; i++)
            {
                for (int j = 0; j < board.Height; j++)
                {
                    Texture2D tileTexture = board.GetTile(i, j).Color == TileColor.Black ? _blackTile : _whiteTile;

                    _spriteBatch.Draw(tileTexture, new Rectangle(i*TileSize, j*TileSize, TileSize, TileSize),
                                     Color.White);
                }
            }

            DrawPieces(_blackPiece, playerOnePieces, selectedPiece);
            DrawPieces(_redPiece, playerTwoPieces, selectedPiece);
        }

        private void DrawPieces(Texture2D pieceTexture, List<CheckersPiece> pieces, CheckersPiece selectedPiece)
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                CheckersPiece piece = pieces[i];
                var rectToDrawIn = new Rectangle(piece.X*TileSize, piece.Y*TileSize, TileSize, TileSize);

                if(piece.Equals(selectedPiece))
                {
                    _spriteBatch.Draw(_selectedGlow, new Vector2(rectToDrawIn.Center.X, rectToDrawIn.Center.Y), null, Color.White, 0f, new Vector2(48, 48), 1f, SpriteEffects.None,  0);
                }

                _spriteBatch.Draw(pieceTexture, rectToDrawIn, Color.White);
            }
        }
    }
}