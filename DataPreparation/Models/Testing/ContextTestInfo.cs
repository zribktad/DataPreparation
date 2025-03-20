using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Data;

public class ContextTestInfo
{
    private readonly string _id;
    private readonly object?[] _arguments;
    private readonly string? _className;
    private readonly string? _methodName;
    private readonly string _fullName;
    ITest? Test { get; }

    protected ContextTestInfo(ITest test)
    {
        _id = test.Id;
        _arguments = test.Arguments;
        _className = test.ClassName;
        _methodName = test.MethodName;
        _fullName = test.FullName;
        Test = test;
        
    }
    
    public ContextTestInfo(TestContext.TestAdapter testAdapter)
    {
        _id = testAdapter.ID;
        _arguments = testAdapter.Arguments;
        _className = testAdapter.ClassName;
        _methodName = testAdapter.MethodName;
        _fullName = testAdapter.FullName;
    }
  

    public override bool Equals(object? obj)
    {
        if (obj is ContextTestInfo other)
        {
            return _id == other._id &&
                   _arguments.SequenceEqual(other._arguments) &&
                   _className == other._className &&
                   _methodName == other._methodName &&
                   _fullName == other._fullName;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_id, _arguments, _className, _methodName, _fullName);
    }
    
    public override string ToString()
    {
        return $"{_fullName}";
    }
}