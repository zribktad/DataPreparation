using DataPreparation.Helpers;
using DataPreparation.Models.Data;
using DataPreparation.Testing;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Provider;

public abstract class PreparationTest
{
    public static TestStore CreateTestStore(TestInfo testInfo)
    {
        var testStore = Store.GetTestStore(testInfo);
        if(testStore != null)  return testStore;
      
        var loggerFactory = LoggerHelper.CreateOrNullLogger(testInfo.Test.Fixture?.GetType());

        var dataPreparationAttributes = AttributeHelper.GetAttributes(testInfo.Test.Method.MethodInfo, 
            typeof(UsePreparedAttribute));
        
        return Store.CreateTestStore(testInfo,loggerFactory,dataPreparationAttributes);
    }
    
    public static TestStore? RemoveTestStore(TestStore? testStore)
    {
        if (testStore != null)
        {
            testStore.SourceFactory.Dispose();
            return Store.RemoveTestStore(testStore.TestInfo);
        }
        return testStore;
    }

    private static FixtureInfo CreateTestInfo(ITest test, out TestInfo testInfo)
    {
        if(test.Parent == null )
        {
            throw new Exception("Test Fixture not found");
        }
            
        FixtureInfo fixtureInfo = new (test.Parent );
        testInfo = new (test,fixtureInfo);
        return fixtureInfo;
    }
}