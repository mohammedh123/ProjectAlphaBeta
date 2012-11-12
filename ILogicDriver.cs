using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
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
        public abstract bool GetNextMove(Board board);
    }
}
