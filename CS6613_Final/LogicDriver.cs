namespace CS6613_Final
{
    internal enum TurnResult
    {
        NotDone,
        Finished
    };

    internal abstract class LogicDriver
    {
        public virtual bool IsPlayer
        {
            get { return false; }
        }

        public PieceColor Color { get; set; }
        public abstract TurnResult GetNextMove(CheckersBoardGame board);
    }
}