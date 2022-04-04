using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CellarAutomatonLib
{
    public class Config
    {
        public const string CaLogName = "CellarAutomatonLog";
        public const string StateGraphsName = "StateGraphs";
        public int Height { get; set; }
        public int Width { get; set; }
        public int IsolationPercent { get; set; }
        public int StepCount { get; set; }
        public Dictionary<int, StateConfig> States { get; set; }
        public bool LoopEdges { get; set; }
        public Dictionary<string, double> Memory { get; set; }
        public Dictionary<string, double> Global { get; set; }
        public Dictionary<string, string> Paths { get; set; }


        public static Config Deserialize(string str) => JsonConvert.DeserializeObject<Config>(str);
        public string Serialize()
        {
            var t = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            });
            t = Replace(t);

            return t;
        }

        private static string Replace(string str)
        {
            var list = str.Select(_ => _.ToString()).ToList();
            for (var i = 1; i < list.Count - 1; i++)
            {
                if (list[i] == "\\" && list[i + 1] == "r" && list[i + 2] == "\\" && list[i + 3] == "n")
                {
                    list.RemoveAt(i);
                    list.RemoveAt(i);
                    list.RemoveAt(i);
                    list.RemoveAt(i);
                    list.Insert(i, Environment.NewLine);
                }
                else if (list[i] == "\\" && list[i + 1] == "t" && list[i - 1] != "\\")
                {
                    list.RemoveAt(i);
                    list.RemoveAt(i);
                    list.Insert(i, "    ");
                }
            }

            return string.Concat(list);
        }
    }
}