using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DataPreparation.Testing
{
    internal static class BaseServiceCollectionForAssemblyStore
    {
        private static ConcurrentDictionary<Assembly, IServiceCollection> BaseDataCollection { get; } = new();
        
        public static IServiceCollection? GetBaseDataCollectionCopy(Assembly assembly)
        {
            if(BaseDataCollection.TryGetValue(assembly, out var serviceCollection))
            {
                var copyServiceCollection = new ServiceCollection();
                foreach (var service in serviceCollection)
                {
                    copyServiceCollection.Add(service);
                }
                return copyServiceCollection;
            }
            return null;
        }
      
        public static bool ContainsBaseDataCollection(Assembly assembly)
        {
            return BaseDataCollection.ContainsKey(assembly);
        }

        public static void AddDescriptor(Assembly typeAssembly, ServiceDescriptor serviceDescriptor)
        {
          
            if(BaseDataCollection.TryGetValue(typeAssembly, out var serviceCollection))
            {
                serviceCollection.Add(serviceDescriptor);
            }
        }

        public static void CreateBaseDataCollection(Assembly typeAssembly)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            BaseDataCollection.TryAdd(typeAssembly,serviceCollection);
        }
    }
}
