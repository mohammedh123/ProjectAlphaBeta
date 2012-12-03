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
        public const int NegativeInfinity = -500;
        public const int PositiveInfinity =  500;

        private Thread _logicThread;

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
                                                      var result = game.Board.MovePiece(move,
                                                                                        game.Board.GetPieceAtPosition(
                                                                                            move.OriginalPieceLocation.X,
                                                                                            move.OriginalPieceLocation.Y));

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
            const int alpha = NegativeInfinity;
            const int beta = PositiveInfinity;
            maxDepth = 0;
            var currentDepth = 0;
            var v = MaxValue(game, alpha, beta, Color, ref currentDepth, ref maxDepth);
            Console.WriteLine("Optimal move found with depth {0}; max depth searched was {1}; time to find move: {2}; [{3}, {4}]", v.Depth, maxDepth, DateTime.Now - AlphaBetaStartTime, alpha, beta);

            return v.Move;
        }

        public AlphaBetaReturnValue MaxValue(CheckersBoard board, int alphaValue, int betaValue, PieceColor color, ref int currentDepth, ref int maxDepth)
        {
            NodesGenerated++;
            if (NodesGenerated % 10000 == 0)
                Console.WriteLine("NodesGenerated: {0}, Max Prunes: {1}, Min Prunes: {2}, Total Time: {3}, [{4}, {5}]",
                                  NodesGenerated, NumberOfMaxPrunes, NumberOfMinPrunes,
                                  DateTime.Now - AlphaBetaStartTime, alphaValue, betaValue);

            //var result = board.GetGameResultState(color);
            
            var v = NegativeInfinity;
            var retVal = new AlphaBetaReturnValue(v, null, currentDepth + 1);
            var moves = board.GetAllAvailableMoves(color).OrderByDescending(mr => mr.JumpResults.Count()).ToList();
            var numJumps = moves.Count(mr => mr.Type == MoveType.Jump) >= 2;

            if (CutoffTest(board, currentDepth) || !moves.Any())
            {
                retVal.Value = Evaluate(board);
                retVal.Depth = currentDepth;

                return retVal;
            }


            foreach(var m in moves)
            {
                var resultingBoard = board.Clone();

                resultingBoard.MovePiece(m,
                                         resultingBoard.GetPieceAtPosition(m.OriginalPieceLocation.X,
                                                                           m.OriginalPieceLocation.Y));

                if (currentDepth == 0 && moves.Count == 1)
                {
                    retVal.Move = m;
                    retVal.Value = Evaluate(board);

                    return retVal;
                }

                var newDepth = currentDepth;

                newDepth++; 
                retVal = MinValue(resultingBoard, alphaValue, betaValue, color == PieceColor.Black ? PieceColor.Red : PieceColor.Black, ref newDepth, ref maxDepth);
              
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

        public AlphaBetaReturnValue MinValue(CheckersBoard board, int alphaValue, int betaValue, PieceColor color, ref int currentDepth, ref int maxDepth)
        {
            NodesGenerated++;
            if (NodesGenerated % 10000 == 0)
                Console.WriteLine("NodesGenerated: {0}, Max Prunes: {1}, Min Prunes: {2}, Total Time: {3}, [{4}, {5}]",
                                  NodesGenerated, NumberOfMaxPrunes, NumberOfMinPrunes,
                                  DateTime.Now - AlphaBetaStartTime, alphaValue, betaValue);

            //var result = board.GetGameResultState(color);

            var v = PositiveInfinity;
            var retVal = new AlphaBetaReturnValue(v, null, currentDepth+1);
            var moves = board.GetAllAvailableMoves(color).OrderByDescending(mr => mr.JumpResults.Count()).ToList();
            var numJumps = moves.Count(mr => mr.Type == MoveType.Jump) >= 2;

            if (CutoffTest(board, currentDepth) || !moves.Any())
            {
                retVal.Value = Evaluate(board);
                retVal.Depth = currentDepth;

                return retVal;
            }

            foreach (var m in moves)
            {
                var resultingBoard = board.Clone();
                resultingBoard.MovePiece(m,
                                         resultingBoard.GetPieceAtPosition(m.OriginalPieceLocation.X,
                                                                           m.OriginalPieceLocation.Y));

                if(currentDepth == 0 && moves.Count == 1)
                {
                    retVal.Move = m;
                    retVal.Value = Evaluate(board);

                    return retVal;
                }

                var newDepth = currentDepth;

                newDepth++;
                retVal = MaxValue(resultingBoard, alphaValue, betaValue, color == PieceColor.Black ? PieceColor.Red : PieceColor.Black, ref newDepth, ref maxDepth);
                
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

        public int Evaluate(CheckersBoard state)
        {
            var pieces = state.InPlayPieces;
            const int basePieceValue = 5;
            
            var ourTotalPieceValue = 0;
            var theirTotalPieceValue = 0;

            foreach (var piece in pieces)
            {
                var pieceRow = (int)(state.TileBoard.Height * 0.5 - Math.Abs(state.TileBoard.Height * 0.5 - piece.Y));
                var pieceValue = basePieceValue + pieceRow;

                if (piece.Color == Color)
                    ourTotalPieceValue += pieceValue;
                else
                    theirTotalPieceValue += pieceValue;
            }
            
            return ourTotalPieceValue - theirTotalPieceValue;
        }

        public bool CutoffTest(CheckersBoard state, int depth)
        {
            if (depth == 12)
                return true;

            var result = state.GetGameResultState(Color);
            if (Color == PieceColor.Black)
                return result == GameResult.BlackWins;
            if (Color == PieceColor.Red)
                return result == GameResult.RedWins;

            return false;
        }
    }
}