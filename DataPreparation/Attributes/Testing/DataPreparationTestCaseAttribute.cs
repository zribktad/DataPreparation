using DataPreparation.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    [AttributeUsage(AttributeTargets.Class,Inherited = false)]
    public class DataPreparationTestCaseAttribute : NUnitAttribute, ITestAction
    {
        // Constructor for the attribute
        public DataPreparationTestCaseAttribute()
        {


        }

        public void BeforeTest(ITest test)
        {
            
            IServiceCollection serviceCollection = DataRegister.GetBaseDataServiceCollection();
            if (test.TypeInfo.Type.GetInterface(nameof(ITestCaseServicesDataPreparation)) != null)
            {
                var dataPreparationTestCase = (ITestCaseServicesDataPreparation)Activator.CreateInstance(test.TypeInfo.Type);
                dataPreparationTestCase.DataPreparationServices(serviceCollection);
                
            }

            CaseProviderStore.RegisterDataCollection(test,serviceCollection);


        }

        public void AfterTest(ITest test)
        {
         
        }

        public ActionTargets Targets =>  ActionTargets.Suite ;


    }
}
