using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using StateMachineType = System.Func<CellarAutomatonLib.Neighbors, System.Collections.Generic.Dictionary<string, double>, System.Collections.Generic.Dictionary<string, double>, int, bool>;
using StartStateType = System.Func<int, int, bool>;
using ProcessorType = System.Action<CellarAutomatonLib.Neighbors, System.Collections.Generic.Dictionary<string, double>, System.Collections.Generic.Dictionary<string, double>, int>;

namespace CellarAutomatonLib
{
    public static class CodeGenerator
    {
        #region StateMachine
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

            var keys = input.Select(_ => _.Key).ToArray();
            foreach (var from in keys)
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (input[from] == null)
                    input[from] = new Dictionary<int, string>();

            var methodsCode = input
                 .SelectMany(from => @from.Value
                     .Select(to =>
                         GetStateMachineMethod(@from.Key, to.Key, to.Value))
                 )
                 .ToList();
            

            var classCode =
                $@"using System;

namespace CellarAutomatonLib
{{
    public static class StateMachineGeneratedCode 
    {{
    private static Random random = new Random();
    private static double RandomPercent(int mantissaLen = 3)
    {{
        double multiplicator = Math.Pow(10, mantissaLen)
        return random.Next(100 * multiplicator) / 10d * multiplicator;
    }}

{string.Join(Environment.NewLine, methodsCode)}
    }}
}}
";
            var cr = cpd.CompileAssemblyFromSource(cp, classCode);
            var assembly = cr.CompiledAssembly;
            var type = assembly.GetType("CellarAutomatonLib.StateMachineGeneratedCode");
            var methods = type.GetMethods();
            var result = new Dictionary<int, Dictionary<int, StateMachineType>>();
            foreach (var from in input)
            {
                result.Add(from.Key, new Dictionary<int, StateMachineType>());
                foreach (var to in from.Value.Select(_ => _.Key))
                {
                    var t = methods.First(_ => _.Name == $"From{from.Key}To{to}");
                    result[from.Key].Add(to, (StateMachineType)Delegate.CreateDelegate(typeof(StateMachineType), t));
                }
            }

            return result;
        }

        private static string GetStateMachineMethod(int from, int to, string code) =>
            $@"
        public static bool From{from}To{to}(Neighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n)
        {{
            {code}
        }}";

        #endregion

        #region StartState

        public static Dictionary<int, StartStateType> GetStartStateFuncs(Dictionary<int, string> input)
        {
            CodeDomProvider cpd = new CSharpCodeProvider();
            var cp = new CompilerParameters
            {
                TempFiles = { KeepFiles = true },
                GenerateExecutable = false
            };

            var methodsCode = input.Select(_ => GetStartStateMethod(_.Key, _.Value))
                .ToList();

            var classCode =
                $@"using System;

namespace CellarAutomatonLib
{{
    public static class StartStateGeneratedCode 
    {{
{string.Join(Environment.NewLine, methodsCode)}
    }}
}}";

            var cr = cpd.CompileAssemblyFromSource(cp, classCode);
            var assembly = cr.CompiledAssembly;
            var type = assembly.GetType("CellarAutomatonLib.StartStateGeneratedCode");
            var methods = type.GetMethods();
            var result = new Dictionary<int, StartStateType>(input.Count);
            foreach (var kvp in input)
            {
                var t = methods.First(_ => _.Name == $"StartState{kvp.Key}");
                result.Add(kvp.Key, (StartStateType)Delegate.CreateDelegate(typeof(StartStateType), t));
            }

            return result;
        }

        private static string GetStartStateMethod(int i, string code) =>
            $@"public static bool StartState{i}(int x, int y)
{{
    {code}
}}";

        #endregion

        #region PreProcessor

        public static Dictionary<int, ProcessorType> GetPreprocessorFuncs(Dictionary<int, string> input)
        {
            CodeDomProvider cpd = new CSharpCodeProvider();
            var cp = new CompilerParameters
            {
                TempFiles = { KeepFiles = true },
                GenerateExecutable = false
            };

            var methodsCode = input.Select(_ => GePreprocessorMethod(_.Key, _.Value))
                .ToList();

            var classCode =
                $@"using System;

namespace CellarAutomatonLib
{{
    public static class PreprocessorGeneratedCode 
    {{
    private static Random random = new Random();
    private static double NextRandom(int maxVal)
    {{
        return random.Next(maxVal);
    }}

{string.Join(Environment.NewLine, methodsCode)}
    }}
}}";

            var cr = cpd.CompileAssemblyFromSource(cp, classCode);
            var assembly = cr.CompiledAssembly;
            var type = assembly.GetType("CellarAutomatonLib.PreprocessorGeneratedCode");
            var methods = type.GetMethods();
            var result = new Dictionary<int, ProcessorType>();
            foreach (var kvp in input)
            {
                var t = methods.First(_ => _.Name == $"Preprocessor{kvp.Key}");
                result.Add(kvp.Key, (ProcessorType)Delegate.CreateDelegate(typeof(ProcessorType), t));
            }

            return result;
        }

        private static string GePreprocessorMethod(int i, string code) =>
            $@"public static bool Preprocessor{i}(Neighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n)
{{
    {code}
}}";

        #endregion       
        
        #region PostProcessor

        public static Dictionary<int, ProcessorType> GetPostprocessorFuncs(Dictionary<int, string> input)
        {
            CodeDomProvider cpd = new CSharpCodeProvider();
            var cp = new CompilerParameters
            {
                TempFiles = { KeepFiles = true },
                GenerateExecutable = false
            };

            var methodsCode = input.Select(_ => GePostprocessorMethod(_.Key, _.Value))
                .ToList();

            var classCode =
                $@"using System;

namespace CellarAutomatonLib
{{
    private static Random random = new Random();
    private static double NextRandom(int maxVal)
    {{
        return random.Next(maxVal);
    }}

    public static class PostprocessorGeneratedCode 
    {{
{string.Join(Environment.NewLine, methodsCode)}
    }}
}}";

            var cr = cpd.CompileAssemblyFromSource(cp, classCode);
            var assembly = cr.CompiledAssembly;
            var type = assembly.GetType("CellarAutomatonLib.PostprocessorGeneratedCode");
            var methods = type.GetMethods();
            var result = new Dictionary<int, ProcessorType>();
            foreach (var kvp in input)
            {
                var t = methods.First(_ => _.Name == $"Postprocessor{kvp.Key}");
                result.Add(kvp.Key, (ProcessorType)Delegate.CreateDelegate(typeof(ProcessorType), t));
            }

            return result;
        }

        private static string GePostprocessorMethod(int i, string code) =>
            $@"public static bool Postprocessor{i}(Neighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n)
{{
    {code}
}}";

        #endregion

    }


}