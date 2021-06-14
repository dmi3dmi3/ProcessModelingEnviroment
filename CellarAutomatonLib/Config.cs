using System.Collections.Generic;
using Newtonsoft.Json;

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
        public string Serialize() => JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
    }
}