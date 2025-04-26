using Mono.Cecil;

namespace DataPreparation.Analyzers;


internal class MethodAnalysisResult
{
    public string MethodName { get; set; }
    public string ReturnType { get; set; }
    public List<ParameterInfo> Parameters { get; set; } = new List<ParameterInfo>();
    public List<MethodAnalysisResult> CalledMethods { get; set; } = new List<MethodAnalysisResult>();

    public MethodAnalysisResult(string methodName, string returnType)
    {
        MethodName = methodName;
        ReturnType = returnType;
    }

    public MethodAnalysisResult(MethodDefinition? methodDef)
    {
        MethodName = methodDef.Name;
        ReturnType = methodDef.ReturnType.FullName;
        
    }

    public void AddCalledMethod(MethodAnalysisResult calledMethod)
    {
        CalledMethods.Add(calledMethod);
    }
    
    public void Print(int indent = 0)
    {
        var indentText = new string(' ', indent * 2);
        Console.WriteLine($"{indentText}Method: {MethodName} (Return type: {ReturnType})");

        foreach (var param in Parameters)
        {
            Console.WriteLine($"{indentText}  Parameter: {param.Name} ({param.Type})");
        }

        foreach (var calledMethod in CalledMethods)
        {
            calledMethod.Print(indent + 1);
        }
    }
}

internal class ParameterInfo(string name, string type)
{
    public string Name { get; set; } = name;
    public string Type { get; set; } = type;
}

