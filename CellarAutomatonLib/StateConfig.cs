using System.Collections.Generic;

namespace CellarAutomatonLib
{
    public class StateConfig
    {
        public string Start { get; set; }
        public string Name { get; set; }
        public Dictionary<int, string> StateMachine { get; set; }
        public string Preprocessor { get; set; }
        public string Postprocessor { get; set; }
    }
}