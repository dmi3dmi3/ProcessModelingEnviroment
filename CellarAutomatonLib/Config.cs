using System.Collections.Generic;
using Newtonsoft.Json;

namespace CellarAutomatonLib
{
    public class Config
    {
        public const string CaLogName = "CellarAutomatonLog";
        public int Height { get; set; }
        public int Width { get; set; }
        public int IsolationPercent { get; set; }
        public int StepCount { get; set; }
        public Dictionary<int, StateConfig> States { get; set; }
        public bool LoopEdges { get; set; }
        public Dictionary<string, int> Memory { get; set; }
        public Dictionary<string, int> Global { get; set; }
        public Dictionary<string, string> Paths { get; set; }

        public static Config Deserialize(string str) => JsonConvert.DeserializeObject<Config>(str);
        public string Serialize() => JsonConvert.SerializeObject(this);
    }
}