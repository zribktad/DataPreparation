using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPreparation.Testing
{
    internal  static class CaseProviderStore
    {

        private static readonly Dictionary<Type, IServiceProvider> providerDictionary = new();

        public static void Register(Type type, IServiceProvider serviceProvider)
        {
            providerDictionary[type] = serviceProvider;
        }
        public static IServiceProvider? GetRegistered(Type testCase)
        {
            return providerDictionary.GetValueOrDefault(testCase);
        }
    }
}
