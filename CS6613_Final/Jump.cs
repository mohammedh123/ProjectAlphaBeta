using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS6613_Final
{
    public class Jump
    {
        public Location FinalLocation { get; set; }
        public List<Location> LocationsJumpedOver { get; set; }

        public Jump(Location finalLocation)
        {
            LocationsJumpedOver = new List<Location>();
            FinalLocation = finalLocation;
        }
    }
}
