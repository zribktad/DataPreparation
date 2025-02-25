using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Analyzers.Test;


public class DataPreparationAutoAnalyzeAttribute : Attribute,ITestAction
{
    public void BeforeTest(ITest test)
    {
        
       // MethodAnalyzer.AnalyzeTestMethod(test.Fixture.GetType(),test.Method.MethodInfo);//TODO
      
    }

    public void AfterTest(ITest test)
    {
     
    }

    public ActionTargets Targets => ActionTargets.Test;
}