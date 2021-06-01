using System.Collections.Generic;

namespace PmeConsoleFramework
{
    public class Config
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int IsolationPercent { get; set; }
        public int StepCount { get; set; }
        public Dictionary<int, StateConfig> States { get; set; }
        public bool LoopEdges { get; set; }
        public Dictionary<string, int> Memory { get; set; }
        public Dictionary<string, int> Global { get; set; }
    }
}