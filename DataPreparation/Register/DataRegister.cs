using System.Linq;
using System.Reflection;
using DataPreparation.Data;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    internal static class DataRegister
    {
       


        public static void RegisterDataCollection(ITest test, IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
            CaseProviderStore.Register(test.Fixture.GetType(), serviceProvider);

        }

        public static IDataPreparation? GetTestCaseServiceData(ITest test, Type dataPreparationType)
        {
            if (dataPreparationType == null || test == null)
            {
                throw new Exception("Incorrect call for service");
            }
            var testProvider = CaseProviderStore.GetRegistered(test.Fixture.GetType());
            if (testProvider == null)
            {
                Console.WriteLine($"Service provider for test {test.Fixture} not found.");
                return null;
            }

            if (testProvider.GetService(dataPreparationType) is not IDataPreparation dataPreparation)
            {
                Console.WriteLine($"Data preparation not found for {dataPreparationType.FullName} not found.");
                return null;
            }
            return dataPreparation;
           
        }


        private static bool registered = false;
        public static void  RegisterDataPreparation()
        {
            if(registered) return;
            registered=true;

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

            foreach (var type in allTypes)
            {
                if (type.GetCustomAttribute<DataClassPreparationForAttribute>() is { } classAttribute )
                {
                    var classType = classAttribute.ClassType;
                    DataTypeStore.SetClassDataPreparationType(classType, type);
                    BaseServiceCollectionStore.Base.Add(new ServiceDescriptor(type, type, classAttribute.Lifetime));
                }else if ( type.GetCustomAttribute<DataMethodPreparationForAttribute>() is { } methodAttribute)
                {
                    var methodInfo = methodAttribute.MethodInfo;
                    DataTypeStore.SetMethodDataPreparationType(methodInfo,type);

                    BaseServiceCollectionStore.Base.Add(new ServiceDescriptor(type, type, methodAttribute.Lifetime));
                }
                else
                {
                    TestAttributeStore.AddAttributes<UsePreparedDataAttribute>(type);
                    TestAttributeStore.AddAttributes<UsePreparedDataForAttribute>(type);
                }
            }
        }

        public static IServiceCollection GetBaseDataServiceCollection()
        {
            RegisterDataPreparation();
            IServiceCollection newServiceCollection = new ServiceCollection();
            foreach (var service in BaseServiceCollectionStore.Base)
            {
                newServiceCollection.Add(service);
            }
            return newServiceCollection;
        }

     
    }
}
