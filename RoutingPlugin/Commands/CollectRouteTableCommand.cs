using System;
using System.Collections.Generic;
using System.Linq;
using PluginSDK;
using RoutingPlugin.Models;

namespace RoutingPlugin.Commands
{
    public class CollectRouteTableCommand : IProcessorCommand
    {
        public string Name => "CollectRouteTable";
        private static int _prevN = -1;

        public void Execute(INeighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global,
            int n, int x, int y)
        {
            var count = (int)global["height"] * (int)global["width"];
            if (GlobalMemory.IsNotInitialized)
                GlobalMemory.Init((int)global["height"], (int)global["width"], global["packageProbability"]);

            if (_prevN != n)
            {
                GlobalMemory.PrevRoutingTable = GlobalMemory.NextRoutingTable;
                GlobalMemory.NextRoutingTable =
                    new Dictionary<Point, Dictionary<Point, int>>(count);
                _prevN = n;
            }

            var point = new Point(x, y);
            var queueSize = GlobalMemory.PackageLists[point].Count;
            GlobalMemory.NextRoutingTable.Add(point, new Dictionary<Point, int>(count) { { point, 0 } });

            // getting routing table from neighbors
            var points = neighbors.NeighborsCell
                .Where(c => c != null)
                .Select(c => new Point(c.X, c.Y))
                .ToList();
            if (!points.Any())
                return;

            var neighborsRoutingTables = points
                .Select(p => GlobalMemory.PrevRoutingTable[p])
                .ToList();
            var unifiedRoutingTable = new Dictionary<Point, int>(neighborsRoutingTables.First().Count);
            foreach (var neighborsRoutingTable in neighborsRoutingTables)
            {
                var keys = neighborsRoutingTable.Keys;
                foreach (var key in keys)
                {
                    var newValue = neighborsRoutingTable[key];
                    if (unifiedRoutingTable.TryGetValue(key, out var oldValue))
                    {
                        if (oldValue > newValue) unifiedRoutingTable[key] = newValue;
                    }
                    else
                    {
                        unifiedRoutingTable.Add(key, newValue);
                    }
                }
            }

            var newRoutingTable = GlobalMemory.NextRoutingTable[point];
            var unifiedRoutingTableKeys = unifiedRoutingTable.Keys;
            foreach (var key in unifiedRoutingTableKeys)
            {
                var newPathValue = unifiedRoutingTable[key] + queueSize + 1;
                if (newRoutingTable.TryGetValue(key, out var value))
                {
                    if (newPathValue < value)
                        newRoutingTable[key] = newPathValue;
                }
                else
                {
                    newRoutingTable.Add(key, newPathValue);
                }
            }
        }
    }
}