using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataPreparation.Testing
{
    internal  static class BaseServiceCollectionStore
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
            IServiceCollection copyServiceCollection = new ServiceCollection();
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
