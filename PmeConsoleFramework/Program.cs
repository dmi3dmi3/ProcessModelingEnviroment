using CellarAutomatonLib;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PmeConsoleFramework
{
    class Program
    {
        private static char[] symbols = {
            ' ',
            '\u2588',
            '*',
            '&',
            '?',
            '@',
        };

        static void Main(string[] args)
        {
            var text = File.ReadAllText(Path.Combine(
                Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName,
                @"TestData\project2.cfg"));
            var config = Config.Deserialize(text);
            var ca = new CellarAutomaton(config);

            var width = Math.Max(config.Width, 8) * 2 + 1;
            var height = Math.Max(config.Height, 8) + 1;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            //ca.InitTest();
            //ca.Init(1234);
            ca.Init();
            var sb = new StringBuilder(ca.Config.Height * (ca.Config.Width + 1));
            while (ca.Step < ca.Config.StepCount)
            {
                for (var j = 0; j < ca.Config.Height; j++)
                {
                    for (var k = 0; k < ca.Config.Width; k++)
                    {
                        sb.Append(symbols[ca.Board[j, k].State]);
                        sb.Append(symbols[ca.Board[j, k].State]);
                    }

                    sb.Append(Environment.NewLine);
                }
                sb.Append(ca.Step);
                sb.Append(Environment.NewLine);
                Console.SetCursorPosition(0, 0);
                Console.WriteLine(sb.ToString());
                sb.Clear();
                Task.WaitAll(Task.Run(() => ca.CalculateNext()), Task.Delay(50));
            }
        }
    }
}
