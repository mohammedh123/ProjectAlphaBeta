using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    enum TurnResult
    {
        NotDone,
        Finished
    };

    abstract class ILogicDriver
    {
        public virtual bool IsPlayer
        {
            get
            {
                return false;
            }
        }

        public PieceColor Color { get; set; }
        public abstract TurnResult GetNextMove(CheckersBoardGame board);
    }
}
