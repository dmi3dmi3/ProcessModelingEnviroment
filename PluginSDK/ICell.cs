using System;
using System.Collections.Generic;

namespace PluginSDK
{
    public interface ICell
    {
        int State { get; set; }
        int X { get; set; }
        int Y { get; set; }
        Dictionary<string, double> Memory { get; set; }
    }
}