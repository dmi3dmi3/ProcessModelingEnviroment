using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PluginSDK;

namespace CellarAutomatonLib
{
    public static class PluginManager
    {
        public static Dictionary<string, IStateChangeCommand> StateChangeCommands { get; set; }
        public static Dictionary<string, IProcessorCommand> ProcessorCommands { get; set; }
        private static string _pluginHome = Path.Combine(Environment.CurrentDirectory, "plugins");

        public static void Init()
        {
            StateChangeCommands = new Dictionary<string, IStateChangeCommand>();
            ProcessorCommands = new Dictionary<string, IProcessorCommand>();

            var libs = new DirectoryInfo(_pluginHome);
            if (!libs.Exists)
            {
                libs.Create();
                return;
            }

            var libsFolder = libs.GetDirectories();
            if (libsFolder.Length == 0)
                return;

            var plugins = libsFolder
                .SelectMany(directory => directory.GetFiles("*.dll"))
                .Select(dll => Assembly.Load(AssemblyName.GetAssemblyName(dll.FullName)))
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(type => type.IsClass && typeof(IPlugin).IsAssignableFrom(type))
                .Select(type => Activator.CreateInstance(type) as IPlugin)
                .Where(plugin => plugin != null)
                .ToList();
            foreach (var plugin in plugins)
            {
                foreach (var processorCommand in plugin.ProcessorCommands)
                    ProcessorCommands.Add($"{plugin.Name}.{processorCommand.Name}", processorCommand);

                foreach (var stateChangeCommand in plugin.StateChangeCommands)
                    StateChangeCommands.Add($"{plugin.Name}.{stateChangeCommand.Name}", stateChangeCommand);
            }
        }
    }
}