using System.Collections.Generic;
using System.Linq;
using PluginSDK;

namespace RoutingPlugin
{
    public class FindNextCommand : ICommand
    {
        public string Name => "FindNext";
        public void Execute(ICell cell, INeighbors neighbors)
        {
            foreach (var neighbor in neighbors.NeighborsCell.Where(c => c != null && c.State == 1))
            {
                var next = neighbor.MemoryStr["next"];
                var x = int.Parse(next.Split(':')[0]);
                var y = int.Parse(next.Split(':')[1]);
            }
        }
    }
}