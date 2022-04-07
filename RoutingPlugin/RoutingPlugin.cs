using System.Collections.Generic;
using PluginSDK;
using RoutingPlugin.Commands;

namespace RoutingPlugin
{
    public class Plugin : IPlugin
    {
        public string Name => "Routing";
        public List<IStateChangeCommand> StateChangeCommands=> new List<IStateChangeCommand>
        {
            new HasNoRoutesCommand(),
            new HasNoPackagesCommand(),
            new IsRoutingCommand(),
        };
        public List<IProcessorCommand> ProcessorCommands=> new List<IProcessorCommand>
        {
            new CollectRouteTableCommand(),
            new MovePackagesCommand(),
        };
    }
}