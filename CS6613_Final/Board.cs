namespace CS6613_Final
{
    public enum TileColor
    {
        Black,
        White
    }

    internal class Board
    {
        public Board(int width, int height)
        {
            Width = width;
            Height = height;

            Tiles = new Tile[width,height];

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    bool isTileBlack = (j + i)%2 != 0;
                    Tiles[i, j] = new Tile(i, j, isTileBlack ? TileColor.Black : TileColor.White);
                }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Tile[,] Tiles { get; private set; }

        public Tile GetTile(int x, int y)
        {
            return Tiles[x, y];
        }

        public string GetNameForLocation(int x, int y)
        {
            var row = (char) ('1' - 1 + Height - y);
            var column = (char) ('A' + x);

            return "" + row + column;
        }

        public string GetNameForLocation(Location loc)
        {
            return GetNameForLocation(loc.X, loc.Y);
        }

        public bool IsValidLocation(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool IsValidLocation(Location loc)
        {
            return IsValidLocation(loc.X, loc.Y);
        }
    }
}