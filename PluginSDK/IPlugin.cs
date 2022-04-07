using System;
using System.Collections.Generic;

namespace PluginSDK
{
    public interface IPlugin
    {
        string Name { get; }
        List<IStateChangeCommand> StateChangeCommands { get; }
        List<IProcessorCommand> ProcessorCommands { get; }
    }
}