using System.Linq;
using System.Reflection;
using DataPreparation.Data;
using DataPreparation.Data.Setup;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    internal static class DataRegister
    {
       

        private static bool _registered = false;
        private static readonly object Lock = new object();
        //TODO only assembly for users
        private static void  RegisterDataPreparation( List<Func<Type, bool> > processors)
        {
            lock (Lock)
            {
                if (_registered) return;
                _registered = true;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = new List<Type>();
            //Load all types from assemblies
            foreach (var assembly in assemblies)
            {
                try
                {
                    //Get all types from assembly (classes, interfaces, structs, enums)
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
            //Register Data Preparation classes
            foreach (var type in allTypes)
            {
                foreach (var processor in processors)
                {
                    if (processor(type)) break;
                }
            }
        }
        public static IServiceCollection GetBaseDataServiceCollection()
        {
            
            List<Func<Type, bool> > processors = new List<Func<Type, bool>>()
            {
                ProcessDataClassPreparation,
                ProcessDataMethodPreparation,
                ProcessDataPreparationTestCases,
                ProcessFactories
            };
            
            RegisterDataPreparation(processors);
            IServiceCollection newServiceCollection = new ServiceCollection();
            foreach (var service in BaseServiceCollectionStore.Base)
            {
                newServiceCollection.Add(service);
            }
            return newServiceCollection;
        }
        
        private static bool ProcessFactories(Type type)
        {
            if (type.GetInterface(nameof(IDataFactory)) == null) return false;
            
            BaseServiceCollectionStore.Base.Add(new ServiceDescriptor(type, type, ServiceLifetime.Singleton));
            return true;
        }

        private static bool ProcessDataPreparationTestCases(Type type)
        {
            if(type.GetCustomAttribute<DataPreparationTestCaseAttribute>() is { } )
            {
                //Register method/class data preparation
                foreach (var testMethod in type.GetMethods())
                {
                    TestAttributeCountStore.AddAttributes(testMethod);
                       
                }
                return true;
            }
            return false;
        }

        private static bool ProcessDataMethodPreparation(Type type)
        {
            if ( type.GetCustomAttribute<DataMethodPreparationForAttribute>() is { } methodAttribute)
            {
                var methodInfo = methodAttribute.MethodInfo;
                DataTypeStore.SetMethodDataPreparationType(methodInfo,type);
                BaseServiceCollectionStore.Base.Add(new ServiceDescriptor(type, type, methodAttribute.Lifetime));
                return true;
            }
            return false;
        }

        private static bool ProcessDataClassPreparation(Type type)
        {
            //Register Data Preparation Classes
            if (type.GetCustomAttribute<DataClassPreparationForAttribute>() is { } classAttribute )
            {
                var classType = classAttribute.ClassType;
                DataTypeStore.SetClassDataPreparationType(classType, type);
                BaseServiceCollectionStore.Base.Add(new ServiceDescriptor(type, type, classAttribute.Lifetime));
                return true;
            }
            return false;
        }


     
    }
}
