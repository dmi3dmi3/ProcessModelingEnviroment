using System;
using System.Collections.Generic;
using RoutingPlugin.Models;

namespace RoutingPlugin
{
    public static class GlobalMemory
    {
        public static bool IsNotInitialized { get; set; } = true;
        public static Dictionary<Point, List<Point>> PackageLists { get; set; }
        public static Dictionary<Point, Dictionary<Point, int>> PrevRoutingTable { get; set; }
        public static Dictionary<Point, Dictionary<Point, int>> NextRoutingTable { get; set; }
        public static List<Tuple<Point, Point>> Routes { get; set; } 

        private static Random _random = new Random();

        public static void Init(int height, int width, double packageProbability)
        {
            IsNotInitialized = false;
            PackageLists = new Dictionary<Point, List<Point>>(height * width);
            PrevRoutingTable = new Dictionary<Point, Dictionary<Point, int>>(height * width);
            NextRoutingTable = new Dictionary<Point, Dictionary<Point, int>>(height * width);
            Routes = new List<Tuple<Point, Point>>();
            
            for (var x = 0; x < height; x++)
            {
                for (var y = 0; y < width; y++)
                {
                    var point = new Point(x, y);
                    PackageLists.Add(point, new List<Point>());
                    NextRoutingTable.Add(point, new Dictionary<Point, int> { { point, 0 } });
                    if (_random.NextDouble() < packageProbability)
                    {
                        var targetX = _random.Next(height);
                        var targetY = _random.Next(width);
                        PackageLists[point].Add(new Point(targetX, targetY));
                    }
                }
            }
        }
    }
}