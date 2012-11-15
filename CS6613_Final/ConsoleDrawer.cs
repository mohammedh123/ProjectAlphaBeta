using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    class ConsoleDrawer : IDisplayDriver
    {
        public override void Draw(Board board, IEnumerable<CheckersPiece> pieces, CheckersPiece selectedPiece)
        {
            System.Console.Clear();

            //if you want to write to console, fill the rest of this function out
        }
        
        //no concept of a ghost piece [from dragging the tile] on the console, do not implement this
    }
}
