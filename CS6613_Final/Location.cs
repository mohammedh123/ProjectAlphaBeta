// Mohammed Hossain 12/12/12

using System;

namespace CS6613_Final
{
    //Location: a useful class that represents a point on the board (integer coordinates)
    public struct Location
    {
        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;

        public override string ToString()
        {
            return String.Format("[{0},{1}]", X, Y);
        }

        public static bool operator ==(Location a, Location b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Location a, Location b)
        {
            return !(a == b);
        }
    }
}