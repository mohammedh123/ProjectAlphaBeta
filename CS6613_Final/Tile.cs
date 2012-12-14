// Mohammed Hossain 12/12/12

namespace CS6613_Final
{
    //a very barren implementation of Tile: simply a color
    internal class Tile
    {
        public Tile(TileColor color)
        {
            Color = color;
        }

        public TileColor Color { get; private set; }
    }
}