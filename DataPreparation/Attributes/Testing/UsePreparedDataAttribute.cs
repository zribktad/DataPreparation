using DataPreparation.Data;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class UsePreparedDataAttribute: Attribute, ITestAction
    {
        public ActionTargets Targets => ActionTargets.Test;
        public UsePreparedDataAttribute()
        {
        }

        private List<IDataPreparation> _dataPreparations = new();
        public UsePreparedDataAttribute(params Type[] dataProviders)
        {
            //get from Data{Register} the data preparation for the test
            foreach (var dataProvider in dataProviders)
            {
                var dataPreparationType = DataPreparationRegister.GetDataPreparationType(dataProvider);
                if (dataPreparationType != null)
                {
                    var dataPreparation = (IDataPreparation)Activator.CreateInstance(dataPreparationType);
                    _dataPreparations.Add(dataPreparation);
                   
                }
            }

        }
        public void BeforeTest(ITest test)
        {

            //up data for the test
            foreach (var dataPreparation in _dataPreparations)
            {
                dataPreparation.TestUpData();
            }
            //Test of method analysis
            AnalyzeA(test);
            AnalyzeB(test);
        }

        public void AfterTest(ITest test)
        {
            //down data for the test
            foreach (var dataPreparation in _dataPreparations)
            {
                dataPreparation.TestDownData();
            }


        }

        #region Analyze

        private void AnalyzeB(ITest test)
        {
            var method = test.Method.MethodInfo;
      
            
     

            // Get the source code as a string (you might have to adjust this based on your setup)
            //var sourceCode = GetSourceCode(method);

            //if (!string.IsNullOrEmpty(sourceCode))
            //{
            //    // Analyze the source code to find called methods
            //    var calledMethods = GetCalledMethods(sourceCode, method.Name);
            //    foreach (var calledMethod in calledMethods)
            //    {
            //        Console.WriteLine($"Called method: {calledMethod}");
            //    }
            //}
        }

        private static void AnalyzeA(ITest test)
        {
            var method = test.Method.MethodInfo;

            // Load the assembly containing the test method
            var assemblyPath = method.DeclaringType.Assembly.Location;
            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);

            // Get the type and method definitions
            var type = assembly.MainModule.GetType(method.DeclaringType.FullName);
            MethodDefinition? methodDef = type.Methods.FirstOrDefault(m => m.Name == method.Name);

            if (methodDef != null)
            {
                Console.WriteLine($"Analyzing method: {method.Name}");
                //var calledMethodNames2 = methodDef.Body.Instructions
                //    .Where(i => i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt)
                //    .ToArray();
                Instruction[] calledMethodNames = methodDef.Body.Instructions
                    .Where(i => i.OpCode == OpCodes.Call)
                    .ToArray();

                foreach (var calledMethod in calledMethodNames)
                {
                    Console.WriteLine($"Called method: {((MethodReference)calledMethod.Operand).FullName}");
                }
            }
        }


        #endregion



  

        #region Helpers

        private string[] GetCalledMethods(MethodDefinition methodDefinition)
        {
            var calledMethodNames = methodDefinition.Body.Instructions
                .Where(i => i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt)
                .Select(i => ((MethodReference)i.Operand).Name)
                .ToArray();

            return calledMethodNames;
        }
        private string[] GetCalledMethods2(string methodSource)
        {
            // Parse the C# code into a Syntax Tree
            var tree = CSharpSyntaxTree.ParseText(methodSource);
            var root = tree.GetRoot();

            // Find method invocations
            var methodCalls = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(invocation => invocation.Expression.ToString())
                .ToArray();

            return methodCalls;
        }

        #endregion


    }
}
