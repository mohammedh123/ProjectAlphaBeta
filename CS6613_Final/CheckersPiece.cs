using System;

namespace CS6613_Final
{
    public class CheckersPiece : IEquatable<CheckersPiece>
    {
        public CheckersPiece(int x, int y, PieceColor color, PieceDirection forward)
        {
            X = x;
            Y = y;

            Color = color;
            InPlay = true;
            Forward = forward;
        }

        public bool InPlay { get; set; }
        public PieceColor Color { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }

        public PieceDirection Forward { get; private set; }
        
        #region IEquatable<CheckersPiece> Members

        public bool Equals(CheckersPiece objp)
        {
            if (objp == null)
                return false;

            return X == objp.X && Y == objp.Y && Color == objp.Color && InPlay == objp.InPlay;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var objp = obj as CheckersPiece;
            if (objp == null)
                return false;

            return X == objp.X && Y == objp.Y && Color == objp.Color && InPlay == objp.InPlay;
        }

        public override string ToString()
        {
            return String.Format("[{0},{1}] : {2}", X, Y, Color);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ InPlay.GetHashCode() ^ Color.GetHashCode();
        }
    }
}