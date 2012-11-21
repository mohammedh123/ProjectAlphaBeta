namespace CS6613_Final
{
    internal class Tile
    {
        private Location _loc;

        public Tile(int x, int y, TileColor color)
        {
            _loc = new Location(x, y);

            Color = color;
        }

        public TileColor Color { get; private set; }
    }
}