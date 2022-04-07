using System.Collections.Generic;
using System.Diagnostics;
using PluginSDK;

namespace CellarAutomatonLib
{
    [DebuggerDisplay("S={" + nameof(State) + "}")]
    public class Cell : ICell
    {
        public int State { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Dictionary<string, double> Memory { get; set; }

        public Cell(int state, int x, int y, Dictionary<string, double> memory = null)
        {
            State = state;
            Memory = memory;
            X = x;
            Y = y;
        }

        public Cell GetCopy()
        {
            return Memory != null
                ? new Cell(State,X, Y, new Dictionary<string, double>(Memory))
                : new Cell(State, X, Y);
        }
    }
}