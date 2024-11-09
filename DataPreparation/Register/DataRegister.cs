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
       

        private static bool _registered = false;
        //TODO only assembly for users
        private static void  RegisterDataPreparation()
        {
            if(_registered) return;
            _registered=true;

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
                else if(type.GetCustomAttribute<DataPreparationTestCaseAttribute>() is { } )
                {
                    
                    foreach (var testMethod in type.GetMethods())
                    {
                        TestAttributeStore.AddAttributes<UsePreparedDataAttribute>(testMethod);
                        TestAttributeStore.AddAttributes<UsePreparedDataForAttribute>(testMethod);
                    }
                    
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
