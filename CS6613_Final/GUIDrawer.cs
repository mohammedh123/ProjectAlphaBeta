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
        private Texture2D _blackPiece, _blackTile, _redPiece, _whiteTile;

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

            for (int i = 0; i < playerOnePieces.Count; i++)
            {
                CheckersPiece piece = playerOnePieces[i];
                Texture2D pieceTexture = _blackPiece;

                _spriteBatch.Draw(pieceTexture, new Rectangle(piece.X * TileSize, piece.Y * TileSize, TileSize, TileSize),
                                  Color.White);
            }

            for (int i = 0; i < playerTwoPieces.Count; i++)
            {
                CheckersPiece piece = playerTwoPieces[i];
                Texture2D pieceTexture = _redPiece;

                _spriteBatch.Draw(pieceTexture, new Rectangle(piece.X * TileSize, piece.Y * TileSize, TileSize, TileSize),
                                  Color.White);
            }
        }
    }
}