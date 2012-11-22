using System;

namespace CS6613_Final
{
    public class Location
    {
        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public override string ToString()
        {
            return String.Format("[{0},{1}]", X, Y);
        }
    }
}