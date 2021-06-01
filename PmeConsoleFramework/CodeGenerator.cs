using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;

namespace PmeConsoleFramework
{
    public static class CodeGenerator
    {
        public static Dictionary<int, Dictionary<int, Func<Neighbors, Dictionary<string, int>, Dictionary<string, int>, int, bool>>> GetStateMachine(
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

namespace PmeConsoleFramework
{{
    public static class GeneratedCode 
    {{
    private static Random random = new Random();

{string.Join(Environment.NewLine, methodsCode)}
    }}
}}
";
            var cr = cpd.CompileAssemblyFromSource(cp, classCode);
            var assembly = cr.CompiledAssembly;
            var type = assembly.GetType("PmeConsoleFramework.GeneratedCode");
            var methods = type.GetMethods();
            var result = new Dictionary<int, Dictionary<int, Func<Neighbors, Dictionary<string, int>, Dictionary<string, int>, int, bool>>>();
            foreach (var from in input)
            {
                result.Add(from.Key, new Dictionary<int, Func<Neighbors, Dictionary<string, int>, Dictionary<string, int>, int, bool>>());
                foreach (var to in from.Value.Select(_ => _.Key))
                {
                    var t = methods.First(_ => _.Name == $"From{from.Key}To{to}");
                    result[from.Key].Add(to,
                        (Func<Neighbors, Dictionary<string, int>, Dictionary<string, int>, int, bool>)
                        Delegate.CreateDelegate(typeof(Func<Neighbors, Dictionary<string, int>, Dictionary<string, int>, int, bool>), t));
                }
            }

            return result;
        }

        static string GetMethod(int from, int to, string code) =>
            $@"
        public static bool From{from}To{to}(Neighbors neighbors, Dictionary<string, int> memory, Dictionary<string, int> global, int n)
        {{
            {code}
        }}";
    }
}