using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    class AILogicDriver : ILogicDriver
    {
        //must be implemented by all logic driver's
        // return true if it is done finding its next move
        public override TurnResult GetNextMove(CheckersBoardGame game)
        {
        }

        public int AlphaBeta(CheckersBoardGame game, int alpha, int beta)
        {
            if (game.IsGameOver())
                return 1; //TODO: FIX THIS CHECK/RET VAL
            
            var possibleMoves = game.GetAllAvailableMoves()
        }
    }
}
