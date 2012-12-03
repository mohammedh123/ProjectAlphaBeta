using System;

namespace CS6613_Final
{
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