using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateMachineType = System.Func<CellarAutomatonLib.Neighbors, System.Collections.Generic.Dictionary<string, double>, System.Collections.Generic.Dictionary<string, double>, int, bool>;


namespace CellarAutomatonLib
{
    public class CellarAutomaton
    {
        public Config Config { get; set; }
        public Cell[,] Board { get; set; }
        public Dictionary<int, Dictionary<int, StateMachineType>> StateMachine { get; set; }
        public int Step { get; private set; }
        private Random _random;
        private string[] _headers;
        private int _sbLen;
        
        public CellarAutomaton(Config config)
        {
            Config = config;

            if (Config.States.Sum(_ => _.Value.StartPercent) != 100)
                throw new Exception("Sum of percents not equal 100");
            if (Config.IsolationPercent < 0)
                throw new Exception("IsolationPercent can not be lower that 0");
            if (Config.IsolationPercent > 100)
                throw new Exception("IsolationPercent can not be higher that 100");

            Board = new Cell[Config.Height, Config.Width];
            StateMachine = CodeGenerator.GetStateMachine(
                config.States.ToDictionary(_ => _.Key, _ => _.Value.StateMachine));
            _headers = Config.Memory.Select(_ => _.Key).ToArray();
            _sbLen = (Config.Width * (1 + _headers.Length * 2) + 1) * Config.Width;
        }

        public void InitTest()
        {
            Board = new[,]
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
            Step = 0;

            var dict = Config.States.ToDictionary(_ => _.Key, _ => _.Value.StartPercent);

            var statesDisp = new int[100];
            var c = 0;
            foreach (var kvp in dict)
                for (var i = 0; i < kvp.Value; i++)
                {
                    statesDisp[c] = kvp.Key;
                    c++;
                }

            for (var i = 0; i < Board.GetLength(0); i++)
            for (var j = 0; j < Board.GetLength(1); j++)
            {
                Board[i, j] = new Cell(statesDisp[_random.Next(100)], Config.Memory != null ? new Dictionary<string, double>(Config.Memory) : null);
            }
        }

        public void CalculateNext()
        {
            Step++;
            var result = new Cell[Board.GetLength(0), Board.GetLength(1)];
          
            for (var i = 0; i < Board.GetLength(0); i++)
            for (var j = 0; j < Board.GetLength(1); j++)
            {
                result[i, j] = Board[i, j].GetCopy();
                var nb = new Neighbors
                {
                    NeighborsCell = GetNeighbors(i, j).ToArray()
                };
                int? newState = null;
                foreach (var kvp in StateMachine[Board[i, j].State])
                {
                    if (!kvp.Value(nb, result[i, j].Memory, Config.Global, Step))
                        continue;
                    newState = kvp.Key;
                    break;
                }

                if (newState.HasValue)
                    result[i, j].State = newState.Value;
            }

            Board = result;
        }

        public string GetSerializedBoard()
        {
            var sb = new StringBuilder(_sbLen);
            for (var i = 0; i < Config.Height; i++)
            {
                for (var j = 0; j < Config.Width; j++)
                {
                    sb.Append(Board[i, j].State);
                    sb.Append(',');
                }

                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        public Dictionary<int, int> GetStatesCount()
        {
            var result = Config.States.ToDictionary(_ => _.Key, _ => 0);
            for (int i = 0; i < Config.Height; i++)
            for (int j = 0; j < Config.Width; j++)
                result[Board[i, j].State]++;
            return result;
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
                    if (_random.Next(100) < Config.IsolationPercent)
                        continue;
                    var k = (x + i + Config.Height) % Config.Height;
                    var h = (y + j + Config.Width) % Config.Width;
                    yield return Board[k, h].GetCopy();
                }
            }
            else
            {
                foreach (var (i, j) in _shiftList)
                {
                    if (_random.Next(100) < Config.IsolationPercent)
                        continue;
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