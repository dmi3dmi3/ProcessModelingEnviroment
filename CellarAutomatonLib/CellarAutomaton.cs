using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StateMachineType = System.Func<CellarAutomatonLib.Neighbors, System.Collections.Generic.Dictionary<string, double>, System.Collections.Generic.Dictionary<string, double>, int, bool>;
using StartStateType = System.Func<int, int, bool>;
using ProcessorType = System.Action<CellarAutomatonLib.Neighbors, System.Collections.Generic.Dictionary<string, double>, System.Collections.Generic.Dictionary<string, double>, int>;


namespace CellarAutomatonLib
{
    public class CellarAutomaton
    {
        public Config Config { get; set; }
        public Cell[,] Board { get; set; }
        public Dictionary<int, Dictionary<int, StateMachineType>> StateMachine { get; set; }
        public Dictionary<int, StartStateType> StartStateFunc { get; set; }
        public Dictionary<int, int> StartStateCount { get; set; }
        public Dictionary<int, double> StartStatePercent { get; set; }
        public Dictionary<int, ProcessorType> Preprocessor { get; set; }
        public Dictionary<int, ProcessorType> Postprocessor { get; set; }
        public int Step { get; private set; }
        private Random _random;
        private readonly int _sbLen;

        public CellarAutomaton(Config config)
        {
            Config = config;
            StartStateFunc = new Dictionary<int, StartStateType>();
            StartStateCount = new Dictionary<int, int>();
            StartStatePercent = new Dictionary<int, double>();
            Preprocessor = new Dictionary<int, ProcessorType>();
            Postprocessor = new Dictionary<int, ProcessorType>();
            _sbLen = (Config.Width + 1) * Config.Width;

            if (Config.IsolationPercent < 0)
                throw new Exception("IsolationPercent can not be lower that 0");
            if (Config.IsolationPercent > 100)
                throw new Exception("IsolationPercent can not be higher that 100");
            //init board
            Board = new Cell[Config.Height, Config.Width];

            //init state machine
            StateMachine = CodeGenerator
                .GetStateMachine(config.States.ToDictionary(_ => _.Key, _ => _.Value.StateMachine));

            //init start configs
            var startConfigs = config.States.ToDictionary(_ => _.Key, _ => _.Value.Start);
            var funcs = new Dictionary<int, string>();
            foreach (var startConfig in startConfigs)
            {
                if (startConfig.Value.Contains("return"))
                {
                    funcs.Add(startConfig.Key, startConfig.Value);
                    continue;
                }

                if (int.TryParse(startConfig.Value, out var c))
                {
                    StartStateCount.Add(startConfig.Key, c);
                    continue;
                }

                if (startConfig.Value.EndsWith("%", StringComparison.Ordinal)
                    && double.TryParse(startConfig.Value.Substring(0, startConfig.Value.Length - 1), out var p))
                {
                    StartStatePercent.Add(startConfig.Key, p);
                }
            }
            StartStateFunc = CodeGenerator.GetStartStateFuncs(funcs);
            if (Math.Abs(StartStatePercent.Sum(_ => _.Value) - 100) > 0.01)
                throw new Exception("Sum of percents not equal 100");


            //init pre/post processor
            var preprocStates = config.States
                .Where(_ => _.Value.Preprocessor != null)
                .ToArray();
            if (preprocStates.Length != 0)
                Preprocessor = CodeGenerator.GetPreprocessorFuncs(
                        preprocStates.ToDictionary(_ => _.Key, _ => _.Value.Preprocessor));

            var postprocStates = config.States
                .Where(_ => _.Value.Postprocessor != null)
                .ToArray();
            if (postprocStates.Length != 0)
                Postprocessor = CodeGenerator.GetPostprocessorFuncs(
                    postprocStates.ToDictionary(_ => _.Key, _ => _.Value.Postprocessor));
        }

        public void Init(int? seed = null)
        {
            _random = seed.HasValue
                ? new Random(seed.Value)
                : new Random();
            Step = 0;
            var preBoard = new int?[Config.Height, Config.Width];
            var n = preBoard.Length;


            for (var i = 0; i < Config.Height; i++)
            {
                for (var j = 0; j < Config.Width; j++)
                {
                    int? res = null;
                    foreach (var func in StartStateFunc.Where(func => func.Value(i, j)))
                        res = func.Key;

                    if (res.HasValue)
                    {
                        preBoard[i, j] = res;
                        n--;
                    }
                }
            }

            var cSum = StartStateCount.Sum(_ => _.Value);
            if (cSum > n)
                throw new Exception("Sum of counted cells is more than count of available cells");
            if (StartStatePercent.Count == 0 && cSum < n)
                throw new Exception("Sum of counted cells is less than count of available cells, and configuration has no percent start values ");

            void PlaceCell(int state)
            {
                var x = _random.Next(Config.Height);
                var y = _random.Next(Config.Width);
                do
                {
                    if (preBoard[x, y] == null)
                    {
                        preBoard[x, y] = state;
                        break;
                    }

                    x++;
                    if (x == Config.Height)
                    {
                        x = 0;
                        y++;
                    }

                    if (y == Config.Width)
                    {
                        y = 0;
                    }
                } while (true);
            }

            foreach (var kvp in StartStateCount)
                for (int i = 0; i < kvp.Value; i++)
                {
                    PlaceCell(kvp.Key);
                    n--;
                }

            var countSum = 0;
            foreach (var kvp in StartStatePercent)
            {
                var count = (int)(n * (kvp.Value / 100));
                countSum += count;
                if (countSum > n)
                    count -= countSum - n;
                for (int i = 0; i < count; i++)
                    PlaceCell(kvp.Key);
            }

            var diff = n - countSum;
            if (diff > 0)
            {
                var state = StartStatePercent.Last().Key;
                for (var i = 0; i < diff; i++)
                    PlaceCell(state);
            }

            Board = new Cell[Config.Height, Config.Width];
            for (var i = 0; i < Config.Height; i++)
                for (var j = 0; j < Config.Width; j++)
                {
                    if (!preBoard[i, j].HasValue)
                        throw new Exception("Board initialization ended with error, check your configuration");
                    Board[i, j] = new Cell(preBoard[i, j].Value, Config.Memory != null ? new Dictionary<string, double>(Config.Memory) : null);
                }
        }

        public void CalculateNext()
        {
            Step++;
            PreprocessorExecute();
            ChangeState();
            PostprocessorExecute();
        }

        private void PreprocessorExecute()
        {
            if (Preprocessor == null || Preprocessor.Count == 0)
            {
                return;
            }
            for (var i = 0; i < Board.GetLength(0); i++)
                for (var j = 0; j < Board.GetLength(1); j++)
                {
                    if (!Preprocessor.TryGetValue(Board[i, j].State, out var action)) 
                        continue;
                    var nb = Neighbors.GetNeighbors(i, j, Board, _random, Config);
                    action(nb, Board[i, j].Memory, Config.Global, Step);
                }
        }

        private void PostprocessorExecute()
        {
            if (Postprocessor == null || Postprocessor.Count == 0)
            {
                return;
            }
            for (var i = 0; i < Board.GetLength(0); i++)
                for (var j = 0; j < Board.GetLength(1); j++)
                {
                    if (!Postprocessor.TryGetValue(Board[i, j].State, out var action)) 
                        continue;
                    var nb = Neighbors.GetNeighbors(i, j, Board, _random, Config);
                    action(nb, Board[i, j].Memory, Config.Global, Step);
                }
        }

        private void ChangeState()
        {
            var result = new Cell[Board.GetLength(0), Board.GetLength(1)];

            for (var i = 0; i < Board.GetLength(0); i++)
                for (var j = 0; j < Board.GetLength(1); j++)
                {
                    result[i, j] = Board[i, j].GetCopy();
                    var nb = Neighbors.GetNeighbors(i, j, Board, _random, Config);
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
    }
}