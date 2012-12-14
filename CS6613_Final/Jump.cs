// Mohammed Hossain 12/12/12

using System.Collections.Generic;
using System.Linq;

namespace CS6613_Final
{
    //Jump: an abstraction of a possible jump for a piece, with a list of locations jumped over and the final location the piece ends up at
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

            for (var i = 0; i < LocationsJumped.Count; i++)
            {
                var jr = LocationsJumped[i];
                clone.LocationsJumped.Add(new JumpResult(jr.JumpedLocation, jr.FinalLocation));
            }

            return clone;
        }
    }
}