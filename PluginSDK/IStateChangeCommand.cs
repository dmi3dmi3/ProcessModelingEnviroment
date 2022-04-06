using System.Collections.Generic;

namespace PluginSDK
{
    public interface IStateChangeCommand: ICommand
    {
        bool Execute(
            INeighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global,
            int n , int x, int y
            );
    }
}