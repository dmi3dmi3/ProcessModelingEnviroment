using System.Collections.Generic;
using System.Linq;
using PluginSDK;

namespace RoutingPlugin
{
    public class FindNextCommand : IProcessorCommand
    {
        public string Name => "FindNext";
        public void Execute(INeighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n, int x, int y)
        {
            memory["nextX"] = 0;
            memory["nextY"] = 0;
        }
    }
}