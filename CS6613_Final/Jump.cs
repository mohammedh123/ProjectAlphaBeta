using System.Collections.Generic;
using System.Linq;

namespace CS6613_Final
{
    public class Jump
    {
        public Location FinalLocation { get { return LocationsJumped.Last().FinalLocation; } }
        public List<JumpResult> LocationsJumped { get; set; }
 
        public Jump()
        {
            LocationsJumped = new List<JumpResult>();
        }

        public void AddJumpedLocation(JumpResult jr)
        {
            LocationsJumped.Add(jr);
        }

        public Jump Clone()
        {
            var clone = new Jump();

            for (int i = 0; i < LocationsJumped.Count; i++)
            {
                var jr = LocationsJumped[i];
                clone.LocationsJumped.Add(new JumpResult(jr.JumpedLocation, jr.FinalLocation));
            }

            return clone;
        }
    }
}