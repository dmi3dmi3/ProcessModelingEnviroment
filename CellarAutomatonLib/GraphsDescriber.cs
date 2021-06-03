using System.Collections.Generic;
using Newtonsoft.Json;

namespace CellarAutomatonLib
{
    public class GraphsDescriber
    {
        public Dictionary<int, List<double>> StateGraphs { get; set; }

        public string Serialize() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static GraphsDescriber Deserialize(string str) => JsonConvert.DeserializeObject<GraphsDescriber>(str);
    }
}