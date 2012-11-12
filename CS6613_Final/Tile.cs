using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    class Tile
    {
        private Location loc;
        public TileColor Color { get; private set;}

        public Tile(int x, int y, TileColor color)
        {
            loc = new Location(x, y);

            this.Color = color;
        }
    }
}
