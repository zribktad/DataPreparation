using DataPreparation.Data;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Execution;

namespace DataPreparation.Testing;

public class TestInfo :ContextTestInfo
{
    
   
    public readonly IMethodInfo Method;
    public readonly FixtureInfo FixtureInfo;
    public readonly ITest Test; 
    
    internal TestInfo(ITest test,FixtureInfo fixtureInfo) : base(test)
    {
        Test = test;
        Method = test.Method;
        FixtureInfo = fixtureInfo;

    }
    
    internal static TestInfo CreateTestInfo(ITest test)
    {
        var start_test = test;
        if(test.Parent == null )
        {
            throw new Exception("Test Fixture not found");
        }
        while (test.Parent is not TestFixture)
        {
            test = test.Parent;
        }
        FixtureInfo fixtureInfo = new (test.Parent);
        TestInfo testInfo = new (start_test,fixtureInfo);
 
        return testInfo;
    }
    
}