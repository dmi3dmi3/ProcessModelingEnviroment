using System;
using System.Collections.Generic;
using ProcessorType = System.Action<PluginSDK.INeighbors, System.Collections.Generic.Dictionary<string, double>, System.Collections.Generic.Dictionary<string, double>, int, int, int>;

namespace PluginSDK
{
    public interface IProcessorCommand : ICommand
    {
        void Execute(
            INeighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, 
            int n , int x, int y
            );
    }
}