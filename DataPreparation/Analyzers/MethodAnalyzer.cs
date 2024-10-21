using Mono.Cecil.Cil;
using Mono.Cecil;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPreparation.Analyzers
{
    internal class MethodAnalyzer
    {


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
    }
}
