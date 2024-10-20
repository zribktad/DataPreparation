using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Testing
{
    [AttributeUsage(AttributeTargets.Class,Inherited = false)]
    public class DataPreparationTestCaseAttribute : Attribute, ITestAction
    {
        // Constructor for the attribute
        public DataPreparationTestCaseAttribute()
        {


        }

        public void BeforeTest(ITest test)
        {

            DataRegister.RegisterDataPreparation();

            IServiceCollection serviceCollection = DataRegister.GetServiceCollection();
            if (test.TypeInfo?.Type.GetInterface(nameof(IDataPreparationTestCase)) != null)
            {
                var dataTestCase = Activator.CreateInstance(test.TypeInfo.Type);
                if(dataTestCase is IDataPreparationTestCase dataPreparationTestCase)
                {
                    dataPreparationTestCase.DataPreparationServices(serviceCollection);
                }
            }

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
           // DataRegister.Register(test.TypeInfo.Type, serviceProvider);

        }

        public void AfterTest(ITest test)
        {
         
        }

        public ActionTargets Targets =>  ActionTargets.Suite ;


    }
}
