using System;
using System.Collections.Generic;

namespace CS6613_Final
{
    internal class ConsoleDrawer : DisplayDriver
    {
        public override void Draw(Board board, List<CheckersPiece> playerOnePieces, List<CheckersPiece> playerTwoPieces, CheckersPiece selectedPiece = null)
        {
            Console.Clear();

            //if you want to write to console, fill the rest of this function out
        }
    }
}