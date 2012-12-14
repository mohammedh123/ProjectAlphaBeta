// Mohammed Hossain 12/12/12

using System;

namespace CS6613_Final
{
    //a value-type to represent a SINGLE jump (not multiple); contains the jumped location and the final location of the jump
    public struct JumpResult
    {
        public Location JumpedLocation;
        public Location FinalLocation;
    
        public JumpResult(Location jumpedLoc, Location finalLoc)
        {
            JumpedLocation = jumpedLoc;
            FinalLocation = finalLoc;
        }

        public override string ToString()
        {
            return String.Format("{0}, jumped over {1}.", FinalLocation, JumpedLocation);
        }
    }
}