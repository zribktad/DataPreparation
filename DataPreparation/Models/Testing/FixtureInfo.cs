using DataPreparation.Data;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Execution;

namespace DataPreparation.Testing;

public class FixtureInfo: ContextTestInfo
{
    
    public FixtureInfo(ITest test): base(test)
    {

    }
    
    
 
}