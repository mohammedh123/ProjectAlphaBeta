using System.Collections.Generic;

namespace CS6613_Final
{
    internal class ComputerLogicDriver : LogicDriver
    {
        //must be implemented by all logic driver's
        // return true if it is done finding its next move
        public override TurnResult GetNextMove(CheckersBoardGame game)
        {
            return TurnResult.NotDone;
        }

        public int AlphaBeta(CheckersBoardGame game, int alpha, int beta)
        {
            if (game.IsGameOver())
                return 1; //TODO: FIX THIS CHECK/RET VAL

            var possibleMoves = game.Board.GetAllAvailableMoves(Color);



            return 0;
        }
    }
}