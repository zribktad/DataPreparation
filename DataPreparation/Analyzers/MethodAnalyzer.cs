using System.Reflection;
using DataPreparation.Helpers;
using DataPreparation.Testing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DataPreparation.Analyzers
{
    internal class MethodAnalyzer
    {
        public static string testSourceCodeTest = @"
                using System;
                using NUnit.Framework;

                public class ExampleTests
                {
                    [Test]
                    public void Test_Addition()
                    {
                        int a = 5;
                        int b = 3;
                        int result = Add(a, b);
                        Assert.AreEqual(8, result);
                    }

                    private int Add(int x, int y)
                    {
                        return x + y;
                    }
                }";

        public static void GenerateDataFromAST(string testSourceCode)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(testSourceCodeTest);
            var root = syntaxTree.GetRoot();

            var assertStatements = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation.Expression.ToString().Contains("Assert"));

            foreach (var assert in assertStatements)
            {
                Console.WriteLine($"Found Assert: {assert}");
            }

            foreach (var assert in assertStatements)
            {
                var arguments = assert.ArgumentList.Arguments;
                if (arguments.Count > 1)
                {
                    var resultExpression = arguments[1].Expression; // The second argument (e.g., `result`)
                    Console.WriteLine($"Result Variable: {resultExpression}");
                }
            }
        }

        public static void AnalyzeTestMethod(string testSourceCode)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(testSourceCode);
            var root = syntaxTree.GetRoot();

            // Locate Assert Statements
            var assertStatements = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation.Expression.ToString().Contains("Assert.AreEqual"));

            foreach (var assert in assertStatements)
            {
                Console.WriteLine("Found Assert Statement: " + assert);

                var arguments = assert.ArgumentList.Arguments;
                if (arguments.Count > 1)
                {
                    var resultExpression = arguments[1].Expression;
                    Console.WriteLine($"Result Variable in Assert: {resultExpression}");

                    // Trace the variable declaration
                    TraceVariableDeclaration(root, resultExpression.ToString());
                }
            }
        }

        static void TraceVariableDeclaration(SyntaxNode root, string variableName)
        {
            var variableDeclaration = root.DescendantNodes()
                .OfType<VariableDeclaratorSyntax>()
                .FirstOrDefault(v => v.Identifier.Text == variableName);

            if (variableDeclaration != null)
            {
                Console.WriteLine($"Variable '{variableName}' declared: {variableDeclaration}");

                var initializer = variableDeclaration.Initializer?.Value;
                Console.WriteLine($"Assigned Value/Expression: {initializer}");

                if (initializer is InvocationExpressionSyntax methodCall)
                {
                    AnalyzeMethodCall(root, methodCall);
                }
            }
            else
            {
                Console.WriteLine($"Variable '{variableName}' not found.");
            }
        }

        static void AnalyzeMethodCall(SyntaxNode root, InvocationExpressionSyntax methodCall)
        {
            var methodName = methodCall.Expression.ToString();
            Console.WriteLine($"Method Called: {methodName}");

            var arguments = methodCall.ArgumentList.Arguments;
            Console.WriteLine("Arguments Passed:");
            foreach (var arg in arguments)
            {
                Console.WriteLine($" - {arg}");
            }

            // Locate the method declaration
            var methodDeclaration = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == methodName);

            if (methodDeclaration != null)
            {
                Console.WriteLine($"Method '{methodName}' Implementation:");
                Console.WriteLine(methodDeclaration.Body);
            }
            else
            {
                Console.WriteLine($"Method '{methodName}' not found in the source.");
            }
        }


        #region Cecil MethodAnalyzer

        public static MethodAnalysisResult Analyze(MethodInfo method)
        {
            var assemblyPath = method.DeclaringType.Assembly.Location;
            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);

            var type = assembly.MainModule.GetType(method.DeclaringType.FullName);
            MethodDefinition? methodDef = type.Methods.FirstOrDefault(m => m.Name == method.Name);

            if (methodDef == null) return null;

            var analysisResult = new MethodAnalysisResult(method.Name, method.ReturnType.FullName);

            AnalyzeMethodCalls(methodDef, analysisResult);

            return analysisResult;
        }

        private static void AnalyzeMethodCalls(MethodDefinition methodDef, MethodAnalysisResult result)
        {
            var calledMethods = methodDef.Body.Instructions
                .Where(i => i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt)
                .ToArray();

            foreach (var instruction in calledMethods)
            {
                var methodReference = (MethodReference)instruction.Operand;

                MethodDefinition calledMethodDef = methodReference.Resolve();
                if (calledMethodDef != null)
                {
                    var calledMethodResult = new MethodAnalysisResult(
                        calledMethodDef
                    );

                    foreach (var param in calledMethodDef.Parameters)
                    {
                        calledMethodResult.Parameters.Add(new ParameterInfo(param.Name, param.ParameterType.FullName));
                    }

                    var methodInfo = MethodConvertor.GetInfo(calledMethodDef);
                    if (methodInfo != null && DataTypeStore.HasMethodDataPreparationType(methodInfo))
                    {
                        AnalyzeMethodCalls(calledMethodDef, calledMethodResult);
                    }

                    result.AddCalledMethod(calledMethodResult);
                }
            }
        }

        #endregion
    }
}