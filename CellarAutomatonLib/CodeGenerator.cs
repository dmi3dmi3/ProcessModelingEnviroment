using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using PluginSDK;
using ProcessorType =
    System.Action<CellarAutomatonLib.Neighbors, System.Collections.Generic.Dictionary<string, double>,
        System.Collections.Generic.Dictionary<string, double>, int, int, int>;
using StartStateType = System.Func<int, int, bool>;
using StateMachineType =
    System.Func<CellarAutomatonLib.Neighbors, System.Collections.Generic.Dictionary<string, double>,
        System.Collections.Generic.Dictionary<string, double>, int, int, int, bool>;

namespace CellarAutomatonLib
{
    public static class CodeGenerator
    {
        #region Templates

//StateMachine
        private const string StateMachineClassMethodsReplace = "#METHODS#";

        private const string StateMachineClassTemplate = $@"using System;
using System.Collections.Generic;
using PluginSDK;

namespace CellarAutomatonLib
{{
    public static class StateMachineGeneratedCode 
    {{
        private static Random random = new Random();
        private static double RandomPercent(int mantissaLen = 3)
        {{
            int multiplicator = (int)Math.Pow(10, mantissaLen);
            return random.Next(100 * multiplicator) / (double)multiplicator;
        }}

        {StateMachineClassMethodsReplace}
    }}
}}
";

        private const string StateMachineMethodFromReplace = "#FROM#";
        private const string StateMachineMethodToReplace = "#TO#";
        private const string StateMachineMethodCodeReplace = "#CODE#";

        private const string StateMachineMethodTemplate = $@"
        public static bool From{StateMachineMethodFromReplace}To{StateMachineMethodToReplace}(Neighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n, int x, int y)
        {{
            {StateMachineMethodCodeReplace}
        }}";

        private const string StateMachineMethodPluginTemplate = $@"
        public static bool From{StateMachineMethodFromReplace}To{StateMachineMethodToReplace}(Neighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n, int x, int y)
        {{
            return PluginManager.StateChangeCommands[{"\"" + StateMachineMethodCodeReplace + "\""}].Execute(neighbors, memory, global, n, x, y);
        }}";

//StartState
        private const string StartStateClassMethodsReplace = "#METHODS#";

        private const string StartStateClassTemplate = $@"using System;

namespace CellarAutomatonLib
{{
    public static class StartStateGeneratedCode 
    {{
        {StartStateClassMethodsReplace}
    }}
}}";

        private const string StartStateMethodIndexReplace = "#INDEX#";
        private const string StartStateMethodCodeReplace = "#CODE#";

        private const string StartStateMethodTemplate = $@"
public static bool StartState{StartStateMethodIndexReplace}(int x, int y)
{{
    {StartStateMethodCodeReplace}
}}";

//Preprocessor
        private const string PreprocessorClassMethodsReplace = "#METHODS#";

        private const string PreprocessorClassTemplate = $@"using System;
using System.Collections.Generic;
using PluginSDK;

namespace CellarAutomatonLib
{{
    public static class PreprocessorGeneratedCode 
    {{
        private static Random random = new Random();
        private static double NextRandom(int maxVal)
        {{
            return random.Next(maxVal);
        }}

            {PreprocessorClassMethodsReplace}
    }}
}}";

        private const string PreprocessorMethodIndexReplace = "#INDEX#";
        private const string PreprocessorMethodCodeReplace = "#CODE#";

        private const string PreprocessorMethodTemplate = $@"
public static void Preprocessor{PreprocessorMethodIndexReplace}(Neighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n, int x, int y)
{{
    {PreprocessorMethodCodeReplace}
}}";

        private const string PreprocessorMethodPluginTemplate = $@"
public static void Preprocessor{PreprocessorMethodIndexReplace}(Neighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n, int x, int y)
{{
    PluginManager.ProcessorCommands[{"\"" + PostprocessorMethodCodeReplace + "\""}].Execute(neighbors, memory, global, n, x, y);
}}";

//Postprocessor
        private const string PostprocessorClassMethodsReplace = "#METHODS#";

        private const string PostprocessorClassTemplate = $@"using System;
using System.Collections.Generic;
using PluginSDK;

namespace CellarAutomatonLib
{{
    public static class PostprocessorGeneratedCode 
    {{
        private static Random random = new Random();
        private static double NextRandom(int maxVal)
        {{
            return random.Next(maxVal);
        }}
            {PostprocessorClassMethodsReplace}
    }}
}}";

        private const string PostprocessorMethodIndexReplace = "#INDEX#";
        private const string PostprocessorMethodCodeReplace = "#CODE#";

        private const string PostprocessorMethodTemplate = $@"
public static void Postprocessor{PostprocessorMethodIndexReplace}(Neighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n, int x, int y)
{{
    {PostprocessorMethodCodeReplace}
}}";

        private const string PostprocessorMethodPluginTemplate = $@"
public static void Postprocessor{PostprocessorMethodIndexReplace}(Neighbors neighbors, Dictionary<string, double> memory, Dictionary<string, double> global, int n, int x, int y)
{{
    PluginManager.ProcessorCommands[{"\"" + PostprocessorMethodCodeReplace + "\""}].Execute(neighbors, memory, global, n, x, y);
}}";

        #endregion

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
            cp.ReferencedAssemblies.Add(new Uri(typeof(Neighbors).Assembly.EscapedCodeBase).LocalPath);
            cp.ReferencedAssemblies.Add(new Uri(typeof(INeighbors).Assembly.EscapedCodeBase).LocalPath);

            var keys = input.Select(_ => _.Key).ToArray();
            foreach (var from in keys)
                input[from] ??= new Dictionary<int, string>();

            var methodsCode = input
                    .SelectMany(from => from.Value
                        .Select(to =>
                            (PluginManager.StateChangeCommands.ContainsKey(to.Value)
                                ? StateMachineMethodPluginTemplate
                                : StateMachineMethodTemplate)
                        .Replace(StateMachineMethodFromReplace, from.Key.ToString())
                        .Replace(StateMachineMethodToReplace, to.Key.ToString())
                        .Replace(StateMachineMethodCodeReplace, to.Value))
                )
                .ToList();


            var classCode = StateMachineClassTemplate.Replace(
                StateMachineClassMethodsReplace, string.Join(Environment.NewLine, methodsCode)
            );
            var cr = cpd.CompileAssemblyFromSource(cp, classCode);
            if (cr.Errors.HasErrors)
                throw new Exception(
                    string.Join(
                        Environment.NewLine,
                        cr.Errors.Cast<CompilerError>().Select(_ => _.ErrorText)) +
                    Environment.NewLine + cr.Errors[0].FileName
                );
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

        public static Dictionary<int, StartStateType> GetStartStateFuncs(Dictionary<int, string> input)
        {
            CodeDomProvider cpd = new CSharpCodeProvider();
            var cp = new CompilerParameters
            {
                TempFiles = { KeepFiles = true },
                GenerateExecutable = false
            };

            var methodsCode = input
                .Select(_ => StartStateMethodTemplate
                    .Replace(StartStateMethodIndexReplace, _.Key.ToString())
                    .Replace(StartStateMethodCodeReplace, _.Value))
                .ToList();

            var classCode = StartStateClassTemplate.Replace(
                StartStateClassMethodsReplace, string.Join(Environment.NewLine, methodsCode)
            );

            var cr = cpd.CompileAssemblyFromSource(cp, classCode);
            if (cr.Errors.HasErrors)
                throw new Exception(
                    string.Join(Environment.NewLine, cr.Errors.Cast<CompilerError>().Select(_ => _.ErrorText)) +
                    Environment.NewLine + cr.Errors[0].FileName);
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

        public static Dictionary<int, ProcessorType> GetPreprocessorFuncs(Dictionary<int, string> input)
        {
            CodeDomProvider cpd = new CSharpCodeProvider();
            var cp = new CompilerParameters
            {
                TempFiles = { KeepFiles = true },
                GenerateExecutable = false
            };
            cp.ReferencedAssemblies.Add(new Uri(typeof(Neighbors).Assembly.EscapedCodeBase).LocalPath);
            cp.ReferencedAssemblies.Add(new Uri(typeof(INeighbors).Assembly.EscapedCodeBase).LocalPath);

            var methodsCode = input
                .Select(_ =>
                    (PluginManager.ProcessorCommands.ContainsKey(_.Value)
                        ? PreprocessorMethodPluginTemplate
                        : PreprocessorMethodTemplate)
                    .Replace(PreprocessorMethodIndexReplace, _.Key.ToString())
                    .Replace(PreprocessorMethodCodeReplace, _.Value))
                .ToList();

            var classCode = PreprocessorClassTemplate
                .Replace(PreprocessorClassMethodsReplace, string.Join(Environment.NewLine, methodsCode));

            var cr = cpd.CompileAssemblyFromSource(cp, classCode);
            if (cr.Errors.HasErrors)
                throw new Exception(
                    string.Join(Environment.NewLine, cr.Errors.Cast<CompilerError>().Select(_ => _.ErrorText)) +
                    Environment.NewLine + cr.Errors[0].FileName);

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

        public static Dictionary<int, ProcessorType> GetPostprocessorFuncs(Dictionary<int, string> input)
        {
            CodeDomProvider cpd = new CSharpCodeProvider();
            var cp = new CompilerParameters
            {
                TempFiles = { KeepFiles = true },
                GenerateExecutable = false
            };
            cp.ReferencedAssemblies.Add(new Uri(typeof(Neighbors).Assembly.EscapedCodeBase).LocalPath);
            cp.ReferencedAssemblies.Add(new Uri(typeof(INeighbors).Assembly.EscapedCodeBase).LocalPath);

            var methodsCode = input
                .Select(_ => (PluginManager.ProcessorCommands.ContainsKey(_.Value)
                        ? PostprocessorMethodPluginTemplate
                        : PostprocessorMethodTemplate)
                    .Replace(PostprocessorMethodIndexReplace, _.Key.ToString())
                    .Replace(PostprocessorMethodCodeReplace, _.Value))
                .ToList();

            var classCode = PostprocessorClassTemplate
                .Replace(PostprocessorClassMethodsReplace, string.Join(Environment.NewLine, methodsCode));

            var cr = cpd.CompileAssemblyFromSource(cp, classCode);

            if (cr.Errors.HasErrors)
                throw new Exception(
                    string.Join(Environment.NewLine, cr.Errors.Cast<CompilerError>().Select(_ => _.ErrorText)) +
                    Environment.NewLine + cr.Errors[0].FileName);

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
    }
}