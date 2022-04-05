using System;
using System.Collections.Generic;

namespace PluginSDK
{
    public interface ICell
    {
         int State { get; set; }
         Dictionary<string, double> Memory { get; set; }
         Dictionary<string, string> MemoryStr { get; set; }
    }
}