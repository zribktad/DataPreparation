using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPreparation.Testing.Register
{
    public static class ServicesProviderRegister
    {

        private static readonly Dictionary<Type, IServiceProvider> providerDictionary = new();

        public static void Register<TDataCase>(IServiceProvider serviceProvider)
        {
            providerDictionary[typeof(TDataCase)] = serviceProvider;
        }
        public static void Register(Type type,IServiceProvider serviceProvider)
        {
            providerDictionary[type] = serviceProvider;
        }



        public static IServiceProvider? GetRegistered(Type classType)
        {
            return providerDictionary.GetValueOrDefault(classType);
        }
    }
}
