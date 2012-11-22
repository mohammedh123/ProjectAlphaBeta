using System.Collections.Generic;

namespace CS6613_Final
{
    public class Jump
    {
        public Jump(Location finalLocation)
        {
            FinalLocation = finalLocation;
        }

        public Location FinalLocation { get; set; }
    }
}