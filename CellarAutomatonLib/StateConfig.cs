using System.Collections.Generic;

namespace CellarAutomatonLib
{
    public class StateConfig
    {
        public int StartPercent { get; set; }
        public Dictionary<int, string> StateMachine { get; set; }
    }
}