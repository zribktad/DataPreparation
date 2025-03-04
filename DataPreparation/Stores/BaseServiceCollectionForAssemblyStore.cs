using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DataPreparation.Testing
{
    internal  static class BaseServiceCollectionForAssemblyStore
    {
        private static ConcurrentDictionary<Assembly, IServiceCollection> BaseDataCollection { get; } = new();

        private static void  AddBaseDataCollection(Assembly assembly, IServiceCollection serviceCollection)
        {
            BaseDataCollection.TryAdd(assembly,serviceCollection);
        }

        public static IServiceCollection? GetBaseDataCollectionCopy(Assembly assembly)
        {
            
            var serviceCollection = BaseDataCollection.GetValueOrDefault(assembly);
            if (serviceCollection == null)
            {
                return null;
            }
            var copyServiceCollection = new ServiceCollection();
            foreach (var service in serviceCollection)
            {
                copyServiceCollection.Add(service);
            }
            return copyServiceCollection;
        }
      
        public static bool ContainsBaseDataCollection(Assembly assembly)
        {
            return BaseDataCollection.ContainsKey(assembly);
        }

        public static void AddDescriptor(Assembly typeAssembly, ServiceDescriptor serviceDescriptor)
        {
            if(ContainsBaseDataCollection(typeAssembly))
            {
                var serviceCollection = BaseDataCollection.GetValueOrDefault(typeAssembly);
                serviceCollection.Add(serviceDescriptor);
            }else
            {
                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.Add(serviceDescriptor);
                AddBaseDataCollection(typeAssembly, serviceCollection);
            }
        }
    }
}
