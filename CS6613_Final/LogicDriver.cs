// Mohammed Hossain 12/12/12

namespace CS6613_Final
{
    //TurnResult: the result of a move
    internal enum TurnResult
    {
        NotDone,
        Finished
    };

    //LogicDriver: an abstract class that acts upon a CheckersGameDriver and moves a piece, and returns if it is done with that move or not
    internal abstract class LogicDriver
    {
        public virtual bool IsPlayer
        {
            get { return false; }
        }

        public PieceColor Color { get; set; }
        public abstract TurnResult GetNextMove(CheckersGameDriver gameDriver);
    }
}