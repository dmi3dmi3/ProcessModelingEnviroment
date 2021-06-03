using System.Collections.Generic;
using System.Diagnostics;

namespace CellarAutomatonLib
{
    [DebuggerDisplay("S={" + nameof(State) + "}")]
    public class Cell
    {
        public int State { get; set; }
        public Dictionary<string, double> Memory { get; set; }
        public Cell(int state, Dictionary<string, double> memory = null)
        {
            State = state;
            Memory = memory;
        }

        public Cell GetCopy()
        {
            return Memory != null
                ? new Cell(State, new Dictionary<string, double>(Memory))
                : new Cell(State);
        }
    }
}