using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    class CheckersPiece : IEquatable<CheckersPiece>
    {
        public bool InPlay { get; set; }
        public PieceColor Color { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }

        public PieceDirection Forward { get; private set; }

        public CheckersPiece(int x, int y, PieceColor color, PieceDirection forward)
        {
            X = x;
            Y = y;

            Color = color;
            InPlay = true;
            Forward = forward;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            CheckersPiece objp = obj as CheckersPiece;
            if ((object)objp == null)
                return false;

            return X == objp.X && Y == objp.Y && Color == objp.Color && InPlay == objp.InPlay;
        }

        public bool Equals(CheckersPiece objp)
        {
            if ((object)objp == null)
                return false;

            return X == objp.X && Y == objp.Y && Color == objp.Color && InPlay == objp.InPlay;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ InPlay.GetHashCode() ^ Color.GetHashCode();
        }
    }
}
