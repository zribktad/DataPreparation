using System.Reflection;
using DataPreparation.Data;
using DataPreparation.Data.Factory;
using DataPreparation.Data.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace DataPreparation.Testing
{
    internal class DataRegister(ILoggerFactory loggerFactory, Assembly typeAssembly)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<DataRegister>();
        private  void RegisterProcessors(List<Func<Type, bool>> processors, Type[] allTypes)
        { 
            _logger.LogDebug("Registering processors for {0} types", allTypes.Length);
            foreach (var type in allTypes)
            {
                foreach (var processor in processors)
                {
                    if (processor(type)) break;
                }
            }
        }

        internal IServiceCollection GetBaseDataServiceCollection()
        {
            lock (typeAssembly)
            {
                _logger.LogDebug("Analyzing process start for assembly {0}", typeAssembly.FullName);
                AnalyzeAssemblyProcessor();
                _logger.LogDebug("Analyzing process end for assembly {0}", typeAssembly.FullName);
            }

            return BaseServiceCollectionForAssemblyStore.GetBaseDataCollectionCopy(typeAssembly) ?? throw new InvalidOperationException();
        }

        private void AnalyzeAssemblyProcessor()
        {
            if (!BaseServiceCollectionForAssemblyStore.ContainsBaseDataCollection(typeAssembly))
            {
                BaseServiceCollectionForAssemblyStore.CreateBaseDataCollection(typeAssembly);
                _logger.LogDebug("Analyzing assembly {0}", typeAssembly.FullName);
                List<Func<Type, bool>> processors =
                [  
                    ProcessDataClassPreparation,
                    ProcessDataMethodPreparation,
                    ProcessFactories
                ];
                //RegisterService Data Preparation classes
                RegisterProcessors(processors,  typeAssembly.GetTypes());
                _logger.LogDebug("Assembly {0} analyzed", typeAssembly.FullName);
            }else
            {
                _logger.LogDebug("Assembly {0} already analyzed", typeAssembly.FullName);
            }
        }

        private bool ProcessFactories(Type type)
        {
            if (type.IsAssignableTo(typeof(IDataFactoryBase)) == false) return false;
            
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped;
            if(type.GetCustomAttribute<FactoryLifetimeAttribute>() is {} factoryLifetimeAttribute)
                serviceLifetime = factoryLifetimeAttribute.Lifetime;
            
            BaseServiceCollectionForAssemblyStore.AddDescriptor(type.Assembly,new ServiceDescriptor(type, type, serviceLifetime)); // add FactoryObjects
            return true;
        }
        

        private static bool ProcessDataMethodPreparation(Type type)
        {
            if ( type.GetCustomAttributes<PreparationMethodForAttribute>() is { } methodAttributes)
            {
                foreach (var methodAttribute in methodAttributes)
                {
                    var methodInfo = methodAttribute.MethodInfo;
                    DataRelationStore.SetMethodDataPreparationType(methodInfo,type);
                    BaseServiceCollectionForAssemblyStore.AddDescriptor(type.Assembly,new ServiceDescriptor(type, type, methodAttribute.Lifetime));
                    return true;
                }
            }
            return false;
        }

        private bool ProcessDataClassPreparation(Type type)
        {
            //RegisterService Data Preparation Classes
            if (type.GetCustomAttributes<PreparationClassForAttribute>() is { } classAttributes )
            {
                foreach (var attribute in classAttributes)
                {
                    var classType = attribute.ClassType;
                    DataRelationStore.SetClassDataPreparationType(classType, type);
                    BaseServiceCollectionForAssemblyStore.AddDescriptor(type.Assembly,new ServiceDescriptor(type, type, attribute.Lifetime));
                    return true;
                }
            }
            return false;
        }


     
    }
}
