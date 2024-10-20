using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    public static class DataRegister
    {
        #region providers for Test Cases

        private static readonly Dictionary<Type, IServiceProvider> providerDictionary = new();
        public static void Register<TDataCase>(IServiceProvider serviceProvider)
        {
            providerDictionary[typeof(TDataCase)] = serviceProvider;
        }
        public static void Register(Type type, IServiceProvider serviceProvider)
        {
            providerDictionary[type] = serviceProvider;
        }
        public static IServiceProvider? GetRegistered(Type testCase)
        {
            return providerDictionary.GetValueOrDefault(testCase);
        }


        #endregion

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
                if (type.GetCustomAttribute<DataPreparationForAttribute>() is { } classAttribute )
                {
                    var classType = classAttribute.ClassType;
                    ClassDataRegister[classType] = type;
                    baseServiceCollection.Add(new ServiceDescriptor(type, type, ServiceLifetime.Transient));
                }else if ( type.GetCustomAttribute<DataMethodPreparationForAttribute>() is { } methodAttribute)
                {
                    var methodInfo = methodAttribute.MethodInfo;
                    MethodDataRegister[methodInfo] = type;
                    baseServiceCollection.Add(new ServiceDescriptor(type, type, ServiceLifetime.Transient));
                }
            }
        }

        public static IServiceCollection GetServiceCollection()
        {
            IServiceCollection newServiceCollection = new ServiceCollection();
            foreach (var service in baseServiceCollection)
            {
                newServiceCollection.Add(service);
            }
            return newServiceCollection;
        }

        public static Type? GetClassDataPreparationType(Type classType)
        {
            return ClassDataRegister.GetValueOrDefault(classType);
        }
        public static Type? GetMethodDataPreparationType(MethodInfo methodInfo)
        {
            return MethodDataRegister.GetValueOrDefault(methodInfo);
        }


        private static readonly IServiceCollection baseServiceCollection = new ServiceCollection();
        private static readonly Dictionary<Type, Type> ClassDataRegister = new();
        private static readonly Dictionary<MethodInfo, Type> MethodDataRegister = new();

        public static object GetTestCaseService(ITest test, Type dataPreparationClassType)
        {
            if (dataPreparationClassType == null || test == null)
            {
                throw new Exception("Incorrect call for service");
            }
            var testProvider = GetRegistered(test.Fixture.GetType());
            if (testProvider == null)
            {
                //TODO  
                return null;
            }

            return testProvider.GetService(dataPreparationClassType);
        }
    }
}
