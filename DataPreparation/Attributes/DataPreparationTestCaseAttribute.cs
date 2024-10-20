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
            IServiceCollection serviceCollection2 = new ServiceCollection();

            IServiceCollection serviceCollection = new ServiceCollection();
            if (test.TypeInfo.Type.GetInterface(nameof(IDataPreparationTestCase)) != null)
            {
                var dataPreparationTestCase = (IDataPreparationTestCase)Activator.CreateInstance(test.TypeInfo.Type);
                dataPreparationTestCase.DataPreparationServices(serviceCollection);
                
            }


            var typesWithAttribute = allTypes.Where(type =>
                type.GetCustomAttributes(typeof(DataPreparationForAttribute), true).Any() ||
                type.GetCustomAttributes(typeof(DataMethodPreparationForAttribute), true).Any());

            ServicesProviderRegister.RegisterDataPreparation();
           // ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            //ServicesProviderRegister.Register(test.TypeInfo.Type, serviceProvider);

        }

        public void AfterTest(ITest test)
        {
         
        }

        public ActionTargets Targets =>  ActionTargets.Suite ;


    }
}
