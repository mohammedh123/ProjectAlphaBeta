using System;
using System.Linq;
using System.Threading;

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
        public const int NegativeInfinity = -1000;
        public const int PositiveInfinity =  1000;

        private Thread _logicThread = null;

        public DateTime AlphaBetaStartTime { get; set; }

        private int NodesGenerated { get; set; }
        private int NumberOfMaxPrunes { get; set; }
        private int NumberOfMinPrunes { get; set; }

        private TurnResult _finalResult = TurnResult.NotDone;

        public ComputerLogicDriver()
        {
            NodesGenerated = 0;
            NumberOfMaxPrunes = 0;
            NumberOfMinPrunes = 0;
            AlphaBetaStartTime = new DateTime();
        }

        public void ResetCounts()
        {
            NodesGenerated = 0;
            NumberOfMaxPrunes = 0;
            NumberOfMinPrunes = 0;
            AlphaBetaStartTime = DateTime.Now;
        }

        //must be implemented by all logic driver's
        // return true if it is done finding its next move
        public override TurnResult GetNextMove(CheckersBoardGame game)
        {
            if (_logicThread == null)
            {
                _finalResult = TurnResult.NotDone;
                _logicThread = new Thread(() =>
                                              {
                                                  ResetCounts();

                                                  var maxDepth = 0;
                                                  var move = AlphaBeta(game.Board, ref maxDepth);
                                                  if (move != null)
                                                  {
                                                      var result = game.Board.MovePiece(move.TypeOfMove,
                                                                                        game.Board.GetPieceAtPosition(
                                                                                            move.OriginalPieceLocation.X,
                                                                                            move.OriginalPieceLocation.Y),
                                                                                        move.FinalPieceLocation.X,
                                                                                        move.FinalPieceLocation.Y);

                                                      _finalResult = result;
                                                  }
                                              });
                _logicThread.Start();
            }
            else
            {
                if(!_logicThread.IsAlive)
                {
                    _logicThread.Join();
                    _logicThread = null;
                }
            }

            return _finalResult;
        }

        public MoveResult AlphaBeta(CheckersBoard game, ref int maxDepth)
        {
            var alpha = NegativeInfinity;
            var beta = PositiveInfinity;
            maxDepth = 0;
            var currentDepth = 0;
            var v = MaxValue(game, ref alpha, ref beta, Color, ref currentDepth, ref maxDepth);
            Console.WriteLine("Optimal move found with depth {0}; max depth searched was {1}.", v.Depth, maxDepth);

            return v.Move;
        }

        public AlphaBetaReturnValue MaxValue(CheckersBoard board, ref int alphaValue, ref int betaValue, PieceColor color, ref int currentDepth, ref int maxDepth)
        {
            NodesGenerated++;
            if (NodesGenerated % 10000 == 0)
                Console.WriteLine("NodesGenerated: {0}, Max Prunes: {1}, Min Prunes: {2}, Total Time: {3}",
                                  NodesGenerated, NumberOfMaxPrunes, NumberOfMinPrunes,
                                  DateTime.Now - AlphaBetaStartTime);

            var result = board.GetGameResultState(color);
            
            var v = NegativeInfinity;
            var retVal = new AlphaBetaReturnValue(v, null, currentDepth + 1);
            var moves = board.GetAllAvailableMoves(color).ToList();

            if (AmIWinner(result) || !moves.Any())
            {
                retVal.Value = Utility(result);
                retVal.Depth = currentDepth;

                return retVal;
            }


            foreach(var m in moves)
            {
                var resultingBoard = board.Clone();
                
                var moveResult = resultingBoard.MovePiece(m.TypeOfMove,
                                            resultingBoard.GetPieceAtPosition(m.OriginalPieceLocation.X,
                                                                              m.OriginalPieceLocation.Y),
                                            m.FinalPieceLocation.X, m.FinalPieceLocation.Y);

                var newDepth = currentDepth;

                if(moveResult == TurnResult.NotDone)
                {
                    retVal = MaxValue(resultingBoard, ref alphaValue, ref betaValue, color, ref newDepth, ref maxDepth);
                }
                else if(moveResult == TurnResult.Finished)
                {
                    newDepth++; 
                    retVal = MinValue(resultingBoard, ref alphaValue, ref betaValue, color == PieceColor.Black ? PieceColor.Red : PieceColor.Black, ref newDepth, ref maxDepth);
                }
              
                retVal.Move = m;
                
                if (retVal.Depth > maxDepth)
                    maxDepth = retVal.Depth;
                
                v = Math.Max(v, retVal.Value);

                if (v >= betaValue)
                {
                    //Console.WriteLine("Max pruned.");
                    retVal.Value = v;
                    NumberOfMaxPrunes++;
                    return retVal;
                }

                alphaValue = Math.Max(alphaValue, v);
            }

            retVal.Value = v;
            return retVal;
        }

        public AlphaBetaReturnValue MinValue(CheckersBoard board, ref int alphaValue, ref int betaValue, PieceColor color, ref int currentDepth, ref int maxDepth)
        {
            NodesGenerated++;
            if (NodesGenerated % 10000 == 0)
                Console.WriteLine("NodesGenerated: {0}, Max Prunes: {1}, Min Prunes: {2}, Total Time: {3}", NodesGenerated, NumberOfMaxPrunes, NumberOfMinPrunes, DateTime.Now - AlphaBetaStartTime);

            var result = board.GetGameResultState(color);

            var v = PositiveInfinity;
            var retVal = new AlphaBetaReturnValue(v, null, currentDepth+1);
            var moves = board.GetAllAvailableMoves(color).ToList();

            if (AmIWinner(result) || !moves.Any())
            {
                retVal.Value = Utility(result);
                retVal.Depth = currentDepth;

                return retVal;
            }
            
            foreach (var m in moves)
            {
                var resultingBoard = board.Clone();
                var moveResult = resultingBoard.MovePiece(m.TypeOfMove,
                                            resultingBoard.GetPieceAtPosition(m.OriginalPieceLocation.X,
                                                                              m.OriginalPieceLocation.Y),
                                            m.FinalPieceLocation.X, m.FinalPieceLocation.Y);
                
                var newDepth = currentDepth;
                if(moveResult == TurnResult.NotDone)
                {
                    retVal = MinValue(resultingBoard, ref alphaValue, ref betaValue, color, ref newDepth, ref maxDepth);
                }
                else if(moveResult == TurnResult.Finished)
                {
                    newDepth++;
                    retVal = MaxValue(resultingBoard, ref alphaValue, ref betaValue, color == PieceColor.Black ? PieceColor.Red : PieceColor.Black, ref newDepth, ref maxDepth);
                }
                
                retVal.Move = m;

                if (retVal.Depth > maxDepth)
                    maxDepth = retVal.Depth;

                v = Math.Min(v, retVal.Value);

                if (v <= alphaValue)
                {
                    //Console.WriteLine("Min pruned.");
                    retVal.Value = v;
                    NumberOfMinPrunes++;
                    return retVal;
                }

                betaValue = Math.Min(betaValue, v);
            }

            retVal.Value = v;
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