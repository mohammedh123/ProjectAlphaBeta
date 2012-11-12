using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace CS6613_Final
{
    class PlayerLogicDriver : ILogicDriver
    {
        public override bool IsPlayer
        {
            get
            {
                return true;
            }
        }

        public override bool GetNextMove(Board board)
        {
            return true;
        }
    }
}
