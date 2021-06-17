using System.Collections.Generic;
using System.Linq;

namespace CellarAutomatonLib
{
    public class StateConfig
    {
        public string Start { get; set; }
        public string Name { get; set; }
        public Dictionary<int, string> StateMachine { get; set; }
        public string Preprocessor { get; set; }
        public string Postprocessor { get; set; }

        public Dictionary<int, string> GetStateMachine()
        {
            if (StateMachine == null)
                return null;

            var result = new Dictionary<int, string>();
            foreach (var kvp in StateMachine)
            {
                var list = kvp.Value.ToList();
                for (var i = 1; i < list.Count -1; i++)
                {
                    if (list[i] == '[' && list[i + 1] != '"')
                    {
                        list.Insert(i+1, '"');
                    }
                    else if (list[i] == ']' && list[i - 1] != '"')
                    {
                        list.Insert(i, '"');
                        i++;
                    }
                }
                result.Add(kvp.Key, string.Concat(list));
            }

            return result;
        }
    }
}