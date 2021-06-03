using CellarAutomatonLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PmeGeneratingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            const string projectName = "project2";
            var configPath = Path.Combine(
                Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName,
                @"TestData\" + projectName + ".cfg");
            var text = File.ReadAllText(configPath);
            var config = Config.Deserialize(text);
            var ca = new CellarAutomaton(config);

            if (config.Paths == null)
                config.Paths = new Dictionary<string, string>();

            var caLogPath = Path.Combine(Directory.GetParent(configPath).FullName, projectName + ".ca");
            if (config.Paths.ContainsKey(Config.CaLogName))
                config.Paths[Config.CaLogName] = caLogPath;
            else
                config.Paths.Add(Config.CaLogName, caLogPath);
            File.Create(caLogPath).Close();

            var graphPath = Path.Combine(Directory.GetParent(configPath).FullName, projectName + ".sg");
            if (config.Paths.ContainsKey(Config.StateGraphsName))
                config.Paths[Config.StateGraphsName] = graphPath;
            else
                config.Paths.Add(Config.StateGraphsName, graphPath);
            File.Create(graphPath).Close();

            const int writeBufferLen = 100;
            var writeBuffer = new string[writeBufferLen];
            var i = 0;

            ca.Init();
            writeBuffer[i++] = ca.GetSerializedBoard();
            var cellCount = config.Width * config.Height;
            var stateGraphs = ca.GetStatesCount().ToDictionary(_ => _.Key, _ => new List<double>(config.StepCount) { (double)_.Value / cellCount });
            while (ca.Step < ca.Config.StepCount)
            {
                ca.CalculateNext();
                writeBuffer[i++] = ca.GetSerializedBoard();
                foreach (var kvp in ca.GetStatesCount())
                    stateGraphs[kvp.Key].Add((double)kvp.Value / cellCount);
                if (i >= writeBufferLen)
                {
                    File.AppendAllText(caLogPath, string.Join("", writeBuffer));
                    i = 0;
                }
                Console.SetCursorPosition(0, 0);
                Console.Write(100d / ca.Config.StepCount * ca.Step);
                Console.Write('%');
            }
            File.AppendAllText(caLogPath, string.Join("", writeBuffer.Where((s, j) => j <= i)));
            File.WriteAllText(graphPath, new GraphsDescriber { StateGraphs = stateGraphs }.Serialize());
            File.WriteAllText(configPath, config.Serialize());
        }
    }
}
