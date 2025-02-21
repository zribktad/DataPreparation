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
    
 
    internal static class MethodAnalyzer
    {
        public static string GetSourceCodeForType(Type type)
        {
            var assemblyPath = type.Assembly.Location;

            var sourceFiles = Directory.GetFiles(Path.GetDirectoryName(assemblyPath), "*.cs", SearchOption.AllDirectories);

            foreach (var file in sourceFiles)
            {
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
        
        public static void AnalyzeTestFixture(string testSourceCodePath, Type testFixtureType)
        {
            // Read the source code of the test case
            var testSourceCodeString = ReadAllFile(testSourceCodePath);
            // SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(testSourceCodeString);
            // CSharpCompilation compilation = CSharpCompilation.Create("DataPreparationCompilation", new[] { syntaxTree });
            // SemanticModel model = compilation.GetSemanticModel(syntaxTree);
            // SyntaxNode root = syntaxTree.GetRoot();
            
            var t = AnalyzerStore.AddOrGetAnalyzeTestFixtureData(testFixtureType,testSourceCodeString);

            if (t == null)
            {
                throw new Exception("Analyzing of this test case not working.");
            }
            
            // var methodDeclarations = analyzedMethodData.Root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            // foreach (var method in methodDeclarations)
            // {
            //     var methodSymbol = analyzedMethodData.Model.GetDeclaredSymbol(method) as IMethodSymbol;
            //     Console.WriteLine("Found Method Symbol from compil: " + methodSymbol.Name + " ns: " + methodSymbol.ContainingNamespace + " class: " + methodSymbol.ContainingType.Name);
            //     
            // }
            //
            // // find the Test Case
            // var testFixture = FindNodeForTestFixture(testFixtureType, analyzedMethodData.Root);
            // // Find all Test Methods in the Test Case
            // var testMethods = FindTestMethodsForCase(testFixture);
            // // Analyze each test method
            // testMethods.ForEach(testMethod => AnalyzeTestMethod(testMethod,testFixture));
            //
         
        }

        public static void AnalyzeTestMethod(Type testFixtureType, MethodInfo methodMethodInfo)
        {
            var analyzedMethodData = AnalyzerStore.AddOrGetAnalyzeMethodData(testFixtureType,methodMethodInfo);
            AnalyzeTestMethod(analyzedMethodData.Root, analyzedMethodData.FileRoot);
            
          
            
        }

        static void PrintSyntaxTree(SyntaxNode node, int level)
        {
            // Odsadenie podľa úrovne
            var indent = new string(' ', level * 2);

            // Vypísanie informácií o uzle
            if (node is MemberAccessExpressionSyntax memberAccess)
            {
                Console.WriteLine($"{indent}MemberAccessExpressionSyntax: Target = {memberAccess.Expression}, Member = {memberAccess.Name}");
            }
            else if (node is InvocationExpressionSyntax invocation)
            {
                var method = invocation.Expression is MemberAccessExpressionSyntax ma
                    ? ma.Name.ToString()
                    : invocation.Expression.ToString();
                Console.WriteLine($"{indent}InvocationExpressionSyntax: Method = {method}");
                Console.WriteLine($"{indent}  Arguments:");
                foreach (var argument in invocation.ArgumentList.Arguments)
                {
                    PrintSyntaxTree(argument, level + 2);
                }
            }
            else if (node is ArgumentSyntax argument)
            {
                Console.WriteLine($"{indent}ArgumentSyntax: {argument}");
                PrintSyntaxTree(argument.Expression, level + 1); // Vypíš podrobnosti o výraze v argumente
            }
            else if (node is IdentifierNameSyntax identifier)
            {
                Console.WriteLine($"{indent}IdentifierNameSyntax: Identifier = {identifier.Identifier.ValueText} with ID: {identifier.Identifier}");
            }
            else if (node is LiteralExpressionSyntax literal)
            {
                Console.WriteLine($"{indent}LiteralExpressionSyntax: Literal = {literal.Token.Value}");
            }
            else
            {
                Console.WriteLine($"{indent}{node.Kind()}");
            }

            // Rekurzia pre poduzly
            foreach (var child in node.ChildNodes())
            {
                PrintSyntaxTree(child, level + 1);
            }
        }
        private static void AnalyzeTestMethod(MethodDeclarationSyntax methodDeclarationSyntax, SyntaxNode testFixture)
        {
            var assertionStatements = GetAssertionStatementsForTestMethod(methodDeclarationSyntax);
            Console.WriteLine("Analyzing Test Method: " + methodDeclarationSyntax.Identifier);
            foreach (var assertion in assertionStatements)
            {
                Console.WriteLine("Found FluentAssertion Statement: " + assertion);
            }
            

            foreach (var assertion in assertionStatements)
                {
                    Console.WriteLine("For Found FluentAssertion Statement: " + assertion);   
                //PrintSyntaxTree(assertion, 0);
                var assertVariable = assertion.Expression.ToString().Split('.')[0];
                var fluentAssertionTreeNodes = assertion.DescendantNodes();
               // assertion.ChildNodes().ToList().ForEach(node => Console.WriteLine(node));
                foreach (var fluentAssertionTreeNode in fluentAssertionTreeNodes)
                {
                    if (fluentAssertionTreeNode is MemberAccessExpressionSyntax memberAccess)
                    {
                        Console.WriteLine($"MemberAccessExpressionSyntax: Target = {memberAccess.Expression}, Member = {memberAccess.Name}");
                    }
                    else if (fluentAssertionTreeNode is InvocationExpressionSyntax invocation)
                    {
                        var method = invocation.Expression is MemberAccessExpressionSyntax ma
                            ? ma.Name.ToString()
                            : invocation.Expression.ToString();
                        Console.WriteLine($"InvocationExpressionSyntax: Method = {method}, Arguments = {string.Join(", ", invocation.ArgumentList.Arguments)}");
                    }
                    else if (fluentAssertionTreeNode is IdentifierNameSyntax identifier)
                    {
                        Console.WriteLine($"IdentifierNameSyntax: Identifier = {identifier.Identifier.ValueText}");
                    }
                    else if (fluentAssertionTreeNode is LiteralExpressionSyntax literal)
                    {
                        Console.WriteLine($"LiteralExpressionSyntax: Literal = {literal.Token.Value}");
                    }
                    else
                    {
                        Console.WriteLine($"Other Syntax Node: {fluentAssertionTreeNode.Kind()}");
                    }
                    
                    TraceVariableDeclaration(testFixture, "result");
                    
                }
                
                //TraceVariableDeclaration(testFixture, assertVariable);
                
                // var arguments = assertion.ArgumentList.Arguments;
                // foreach (var argument in arguments)
                // {
                //     var expectedValue = argument.Expression;
                //     Console.WriteLine($"Expected Value in Assertion: {expectedValue}");
                //
                //     // Trace the variable declaration
                //     TraceVariableDeclaration(methodDeclarationSyntax, expectedValue.ToString());
                // }
                }
        }

        static string ShouldClausule2 = "#track#.#param#.Should().Be(#value#)";
        static string ShouldClausule = "#track#.Should(c => c.#param# == #value#)";
        static string ShouldBeClausule = "#track#.Should().Be(#value#)";
        static string AssertClausule = "Assert.That(#track#, Is.EqualTo(#value#))";
        static string AssertClausule2 = "Assert.That(#track#, Has.Exactly(#count#).Matches<*>(* => *.#param# == #value#))"; //Assert.That(result, Has.Exactly(1).Matches<Customer>(c => c.Id == 1L));
        private static IEnumerable<InvocationExpressionSyntax> GetAssertionStatementsForTestMethod(MethodDeclarationSyntax testSourceCode)
        {
            var assertionStatements = testSourceCode.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation.Expression.ToString().Contains("Should()"));
            return assertionStatements;
        }

        private static List<MethodDeclarationSyntax> FindTestMethodsForCase(ClassDeclarationSyntax testFixture)
        {
            var testMethods = testFixture.DescendantNodes()
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

        private static ClassDeclarationSyntax FindNodeForTestFixture(Type testFixtureType, SyntaxNode root)
        {
            var testFixture = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(classDeclaration => classDeclaration.Identifier.Text == testFixtureType.Name
                                                    && classDeclaration.AttributeLists
                                                        .SelectMany(list => list.Attributes)
                                                        .Any(attr => attr.Name.ToString() == nameof(DataPreparationTestFixtureAttribute).Replace("Attribute", "")));
            
            if (testFixture == null)
            {
                Console.WriteLine("Test Case not found.");
                throw new Exception("Test Case not found.");
            }
            else
            {
                Console.WriteLine("Found Test Case: " + testFixture.Identifier);
            }
            return testFixture;
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
        //TrackVariableSetters(root, "result");
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