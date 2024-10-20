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
            

            IServiceCollection serviceCollection = new ServiceCollection();
            if (test.TypeInfo.Type.GetInterface(nameof(IDataPreparationTestCase)) != null)
            {
                var dataPreparationTestCase = (IDataPreparationTestCase)Activator.CreateInstance(test.TypeInfo.Type);
                dataPreparationTestCase.DataPreparationServices(serviceCollection);
                
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    allTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine($"Warning: Unable to load types from assembly {assembly.FullName}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Unable to load assembly {assembly.FullName}: {ex.Message}");
                }
            }

            var typesWithAttribute = allTypes.Where(type =>
                type.GetCustomAttributes(typeof(DataPreparationForAttribute), true).Any() ||
                type.GetCustomAttributes(typeof(DataMethodPreparationForAttribute), true).Any());

            
           // ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            //DataRegister.Register(test.TypeInfo.Type, serviceProvider);

        }

        public void AfterTest(ITest test)
        {
         
        }

        public ActionTargets Targets =>  ActionTargets.Suite ;


    }
}
