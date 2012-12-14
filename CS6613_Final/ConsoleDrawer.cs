// Mohammed Hossain 12/12/12

using System;
using System.Collections.Generic;

namespace CS6613_Final
{
    //a blank template for an implementation of displayDriver that outputs to the console, but i did not feel it was necessary with a full implementation of a GUI drawer
    internal class ConsoleDrawer : DisplayDriver
    {
        public override void Draw(Board board, List<CheckersPiece> playerOnePieces, List<CheckersPiece> playerTwoPieces, List<MoveResult> availableMoves, CheckersPiece selectedPiece = null)
        {
            Console.Clear();

            //if you want to write to console, fill the rest of this function out
        }
    }
}