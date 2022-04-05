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

        /// <summary>
        /// Returns StateMachine adding quotes
        /// </summary>
        public Dictionary<int, string> GetStateMachine() => 
            StateMachine?.ToDictionary(kvp => kvp.Key, kvp => AddQuotes(kvp.Value));

        /// <summary>
        /// Returns Preprocessor adding quotes
        /// </summary>
        public string GetPreprocessor() => AddQuotes(Preprocessor);

        /// <summary>
        /// Returns Postprocessor adding quotes
        /// </summary>
        public string GetPostprocessor() => AddQuotes(Postprocessor);

        private static string AddQuotes(string str)
        {
            var list = str.ToList();
            for (var i = 1; i < list.Count - 1; i++)
            {
                switch (list[i])
                {
                    case '[' when list[i + 1] != '"':
                        list.Insert(i + 1, '"');
                        break;
                    case ']' when list[i - 1] != '"':
                        list.Insert(i, '"');
                        i++;
                        break;
                }
            }

            return string.Concat(list);
        }
    }
}