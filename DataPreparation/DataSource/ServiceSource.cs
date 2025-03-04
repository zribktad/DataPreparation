using System.Collections;
using DataPreparation.Provider;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    internal class ServiceSource:NUnitAttribute, IParameterDataSource
    {
        public IEnumerable GetData(IParameterInfo parameter)
        {
            var provider = PreparationContext.GetProvider();
            
            yield return provider;
        }
    }
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    internal class FactorySourceAttribute : NUnitAttribute, IParameterDataSource
    {
        public FactorySourceAttribute()
        {
           
        }
        public IEnumerable GetData(IParameterInfo parameter)
        {


            // TestInfo testInfo = null;
            //
            // PreparationTest.CreateTestStore(testInfo);
            yield return null;
            yield return null;
           // yield return PreparationContext.GetFactory();;
        }
    }
}
