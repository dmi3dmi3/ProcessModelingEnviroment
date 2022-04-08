using System;
using System.Collections.Generic;
using System.Linq;
using PluginSDK;
using RoutingPlugin.Models;

namespace RoutingPlugin.Commands
{
    public class IsRoutingCommand : IStateChangeCommand
    {
        public string Name => "IsRouting";

        public bool Execute(INeighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global,
            int n, int x, int y)
        {
            var point = new Point(x, y);
            var packageQueue = GlobalMemory.PackageLists[point];
            var points = neighbors.NeighborsCell
                .Where(c => c != null)
                .Select(c => new Point(c.X, c.Y))
                .ToList();

            var neighborsRoutingTable = points
                .ToDictionary(p => p, p => GlobalMemory.PrevRoutingTable[p]);

            Point FindPackage()
            {
                foreach (var neighbor in points)
                foreach (var package in packageQueue)
                    if (neighborsRoutingTable[neighbor].ContainsKey(package))
                        return package;
                throw new ApplicationException("Wrong command execution order");
            }

            var packageToRoute = FindPackage();
            var routes = neighborsRoutingTable
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value.TryGetValue(packageToRoute, out var value) ? value : -1)
                .Where(pair => pair.Value != -1)
                .ToList();
            Point next;
            switch (routes.Count)
            {
                case 0: throw new ApplicationException("Wrong command execution order");
                case 1:
                    next = routes[0].Key;
                    break;
                default:
                    var min = routes.Min(pair => pair.Value);
                    next = routes.First(pair => pair.Value == min).Key;
                    break;
            }

            GlobalMemory.Routes.Add(new Tuple<Point, Point>(next, packageToRoute));
            packageQueue.Remove(packageToRoute);
            return true;
        }
    }
}