using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using StateMachineType = System.Func<CellarAutomatonLib.Neighbors, System.Collections.Generic.Dictionary<string, double>, System.Collections.Generic.Dictionary<string, double>, int, bool>;

namespace CellarAutomatonLib
{
    public static class CodeGenerator
    {
        public static Dictionary<int, Dictionary<int, StateMachineType>> GetStateMachine(
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

            foreach (var from in input.Select(_ => _.Key).ToArray())
                if (input[from] == null)
                    input[from] = new Dictionary<int, string>();
            var methodsCode = input
                 .SelectMany(from => @from.Value
                     .Select(to =>
                         GetMethod(@from.Key, to.Key, to.Value))
                 )
                 .ToList();

            var classCode =
                $@"
using System;
using System.Collections.Generic;

namespace CellarAutomatonLib
{{
    public static class GeneratedCode 
    {{
    private static Random random = new Random();
    private static double RandomPercent()
    {{
        return random.Next(100000) / 1000d;
    }}

{string.Join(Environment.NewLine, methodsCode)}
    }}
}}
";
            var cr = cpd.CompileAssemblyFromSource(cp, classCode);
            var assembly = cr.CompiledAssembly;
            var type = assembly.GetType("CellarAutomatonLib.GeneratedCode");
            var methods = type.GetMethods();
            var result = new Dictionary<int, Dictionary<int, StateMachineType>>();
            foreach (var from in input)
            {
                result.Add(from.Key, new Dictionary<int, StateMachineType>());
                foreach (var to in from.Value.Select(_ => _.Key))
                {
                    var t = methods.First(_ => _.Name == $"From{from.Key}To{to}");
                    result[from.Key].Add(to, (StateMachineType) Delegate.CreateDelegate(typeof(StateMachineType), t));
                }
            }

            return result;
        }

        static string GetMethod(int from, int to, string code) =>
            $@"
        public static bool From{from}To{to}(Neighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n)
        {{
            {code}
        }}";
    }
}