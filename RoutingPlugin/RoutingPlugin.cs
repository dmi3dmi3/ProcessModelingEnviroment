using System.Collections.Generic;
using PluginSDK;

namespace RoutingPlugin
{
    public class Plugin : IPlugin
    {
        public string Name => "Routing";
        public List<ICommand> commands { get; }
    }
}