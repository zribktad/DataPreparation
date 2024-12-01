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
        public static string GetSourceCodeForType(Type type)
        {
            var assemblyPath = type.Assembly.Location;

            // Krok 2: Načítať všetky súbory zdrojového kódu z tejto assembly
            var sourceFiles = Directory.GetFiles(Path.GetDirectoryName(assemblyPath), "*.cs", SearchOption.AllDirectories);

            foreach (var file in sourceFiles)
            {
                // Krok 3: Prečítať zdrojový kód a vytvoriť syntaktický strom
                var sourceCode = File.ReadAllText(file);
                var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
                var root = syntaxTree.GetRoot();

                // Krok 4: Hľadať deklaráciu triedy, ktorá zodpovedá danému typu
                var classDeclaration = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(cd => cd.Identifier.Text == type.Name);

                if (classDeclaration != null)
                {
                    return classDeclaration.ToFullString();
                }
            }

            // Ak sa trieda nenašla, vrátime chybové hlásenie
            throw new Exception($"Source code for type {type.Name} not found.");

        }
     
        
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
        
        public static void AnalyzeTestCase(string testSourceCodePath, Type testCaseType)
        {
            // Read the source code of the test case
            var testSourceCodeString = ReadAllFile(testSourceCodePath);
            var syntaxTree = CSharpSyntaxTree.ParseText(testSourceCodeString);
            var root = syntaxTree.GetRoot();
            // find the Test Case
            var testCase = FindNodeForTestCase(testCaseType, root);
            // Find all Test Methods in the Test Case
            var testMethods = FindTestMethodsForCase(testCase);
            // Analyze each test method
            testMethods.ForEach(testMethod => AnalyzeTestMethod(testMethod,testCase));

         
        }

        private static void AnalyzeTestMethod(MethodDeclarationSyntax testSourceCode, ClassDeclarationSyntax testCase)
        {
            var assertionStatements = GetAssertionStatementsForTestMethod(testSourceCode);
            Console.WriteLine("Analyzing Test Method: " + testSourceCode.Identifier);
            foreach (var assertion in assertionStatements)
            {
                Console.WriteLine("Found FluentAssertion Statement: " + assertion);
                var assertVariable = assertion.Expression.ToString().Split('.')[0];
            
                var arguments = assertion.ArgumentList.Arguments;
                TraceVariableDeclaration(testCase, assertVariable);
                if (arguments.Count > 0)
                {
                    var expectedValue = arguments[0].Expression;
                    Console.WriteLine($"Expected Value in Assertion: {expectedValue}");
                
                    // Trace the variable declaration
                    TraceVariableDeclaration(testCase, expectedValue.ToString());
                }
            }
        }

        private static IEnumerable<InvocationExpressionSyntax> GetAssertionStatementsForTestMethod(MethodDeclarationSyntax testSourceCode)
        {
            var assertionStatements = testSourceCode.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation.Expression.ToString().Contains("Should()"));
            return assertionStatements;
        }

        private static List<MethodDeclarationSyntax> FindTestMethodsForCase(ClassDeclarationSyntax testCase)
        {
            var testMethods = testCase.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.AttributeLists
                    .SelectMany(list => list.Attributes)
                    .Any(attr => attr.Name.ToString() == "Test")).ToList(); // TODO maybe add more or more specific values like [DataPreparationAnalysis]
            
            foreach (var method in testMethods)
            {
                Console.WriteLine("Found Test Method: " + method.Identifier);
            }
            
            return testMethods;
        }

        private static ClassDeclarationSyntax FindNodeForTestCase(Type testCaseType, SyntaxNode root)
        {
            var testCase = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(classDeclaration => classDeclaration.Identifier.Text == testCaseType.Name
                                                    && classDeclaration.AttributeLists
                                                        .SelectMany(list => list.Attributes)
                                                        .Any(attr => attr.Name.ToString() == nameof(DataPreparationTestCaseAttribute).Replace("Attribute", "")));
            
            if (testCase == null)
            {
                Console.WriteLine("Test Case not found.");
                throw new Exception("Test Case not found.");
            }
            else
            {
                Console.WriteLine("Found Test Case: " + testCase.Identifier);
            }
            return testCase;
        }

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
            var assertVariable = assertion.Expression.ToString().Split('.')[0];
            
            var arguments = assertion.ArgumentList.Arguments;
            TraceVariableDeclaration(root, assertVariable);
            if (arguments.Count > 0)
            {
                var expectedValue = arguments[0].Expression;
                Console.WriteLine($"Expected Value in Assertion: {expectedValue}");
                
                // Trace the variable declaration
                TraceVariableDeclaration(root, expectedValue.ToString());
            }
        }
        // Now track references to all variables in the method
      //  TrackVariableSetters(root, "result");
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
            .LastOrDefault(v => v.Identifier.Text == variableName);
        
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

    static void TrackVariableReferences(SyntaxNode root)
    {
        var res = root.ChildNodes().Select(node => node.ChildNodes().Select(nodeChild => nodeChild.ChildNodes()))
            .ToList();
        var t = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().ToList();
        var t2 = root.DescendantNodes().ToList();
        var variableReferences = root.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Where(id => id.Identifier.Text != "Should") // Avoid FluentAssertions references
            .ToList();

        foreach (var reference in variableReferences)
        {
            Console.WriteLine($"Variable reference found: {reference.Identifier.Text}");
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
                    if (methodInfo != null && DataTypeStore.HasMethodDataPreparationType(methodInfo))
                    {
                        AnalyzeMethodCalls(calledMethodDef, calledMethodResult);
                    }

                    result.AddCalledMethod(calledMethodResult);
                }
            }
        }

        #endregion

        public static string ReadAllFile(string filePath)
        {
           return System.IO.File.ReadAllText(filePath);
        }
    }
}