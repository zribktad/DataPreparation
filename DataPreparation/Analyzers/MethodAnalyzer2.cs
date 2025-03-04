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
    internal class MethodAnalyzer2
    {
        public static string testSourceCodeTest = @"
using System;
using FluentAssertions;

public class ExampleTests
{
    [Test]
    public void Test_Addition()
    {
        int a = 5;
        int b = 3;
        int result = Add(a, b);
        result.Should().Be(8);
        result.Should().Be().Empty();
    }

    private int Add(int x, int y)
    {
        return x + y;
    }
}";

  public static void AnalyzeTestMethod(string testSourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(testSourceCode);
        var root = syntaxTree.GetRoot();

        // Locate FluentAssertions Should().Be() Calls
        var assertionStatements = root.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation => invocation.Expression.ToString().Contains("Should().Be"));

        foreach (var assertion in assertionStatements)
        {
            Console.WriteLine("Found FluentAssertion Statement: " + assertion);

            var arguments = assertion.ArgumentList.Arguments;
            if (arguments.Count > 0)
            {
                var expectedValue = arguments[0].Expression;
                Console.WriteLine($"Expected Value in Assertion: {expectedValue}");

                // Trace the variable declaration
                TraceVariableDeclaration(root, expectedValue.ToString());
            }
        }

        // Now track references and setters to all variables in the method
        TrackVariableSetters(root, "result");
    }

    static void TraceVariableDeclaration(SyntaxNode root, string variableName)
    {
        // If the variable name is actually a literal value, skip tracing it
        if (IsLiteralValue(variableName))
        {
            Console.WriteLine($"Literal Expected Value: {variableName}");
            return;
        }

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

    static void TrackVariableSetters(SyntaxNode root, string variableName)
    {
        // Find assignments to the variable
        var assignments = root.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Where(assignment => assignment.Left.ToString() == variableName)
            .ToList();

        foreach (var assignment in assignments)
        {
            Console.WriteLine($"Assignment found to '{variableName}': {assignment}");
        }

        // Find setter methods if 'result' is a property
        var setterMethods = root.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Where(property => property.Identifier.Text == variableName && property.AccessorList != null)
            .SelectMany(property => property.AccessorList.Accessors)
            .Where(accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
            .ToList();

        foreach (var setter in setterMethods)
        {
            Console.WriteLine($"Setter method found for '{variableName}': {setter}");
        }
    }

    static bool IsLiteralValue(string value)
    {
        // Check if the string is a numeric literal or a valid value in the assertion
        return int.TryParse(value, out _);
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
                    if (methodInfo != null && DataRelationStore.HasMethodDataPreparationType(methodInfo))
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