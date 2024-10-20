using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Microsoft.Extensions.DependencyInjection;
using DataPreparation.Testing.Register;

namespace DataPreparation.Testing
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false,Inherited = false)]
    public class DataPreparationTestCaseAttribute : Attribute, ITestAction
    {
        // Constructor for the attribute
        public DataPreparationTestCaseAttribute()
        {


        }

        public void BeforeTest(ITest test)
        {
            if (test.TypeInfo.Type.GetInterface(nameof(IDataPreparationTestCase)) != null)
            {
                var dataPreparationTestCase = (IDataPreparationTestCase)Activator.CreateInstance(test.TypeInfo.Type);
                IServiceCollection serviceCollection = new ServiceCollection();
                dataPreparationTestCase.DataPreparationServices(serviceCollection);
                ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                ServicesProviderRegister.Register(test.TypeInfo.Type,serviceProvider);

            }
            DataPreparationRegister.RegisterFromAttributes();
        }

        public void AfterTest(ITest test)
        {
         
        }

        public ActionTargets Targets =>  ActionTargets.Suite ;


    }
}
