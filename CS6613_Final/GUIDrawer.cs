using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CS6613_Final
{
    class GUIDrawer : IDisplayDriver
    {
        SpriteBatch spriteBatch; 
        Texture2D blackPiece, blackTile, redPiece, whiteTile;
        ContentManager contentManager;

        const int TILE_SIZE = CheckersGame.TILE_SIZE;
        readonly Vector2 tileCenter = new Vector2(TILE_SIZE * 0.5f, TILE_SIZE * 0.5f);

        public GUIDrawer(SpriteBatch sb, ContentManager content)
        {
            spriteBatch = sb;
            contentManager = content;

            LoadImages();
        }

        private void LoadImages()
        {
            blackPiece = contentManager.Load<Texture2D>("blackpiece");
            blackTile = contentManager.Load<Texture2D>("blacktile");
            redPiece = contentManager.Load<Texture2D>("redpiece");
            whiteTile = contentManager.Load<Texture2D>("whitetile");
        }

        public override void Draw(Board board, IEnumerable<CheckersPiece> pieces)
        {
            Texture2D tileTexture;
            for (int i = 0; i < board.Width; i++)
            {
                for (int j = 0; j < board.Height; j++)
                {
                    tileTexture = board.GetTile(i, j).Color == TileColor.BLACK ? blackTile : whiteTile;

                    spriteBatch.Draw(tileTexture, new Rectangle(i * TILE_SIZE, j * TILE_SIZE, TILE_SIZE, TILE_SIZE), Color.White);
                }
            }

            Texture2D pieceTexture;
            foreach (var piece in pieces)
            {
                pieceTexture = piece.Color == PieceColor.BLACK ? blackPiece : redPiece;

                spriteBatch.Draw(pieceTexture, new Rectangle(piece.X * TILE_SIZE, piece.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE), Color.White);
            }
        }

        public override void DrawGhostPiece(Board board, CheckersPiece ghostPiece, Location pixelCoords)
        {
            var pieceTexture = ghostPiece.Color == PieceColor.BLACK ? blackPiece : redPiece;

            spriteBatch.Draw(pieceTexture, new Vector2(pixelCoords.X, pixelCoords.Y), null, Color.White*0.5f, 0.0f, tileCenter, 1.0f, SpriteEffects.None, 0);
        }
    }
}
