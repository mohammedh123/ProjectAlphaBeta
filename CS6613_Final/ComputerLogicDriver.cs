using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace CS6613_Final
{
    public enum ComputerDifficulty
    {
        Easy = 2,
        Normal = 10,
        Hard = 18,
        VeryHard = Int32.MaxValue
    }

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
        private static readonly Dictionary<ComputerDifficulty, int> BaseDepthMap = new Dictionary<ComputerDifficulty, int>()
                                                                               {
                                                                                   {ComputerDifficulty.Easy, 1},
                                                                                   {ComputerDifficulty.Normal, 5},
                                                                                   {ComputerDifficulty.Hard, 10},
                                                                                   {ComputerDifficulty.VeryHard, 12}
                                                                               };
        public const int NegativeInfinity = -500;
        public const int PositiveInfinity =  500;

        private Thread _logicThread;                                                                                       

        public DateTime AlphaBetaStartTime { get; set; }

        private int NodesGenerated { get; set; }
        private int NumberOfMaxPrunes { get; set; }
        private int NumberOfMinPrunes { get; set; }

        private readonly ComputerDifficulty _difficultyLevel;
        private TurnResult _finalResult = TurnResult.NotDone;
        private int _numOfTurns = 0;
        private readonly int _baseDepth;
        private int _lastSuccessfulDepth = 1;
        private bool _depthCutoffHit = false;

        public ComputerLogicDriver(ComputerDifficulty difficulty)
        {
            _difficultyLevel = difficulty;
            _baseDepth = BaseDepthMap[difficulty];
            _lastSuccessfulDepth = _baseDepth;

            NodesGenerated = 0;
            NumberOfMaxPrunes = 0;
            NumberOfMinPrunes = 0;
            AlphaBetaStartTime = new DateTime();
            _depthCutoffHit = false;
        }

        public void ResetCounts()
        {
            NodesGenerated = 0;
            NumberOfMaxPrunes = 0;
            NumberOfMinPrunes = 0;
            _depthCutoffHit = false;
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
                                                  AlphaBetaStartTime = DateTime.Now;

                                                  var maxDepth = 0;
                                                  var move = AlphaBeta(game.Board, ref maxDepth);
                                                  if (move != null)
                                                  {
                                                      var result = game.Board.MovePiece(move, Color);

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
                    _numOfTurns++;
                }
            }

            return _finalResult;
        }

        public MoveResult AlphaBeta(CheckersBoard game, ref int maxDepth)
        {
            int iterativeDepth = _lastSuccessfulDepth;
            const int alpha = NegativeInfinity;
            const int beta = PositiveInfinity;

            int totalNodesGenerated = 0, totalMaxPrunes = 0, totalMinPrunes = 0;

            AlphaBetaReturnValue lastCompletedMove = null;
            while (iterativeDepth <= (int)_difficultyLevel)
            {
                ResetCounts();

                maxDepth = 0;
                var currentDepth = 0;
                var copy = game.Clone();

                var v = MaxValue(copy, alpha, beta, Color, ref currentDepth, ref maxDepth, iterativeDepth);

                if (v != null)
                {
                    lastCompletedMove = v;

                    Console.WriteLine(
                        "\tSearch at depth {0} completed. Optimal move found with value {1}; time to find move: {2}.\n" +
                        "\tNodes generated: {3}, max prunes: {4}, min prunes: {5}.\n",
                        maxDepth, lastCompletedMove.Value, DateTime.Now - AlphaBetaStartTime,
                        NodesGenerated, NumberOfMaxPrunes, NumberOfMinPrunes);

                    totalNodesGenerated += NodesGenerated;
                    totalMaxPrunes += NumberOfMaxPrunes;
                    totalMinPrunes += NumberOfMinPrunes;
                }
                else break;

                if (maxDepth == 0 || !_depthCutoffHit || iterativeDepth == (int)_difficultyLevel)
                    break;

                if(iterativeDepth != (int)_difficultyLevel)
                    iterativeDepth++;
            }

            _lastSuccessfulDepth = iterativeDepth;

            Console.WriteLine(
                "IDS ended. Optimal move found with value {0}; IDS stopped at depth {1}; time to find move: {2}.\n" +
                "Total completed IDS stats: \n\tNodes generated: {3}. \n\tMax prunes: {4}. \n\tMin prunes: {5}.\n",
                lastCompletedMove.Value, iterativeDepth, DateTime.Now - AlphaBetaStartTime,
                totalNodesGenerated, totalMaxPrunes, totalMinPrunes);
            return lastCompletedMove.Move;
        }

        public AlphaBetaReturnValue MaxValue(CheckersBoard board, int alphaValue, int betaValue, PieceColor color, ref int currentDepth, ref int maxDepth, int maxDepthToSearchFor)
        {
            NodesGenerated++;

            //var result = board.GetGameResultState(color);

            var v = new AlphaBetaReturnValue(NegativeInfinity, null, currentDepth + 1);

            var moves = board.GetAllAvailableMoves(color).ToList().OrderByDescending(mr => mr.JumpResults.Count()).ToList();
            //var numJumps = moves.Count(mr => mr.Type == MoveType.Jump) >= 2;

            var coTest = CutoffTest(board, currentDepth, maxDepthToSearchFor);
            if (coTest.HasValue && coTest.Value || !moves.Any())
            {
                v.Value = Evaluate(board, color);
                v.Depth = currentDepth;

                return v;
            }
            else if (!coTest.HasValue)
            {
                return null;
            }

            for (int i = 0; i < moves.Count; i++)
            {
                var m = moves[i];
                board.MovePiece(m, color);

                if (currentDepth == 0 && moves.Count == 1)
                {
                    v.Move = m;
                    v.Value = Evaluate(board, color);

                    board.RevertMove(m, color);
                    return v;
                }

                var newDepth = currentDepth;

                newDepth++;
                var retVal = MinValue(board, alphaValue, betaValue,
                                  color == PieceColor.Black ? PieceColor.Red : PieceColor.Black, ref newDepth,
                                  ref maxDepth, maxDepthToSearchFor);

                if (retVal == null)
                    return null;

                retVal.Move = m;

                board.RevertMove(m, color);

                if (retVal.Depth > maxDepth)
                    maxDepth = retVal.Depth;

                //max
                if (retVal.Value > v.Value)
                {
                    v.Value = retVal.Value;
                    v.Move = retVal.Move;
                }

                if (v.Value >= betaValue)
                {
                    //Console.WriteLine("Max pruned.");
                    NumberOfMaxPrunes++;
                    return v;
                }

                alphaValue = Math.Max(alphaValue, v.Value);
            }
            return v;
        }

        public AlphaBetaReturnValue MinValue(CheckersBoard board, int alphaValue, int betaValue, PieceColor color, ref int currentDepth, ref int maxDepth, int maxDepthToSearchFor)
        {
            NodesGenerated++;

            //var result = board.GetGameResultState(color);

            var v = new AlphaBetaReturnValue(PositiveInfinity, null, currentDepth + 1);
            var moves = board.GetAllAvailableMoves(color).ToList().OrderByDescending(mr => mr.JumpResults.Count()).ToList();
            //var numJumps = moves.Count(mr => mr.Type == MoveType.Jump) >= 2;

            var coTest = CutoffTest(board, currentDepth, maxDepthToSearchFor);
            if (coTest.HasValue && coTest.Value || !moves.Any())
            {
                v.Value = Evaluate(board, color);
                v.Depth = currentDepth;

                return v;
            }
            else if (!coTest.HasValue)
            {
                return null;
            }

            for (int i = 0; i < moves.Count; i++)
            {
                var m = moves[i];
                board.MovePiece(m, color);

                if (currentDepth == 0 && moves.Count == 1)
                {
                    v.Move = m;
                    v.Value = Evaluate(board, color);

                    board.RevertMove(m, color);
                    return v;
                }

                var newDepth = currentDepth;

                newDepth++;
                var retVal = MaxValue(board, alphaValue, betaValue,
                                  color == PieceColor.Black ? PieceColor.Red : PieceColor.Black, ref newDepth,
                                  ref maxDepth, maxDepthToSearchFor);

                if (retVal == null)
                    return null;

                retVal.Move = m;

                board.RevertMove(m, color);

                if (retVal.Depth > maxDepth)
                    maxDepth = retVal.Depth;

                if(retVal.Value < v.Value)
                {
                    v.Value = retVal.Value;
                    v.Move = retVal.Move;
                }

                if (v.Value <= alphaValue)
                {
                    //Console.WriteLine("Min pruned.");
                    NumberOfMinPrunes++;
                    return retVal;
                }

                betaValue = Math.Min(betaValue, v.Value);
            }

            return v;
        }

        public int Evaluate(CheckersBoard state, PieceColor color)
        {
            int pOne = 0, pTwo = 0;

            
            //var pOnePieces = state.PlayerOnePieces.Where(cp => cp.InPlay);
            //var pTwoPieces = state.PlayerTwoPieces.Where(cp => cp.InPlay);
            
            //var pOneRow = 1;
            //var pTwoRow = state.TileBoard.Height - 2 - pOneRow;

            for (int i = 0; i < state.Pieces.AlivePlayerOnePieces.Count; i++)
            {
                var piece = state.Pieces.AlivePlayerOnePieces[i];
                pOne += 3*(state.TileBoard.Height >> 1 - Math.Abs(state.TileBoard.Height >> 1 - piece.Y)) +
                        3*(state.TileBoard.Width >> 1 - Math.Abs(state.TileBoard.Width >> 1 - piece.X));
            }
            for (int i = 0; i < state.Pieces.AlivePlayerTwoPieces.Count; i++)
            {
                var piece = state.Pieces.AlivePlayerTwoPieces[i];
                pTwo += 3*(state.TileBoard.Height >> 1 - Math.Abs(state.TileBoard.Height >> 1 - piece.Y)) +
                        3*(state.TileBoard.Width >> 1 - Math.Abs(state.TileBoard.Width >> 1 - piece.X));
            }

            //pOne = state.PlayerOnePieces.Count();
            //pTwo = state.PlayerTwoPieces.Count();

            //pOne += state.Pieces.AlivePlayerOnePieces.Sum(piece => 5);
            //pTwo += state.Pieces.AlivePlayerTwoPieces.Sum(piece => 5);
            pOne += state.GetAllAvailableMoves(PieceColor.Black).Count()*10;
            pTwo += state.GetAllAvailableMoves(PieceColor.Red).Count()*10;

            //pOne += state.PlayerOnePieces.Count(p => p.InPlay) * 5;
            //pTwo += state.PlayerTwoPieces.Count(p => p.InPlay) * 5;

            return (Color == PieceColor.Black ? pOne - pTwo : pTwo - pOne);
        }

        public bool? CutoffTest(CheckersBoard state, int depth, int maxDepthToSearchFor)
        {
            if ((int)(DateTime.Now - AlphaBetaStartTime).TotalSeconds >= 60)
                return null;

            if (depth == maxDepthToSearchFor)
            {
                _depthCutoffHit = true;
                return true;
            }

            var result = state.GetGameResultState(Color);
            if (Color == PieceColor.Black)
                return result == GameResult.BlackWins;
            if (Color == PieceColor.Red)
                return result == GameResult.RedWins;

            return false;
        }
    }
}