using System.Collections.Generic;
using System.Linq;
using PluginSDK;
using RoutingPlugin.Models;

namespace RoutingPlugin.Commands
{
    public class HasNoRoutesCommand : IStateChangeCommand
    {
        public string Name => "HasNoRoutes";

        public bool Execute(INeighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global,
            int n, int x, int y)
        {
            var point = new Point(x, y);
            var packageQueue = GlobalMemory.PackageLists[point];
            var points = neighbors.NeighborsCell
                .Where(c => c != null)
                .Select(c => new Point(c.X, c.Y))
                .ToList();

            var queueCopy = packageQueue.ToHashSet();
            foreach (var neighbor in points)
            foreach (var target in GlobalMemory.PrevRoutingTable[neighbor].Keys)
                if (queueCopy.Contains(target))
                    return false;
            return true;
        }
    }
}