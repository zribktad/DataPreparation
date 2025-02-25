using DataPreparation.Data;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Execution;

namespace DataPreparation.Testing;

public class TestInfo :ContextTestInfo
{
    
   
    private readonly IMethodInfo _method;
    public readonly FixtureInfo FixtureInfo;
    public readonly ITest Test; 
    
    internal TestInfo(ITest test,FixtureInfo fixtureInfo) : base(test)
    {
        Test = test;
        _method = test.Method;
        FixtureInfo = fixtureInfo;

    }
    
    internal static TestInfo CreateTestInfo(ITest test)
    {
        if(test.Parent == null )
        {
            throw new Exception("Test Fixture not found");
        }
            
        FixtureInfo fixtureInfo = new (test.Parent );
        TestInfo testInfo = new (test,fixtureInfo);
        return testInfo;
    }
 
    
}