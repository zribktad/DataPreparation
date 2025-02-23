using System.Reflection;
using DataPreparation.Data;
using DataPreparation.Data.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Testing
{
    internal static class DataRegister
    {
        private static void RegisterProcessors(List<Func<Type, bool>> processors, Type[] allTypes)
        {
            foreach (var type in allTypes)
            {
                foreach (var processor in processors)
                {
                    if (processor(type)) break;
                }
            }
        }

        public static IServiceCollection GetBaseDataServiceCollection(Assembly assembly)
        {
            lock (assembly)
            {
                if (!BaseServiceCollectionStore.ContainsBaseDataCollection(assembly))
                {
                    List<Func<Type, bool>> processors =
                    [  
                        ProcessDataClassPreparation,
                        ProcessDataMethodPreparation,
                        ProcessDataPreparationTestFixtures,
                        ProcessFactories
                    ];
                    //Register Data Preparation classes
                    RegisterProcessors(processors,  assembly.GetTypes());
                }
            }
            
            return BaseServiceCollectionStore.GetBaseDataCollectionCopy(assembly) ?? throw new InvalidOperationException();
        }
        
        private static bool ProcessFactories(Type type)
        {
            if (type.IsAssignableTo(typeof(IDataFactoryBase)) == false) return false;
            
            BaseServiceCollectionStore.AddDescriptor(type.Assembly,new ServiceDescriptor(type, type, ServiceLifetime.Singleton)); // add Factory
            return true;
        }

        private static bool ProcessDataPreparationTestFixtures(Type type)
        {
            if(type.GetCustomAttribute<DataPreparationTestFixtureAttribute>() is { } )
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
                BaseServiceCollectionStore.AddDescriptor(type.Assembly,new ServiceDescriptor(type, type, methodAttribute.Lifetime));
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
                BaseServiceCollectionStore.AddDescriptor(type.Assembly,new ServiceDescriptor(type, type, classAttribute.Lifetime));
                return true;
            }
            return false;
        }


     
    }
}
