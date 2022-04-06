using System.Collections.Generic;
using PluginSDK;

namespace RoutingPlugin
{
    public class RouteNextCommand : IStateChangeCommand
    {
        public string Name => "RouteNext";
        public bool Execute(INeighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n, int x, int y)
        {
            throw new System.NotImplementedException();
        }
    }
}