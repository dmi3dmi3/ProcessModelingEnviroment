using System.Collections.Generic;
using System.Diagnostics;

namespace PmeConsoleFramework
{
    [DebuggerDisplay("S={" + nameof(State) + "}")]
    public class Cell
    {
        public int State { get; set; }
        public Dictionary<string, int> Memory { get; set; }
        public Cell(int state, Dictionary<string, int> memory = null)
        {
            State = state;
            Memory = memory;
        }

        public Cell GetCopy()
        {
            return Memory != null
                ? new Cell(State, new Dictionary<string, int>(Memory))
                : new Cell(State);
        }
    }
}