using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    public enum TileColor
    {
        BLACK,
        WHITE
    }

    class Board
    {
        public int Width    { get; private set; }
        public int Height   { get; private set; }

        public Tile[,] Tiles { get; private set; }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;

            Tiles = new Tile[width, height];

            for(int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    bool isTileBlack = (j + i) % 2 != 0;
                    Tiles[i, j] = new Tile(i, j, isTileBlack ? TileColor.BLACK : TileColor.WHITE);
                }
        }

        public Tile GetTile(int x, int y)
        {
            return Tiles[x, y];
        }

        public string GetNameForLocation(int x, int y)
        {
            char row, column;
            row = (char)('1' - 1 + Height - y);
            column = (char)('A' - 1 + x);

            return "" + row + column;
        }

        public bool IsValidLocation(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
