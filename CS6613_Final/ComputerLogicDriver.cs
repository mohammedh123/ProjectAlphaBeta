using System;
using System.Linq;

namespace CS6613_Final
{
    class AlphaBetaReturnValue
    {
        public int Value { get; set; }
        public MoveResult Move { get; set; }
        public int Depth { get; set; }

        public AlphaBetaReturnValue(int value, MoveResult m, int maxDepth)
        {
            Value = value;
            Move = m;
            Depth = maxDepth;
        }
    }

    internal class ComputerLogicDriver : LogicDriver
    {
        //must be implemented by all logic driver's
        // return true if it is done finding its next move
        public override TurnResult GetNextMove(CheckersBoardGame game)
        {
            //var possibleMoves = board.Board.GetAllAvailableMoves(Color);

            //foreach (var move in possibleMoves)
            //{
            //    board.MovePiece(move.TypeOfMove, board.Board.GetPieceAtPosition(move.OriginalPieceLocation), move.FinalPieceLocation.X,
            //                   move.FinalPieceLocation.Y);

            //    return TurnResult.Finished;
            //}
            var maxDepth = 0;
            var move = AlphaBeta(game.Board, ref maxDepth);
            if (move != null)
            {
                var result = game.Board.MovePiece(move.TypeOfMove,
                                     game.Board.GetPieceAtPosition(move.OriginalPieceLocation.X,
                                                                   move.OriginalPieceLocation.Y),
                                     move.FinalPieceLocation.X, move.FinalPieceLocation.Y);

                return result;
            }

            return TurnResult.NotDone;
        }

        public MoveResult AlphaBeta(CheckersBoard game, ref int maxDepth)
        {
            var alpha = Int32.MinValue;
            var beta = Int32.MaxValue;
            maxDepth = 0;
            int currentDepth = 0;
            var v = MaxValue(game, ref alpha, ref beta, Color, ref currentDepth, ref maxDepth);
            Console.WriteLine("Optimal move found with depth {0}; max depth searched was {1}.", v.Depth, maxDepth);

            return v.Move;
        }

        public AlphaBetaReturnValue MaxValue(CheckersBoard board, ref int alphaValue, ref int betaValue, PieceColor color, ref int currentDepth, ref int maxDepth)
        {
            var result = board.GetGameResultState(color);
            
            var v = Int32.MinValue;
            var retVal = new AlphaBetaReturnValue(v, null, currentDepth + 1);
            var moves = board.GetAllAvailableMoves(color).ToList();

            if (AmIWinner(result) || !moves.Any())
                return new AlphaBetaReturnValue(Utility(result), null, currentDepth);

            var copyOfBoard = board.Clone();
            foreach(var m in moves)
            {
                var numJumps = moves.Count(mv => mv.TypeOfMove == MoveType.Jump);
                var resultingBoard = copyOfBoard.Clone();
                resultingBoard.MovePiece(m.TypeOfMove,
                                            resultingBoard.GetPieceAtPosition(m.OriginalPieceLocation.X,
                                                                              m.OriginalPieceLocation.Y),
                                            m.FinalPieceLocation.X, m.FinalPieceLocation.Y);

                var newDepth = currentDepth + 1;
                retVal = MinValue(resultingBoard, ref alphaValue, ref betaValue, color == PieceColor.Black ? PieceColor.Red : PieceColor.Black, ref newDepth, ref maxDepth);
                retVal.Move = m;
                
                if (retVal.Depth > maxDepth)
                    maxDepth = retVal.Depth;
                
                v = Math.Max(v, retVal.Value);

                if (v >= betaValue)
                {
                    //Console.WriteLine("Max pruned.");
                    return retVal;
                }
                alphaValue = Math.Max(alphaValue, v);
            }

            return retVal;
        }

        public AlphaBetaReturnValue MinValue(CheckersBoard board, ref int alphaValue, ref int betaValue, PieceColor color, ref int currentDepth, ref int maxDepth)
        {
            var result = board.GetGameResultState(color);

            var v = Int32.MaxValue;
            var retVal = new AlphaBetaReturnValue(v, null, currentDepth+1);
            var moves = board.GetAllAvailableMoves(color).ToList();

            if (AmIWinner(result) || !moves.Any())
                return new AlphaBetaReturnValue(Utility(result), null, currentDepth);

            var copyOfBoard = board.Clone();
            foreach (var m in moves)
            {
                var numJumps = moves.Count(mv => mv.TypeOfMove == MoveType.Jump);
                var resultingBoard = copyOfBoard.Clone();
                board.MovePiece(m.TypeOfMove,
                                            resultingBoard.GetPieceAtPosition(m.OriginalPieceLocation.X,
                                                                              m.OriginalPieceLocation.Y),
                                            m.FinalPieceLocation.X, m.FinalPieceLocation.Y);
                
                var newDepth = currentDepth + 1;
                retVal = MaxValue(resultingBoard, ref alphaValue, ref betaValue, color == PieceColor.Black ? PieceColor.Red : PieceColor.Black, ref newDepth, ref maxDepth);
                retVal.Move = m;

                if (retVal.Depth > maxDepth)
                    maxDepth = retVal.Depth;

                v = Math.Min(v, retVal.Value);

                if (v >= alphaValue)
                {
                    //Console.WriteLine("Max pruned.");
                    return retVal;
                }

                betaValue = Math.Min(betaValue, v);
            }

            return retVal;
        }

        public int Utility(GameResult result)
        {
            if(Color == PieceColor.Black)
            {
                if(result == GameResult.BlackWins)
                    return 1;
            }
            else if(Color == PieceColor.Red)
            {
                if(result == GameResult.RedWins)
                    return 1;
            }
            
            if(result == GameResult.Tie)
                return 0;

            return -1;
        }

        public bool AmIWinner(GameResult result)
        {
            if (Color == PieceColor.Black)
                return result == GameResult.BlackWins;
            if (Color == PieceColor.Red)
                return result == GameResult.RedWins;

            return false;
        }
    }
}