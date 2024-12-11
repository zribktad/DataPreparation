using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Analyzers.Test;


public class DataPreparationAutoAnalyzeAttribute : Attribute,ITestAction
{
    public void BeforeTest(ITest test)
    {
        var t = AnalyzerStore.AddOrGetAnalyzeMethodData(test.Fixture.GetType(),test.Method.MethodInfo);
    }

    public void AfterTest(ITest test)
    {
     
    }

    public ActionTargets Targets => ActionTargets.Test;
}