using System.Reflection;
using DataPreparation.Data;
using DataPreparation.Data.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace DataPreparation.Testing
{
    internal class DataRegister
    {
        private ILogger _logger;

        public DataRegister(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DataRegister>();;
        }

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

        internal IServiceCollection GetBaseDataServiceCollection(Assembly assembly)
        {
            _logger.LogDebug("Analyzing process start for assembly {0}", assembly.FullName);
            AnalyzeAssemblyProcessor(assembly);
            _logger.LogDebug("Analyzing process end for assembly {0}", assembly.FullName);
        
            return BaseServiceCollectionForAssemblyStore.GetBaseDataCollectionCopy(assembly) ?? throw new InvalidOperationException();
        }

        private void AnalyzeAssemblyProcessor(Assembly assembly)
        {
            lock (assembly)
            {
                if (!BaseServiceCollectionForAssemblyStore.ContainsBaseDataCollection(assembly))
                {
                    _logger.LogDebug("Analyzing assembly {0}", assembly.FullName);
                    List<Func<Type, bool>> processors =
                    [  
                        ProcessDataClassPreparation,
                        ProcessDataMethodPreparation,
                        ProcessFactories
                    ];
                    //RegisterService Data Preparation classes
                    RegisterProcessors(processors,  assembly.GetTypes());
                    _logger.LogDebug("Assembly {0} analyzed", assembly.FullName);
                }else
                {
                    _logger.LogDebug("Assembly {0} already analyzed", assembly.FullName);
                }
            }
        }

        private bool ProcessFactories(Type type)
        {
            if (type.IsAssignableTo(typeof(IDataFactoryBase)) == false) return false;
            
            BaseServiceCollectionForAssemblyStore.AddDescriptor(type.Assembly,new ServiceDescriptor(type, type, ServiceLifetime.Singleton)); // add FactoryObjects
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
             
                return true;
            }
            return false;
        }


     
    }
}
