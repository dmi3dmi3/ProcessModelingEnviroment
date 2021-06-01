using System.Linq;

namespace PmeConsoleFramework
{
    public class Neighbors
    {
        public Cell[] NeighborsCell { get; set; }
        public int StateCount(int state)
        {
            return NeighborsCell.Count(_ => _.State == state);
        }
    }
}