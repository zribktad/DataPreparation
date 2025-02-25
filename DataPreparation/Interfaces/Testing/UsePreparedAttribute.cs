using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing;

public abstract class UsePreparedAttribute : NUnitAttribute, ITestAction
{
    public abstract void AfterTest(ITest test);
    public abstract void BeforeTest(ITest test);
    public abstract ActionTargets Targets { get; }
}