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
            var projectName = "project2";
            var configPath = Path.Combine(
                Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName,
                @"TestData\" + projectName + ".cfg");
            var text = File.ReadAllText(configPath);
            var config = Config.Deserialize(text);
            var ca = new CellarAutomaton(config);
            var caLogPath = Path.Combine(Directory.GetParent(configPath).FullName, projectName + ".ca");
            if (config.Paths == null) 
                config.Paths = new Dictionary<string, string>();
            if (config.Paths.ContainsKey(Config.CaLogName))
                config.Paths[Config.CaLogName] = caLogPath;
            else
                config.Paths.Add(Config.CaLogName, caLogPath);

            var writeBufferLen = 100;
            var writeBuffer = new string[writeBufferLen];
            var i = 0;
            File.Create(caLogPath).Close();

            ca.Init();
            writeBuffer[i++] = ca.GetSerializedBoard();
            while (ca.Step < ca.Config.StepCount)
            {
                ca.CalculateNext();
                writeBuffer[i++] = ca.GetSerializedBoard();
                if (i >= writeBufferLen)
                {
                    File.AppendAllText(caLogPath, string.Join("", writeBuffer));
                    i = 0;
                }
                Console.SetCursorPosition(0,0);
                Console.Write(100d / ca.Config.StepCount * ca.Step);
                Console.Write('%');
            }
            File.AppendAllText(caLogPath, string.Join("", writeBuffer.Where((s, j) => j <= i)));
            File.WriteAllText(configPath, config.Serialize());
        }
    }
}
