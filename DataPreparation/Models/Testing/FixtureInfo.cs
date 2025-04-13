using DataPreparation.Data;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Execution;

namespace DataPreparation.Testing;

public class FixtureInfo: ContextTestInfo
{
    public readonly Type Type;
    public readonly object Instance;
    public FixtureInfo(ITest test, object? testFixture): base(test)
    {
        Type = test.TypeInfo?.Type ?? throw new Exception("Test Fixture not found");
        Instance = testFixture ;
    }
    
    
 
}