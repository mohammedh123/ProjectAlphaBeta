using System.Collections.Generic;

namespace CS6613_Final
{
    public class Jump
    {
        public Jump(Location finalLocation)
        {
            LocationsJumpedOver = new List<Location>();
            FinalLocation = finalLocation;
        }

        public Location FinalLocation { get; set; }
        public List<Location> LocationsJumpedOver { get; set; }
    }
}