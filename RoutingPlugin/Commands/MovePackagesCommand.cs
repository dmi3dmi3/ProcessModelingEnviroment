using System.Collections.Generic;
using System.Linq;
using PluginSDK;
using RoutingPlugin.Models;

namespace RoutingPlugin.Commands
{
    public class MovePackagesCommand : IProcessorCommand
    {
        public string Name => "MovePackages";

        public void Execute(INeighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global,
            int n, int x, int y)
        {
            var point = new Point(x, y);
            var route = GlobalMemory.Routes.FirstOrDefault(tuple => tuple.Item1.Equals(point));
            if (route != null)
            {
                GlobalMemory.PackageLists[point].Add(route.Item2);
                GlobalMemory.Routes.Remove(route);
            }
        }
    }
}