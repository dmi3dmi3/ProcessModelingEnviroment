using System.Collections.Generic;
using System.Linq;
using PluginSDK;
using RoutingPlugin.Models;

namespace RoutingPlugin.Commands
{
    public class HasNoPackagesCommand : IStateChangeCommand
    {
        public string Name => "HasNoPackages";
        public bool Execute(INeighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n, int x, int y)
        {
            var point = new Point(x, y);
            var packageQueue = GlobalMemory.PackageLists[point];
            return packageQueue.All(p => !Equals(p, point));
        }
    }
}