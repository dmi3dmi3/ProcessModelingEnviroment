using Microsoft.CSharp;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PmeConsoleFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            var text = File.ReadAllText(Path.Combine(
                Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName,
                @"TestData\project1.cfg"));
            var config = JsonConvert.DeserializeObject<Config>(text);
            var ca = new CellarAutomaton(config);

            var width = Math.Max(config.Width, 8) * 2 + 1;
            var height = Math.Max(config.Height, 8) + 1;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            //ca.InitTest();
            //ca.Init(1234);
            ca.Init();
            var sb = new StringBuilder(ca.Config.Height * (ca.Config.Width + 1));
            for (int i = 0; i < ca.Config.StepCount; i++)
            {
                for (int j = 0; j < ca.Config.Height; j++)
                {
                    for (int k = 0; k < ca.Config.Width; k++)
                    {
                        sb.Append(ca.Board[j, k].State == 1 ? '\u2588' : ' ');
                        sb.Append(ca.Board[j, k].State == 1 ? '\u2588' : ' ');
                    }

                    sb.Append(Environment.NewLine);
                }
                Console.SetCursorPosition(0, 0);
                Console.WriteLine(sb.ToString());
                sb.Clear();
                ca.CalculateNext();
                Task.Delay(50).Wait();
            }

        }
    }


    public static class CodeGenerator
    {
        public static Dictionary<int, Dictionary<int, Func<Neighbors, Dictionary<string, int>, bool>>> GetStateMachine(
            Dictionary<int, Dictionary<int, string>> input)
        {
            CodeDomProvider cpd = new CSharpCodeProvider();
            var cp = new CompilerParameters
            {
                TempFiles = { KeepFiles = true },
                GenerateExecutable = false
            };
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add(new Uri(Assembly.GetExecutingAssembly().EscapedCodeBase).LocalPath);
            var methodsCode = input
                .SelectMany(from => from.Value
                    .Select(to =>
                        GetMethod(from.Key, to.Key, to.Value)))
                .ToList();

            var classCode =
$@"
using System.Collections.Generic;

namespace PmeConsoleFramework
{{
    public static class GeneratedCode 
    {{
{string.Join(Environment.NewLine, methodsCode)}
    }}
}}
";
            var cr = cpd.CompileAssemblyFromSource(cp, classCode);
            var assembly = cr.CompiledAssembly;
            var type = assembly.GetType("PmeConsoleFramework.GeneratedCode");
            var methods = type.GetMethods();
            var result = new Dictionary<int, Dictionary<int, Func<Neighbors, Dictionary<string, int>, bool>>>();
            foreach (var from in input)
            {
                result.Add(from.Key, new Dictionary<int, Func<Neighbors, Dictionary<string, int>, bool>>());
                foreach (var to in from.Value.Select(_ => _.Key))
                {
                    var t = methods.First(_ => _.Name == $"From{from.Key}To{to}");
                    result[from.Key].Add(to,
                        (Func<Neighbors, Dictionary<string, int>, bool>)Delegate.CreateDelegate(typeof(Func<Neighbors, Dictionary<string, int>, bool>), t));
                }
            }

            return result;
        }

        static string GetMethod(int from, int to, string code) =>
$@"
        public static bool From{from}To{to}(Neighbors neighbors, Dictionary<string, int> cell)
        {{
            {code}
        }}";
    }

    public class Config
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int IsolationPercent { get; set; }
        public int StepCount { get; set; }
        public Dictionary<int, StateConfig> States { get; set; }
        public bool LoopEdges { get; set; }
        public Dictionary<string, int> Memory { get; set; }
    }

    public class StateConfig
    {
        public int StartPercent { get; set; }
        public Dictionary<int, string> StateMachine { get; set; }
    }

    public class Neighbors
    {
        public Cell[] NeighborsCell { get; set; }
        public int StateCount(int state)
        {
            return NeighborsCell.Count(_ => _.State == state);
        }
    }

    [DebuggerDisplay("S={" + nameof(State) + "}")]
    public class Cell
    {
        public int State { get; set; }
        public Dictionary<string, int> Memory { get; set; }
        public Cell(int state, Dictionary<string, int> memory = null)
        {
            State = state;
            Memory = memory;
        }

        public Cell GetCopy() => new Cell(State, new Dictionary<string, int>(Memory));
    }

    class CellarAutomaton
    {
        public Config Config { get; set; }
        public Cell[,] Board { get; set; }
        public Dictionary<int, Dictionary<int, Func<Neighbors, Dictionary<string, int>, bool>>> StateMachine { get; set; }
        private Random _random;

        public CellarAutomaton(Config config)
        {
            Config = config;
            Board = new Cell[Config.Height, Config.Width];
            StateMachine = CodeGenerator.GetStateMachine(
                config.States.ToDictionary(_ => _.Key, _ => _.Value.StateMachine));
        }

        public void InitTest()
        {
            Board = new Cell[3, 3]
            {
                {new Cell(0), new Cell(1), new Cell(0) },
                {new Cell(0), new Cell(1), new Cell(0) },
                {new Cell(0), new Cell(1), new Cell(0) },
            };
        }
        public void Init(int? seed = null)
        {
            _random = seed.HasValue
                ? new Random(seed.Value)
                : new Random();
            var dict = Config.States.ToDictionary(_ => _.Key, _ => _.Value.StartPercent);
            if (dict.Sum(_ => _.Value) != 100)
                throw new Exception("Sum of percents not equal 100");

            var statesDisp = new int[100];
            var c = 0;
            foreach (var kvp in dict)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    statesDisp[c] = kvp.Key;
                    c++;
                }
            }

            for (var i = 0; i < Board.GetLength(0); i++)
                for (var j = 0; j < Board.GetLength(1); j++)
                {
                    Board[i, j] = new Cell(statesDisp[_random.Next(100)], new Dictionary<string, int>(Config.Memory));
                }
        }

        public void CalculateNext()
        {
            var result = new Cell[Board.GetLength(0), Board.GetLength(1)];
            for (var i = 0; i < Board.GetLength(0); i++)
                for (var j = 0; j < Board.GetLength(1); j++)
                {
                    result[i, j] = Board[i, j].GetCopy();
                    var nb = new Neighbors()
                    {
                        NeighborsCell = GetNeighbors(i, j).ToArray()
                    };
                    int? newState = null;
                    foreach (var kvp in StateMachine[Board[i, j].State])
                    {
                        if (!kvp.Value(nb, result[i, j].Memory))
                            continue;
                        newState = kvp.Key;
                        break;
                    }

                    if (newState.HasValue)
                        result[i, j].State = newState.Value;
                }

            Board = result;
        }

        private readonly List<(int, int)> _shiftList = new List<(int, int)>
        {
            (-1, -1),
            (-1, 0),
            (-1, 1),
            (0, -1),
            (0, 1),
            (1, -1),
            (1, 0),
            (1, 1)
        };
        private IEnumerable<Cell> GetNeighbors(int x, int y)
        {
            if (Config.LoopEdges)
            {
                foreach (var (i, j) in _shiftList)
                {
                    var k = (x + i + Config.Height) % Config.Height;
                    var h = (y + j + Config.Width) % Config.Width;
                    yield return Board[k, h].GetCopy();
                }
            }
            else
            {
                foreach (var (i, j) in _shiftList)
                {
                    var k = x + i;
                    if (-1 == k || k == Config.Height)
                        continue;
                    var h = y + j;
                    if (-1 == h || h == Config.Width)
                        continue;

                    yield return Board[k, h].GetCopy();
                }
            }
        }
    }
}