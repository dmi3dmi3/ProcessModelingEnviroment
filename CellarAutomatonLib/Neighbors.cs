using System.Linq;

namespace CellarAutomatonLib
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