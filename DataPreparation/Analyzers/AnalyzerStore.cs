using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DataPreparation.Analyzers;

internal static class AnalyzerStore
{
    static readonly Dictionary<Type, AnalyzerTestClassStore?> _analyzers = new();

    internal static AnalyzerTestClassStore? AddOrGetAnalyzeData(Type testClassType, string? sourceCodeString = null)
    {
        if (_analyzers.TryGetValue(testClassType, out var analyzer))
        {
            return analyzer;
        }

        if (sourceCodeString == null)
        {
            return null;
        }

        var newAnalyzer = new AnalyzerTestClassStore(sourceCodeString);
        _analyzers.Add(testClassType, newAnalyzer);
        return newAnalyzer;
    }

    internal static AnalyzerTestMethodData? AddOrGetAnalyzeMethodData(Type testClassType, MethodInfo testMethodInfo)
    {
        if (_analyzers.TryGetValue(testClassType, out var analyzer))
        {
            return analyzer?.AddOrGetMethodData(testMethodInfo);
        }

        return null;
    }
}

class AnalyzerTestClassStore
{
    internal readonly SyntaxTree SyntaxTree;
    internal readonly CSharpCompilation Compilation;
    internal readonly SemanticModel Model;
    internal readonly SyntaxNode Root;
    internal readonly Dictionary<MethodInfo, AnalyzerTestMethodData?> TestMethodStore ;

    public AnalyzerTestClassStore(string sourceCodeString)
    {
         TestMethodStore = new();
         SyntaxTree = CSharpSyntaxTree.ParseText(sourceCodeString);
         Compilation = CSharpCompilation.Create("DataPreparationCompilation", new[] { SyntaxTree });
         Model = Compilation.GetSemanticModel(SyntaxTree);
         Root = SyntaxTree.GetRoot();
    }
    
    public AnalyzerTestMethodData? AddOrGetMethodData(MethodInfo testMethodName)
    {
        if (TestMethodStore.TryGetValue(testMethodName, out var methodData))
        {
            return methodData;
        }
        
        var method = Root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(m => m.Identifier.Text == testMethodName.Name);
        if (method == null)
        {
            return null;
        }

        methodData = new AnalyzerTestMethodData(SyntaxTree, Compilation, Model, Root, method);
        TestMethodStore.Add(testMethodName, methodData);
        return methodData;
    }
}

internal class AnalyzerTestMethodData
{
    internal readonly SyntaxTree SyntaxTree;
    internal readonly CSharpCompilation Compilation;
    internal readonly SemanticModel Model;
    internal readonly SyntaxNode FileRoot;
    internal readonly SyntaxNode Root;

    public AnalyzerTestMethodData(SyntaxTree syntaxTree, CSharpCompilation compilation, SemanticModel model, SyntaxNode fileRoot, SyntaxNode root)
    {
        SyntaxTree = syntaxTree;
        Compilation = compilation;
        Model = model;
        FileRoot = fileRoot;
        Root = root;
    }
}