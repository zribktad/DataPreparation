using DataPreparation.Data;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Interfaces;
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


        public static void RegisterDataCollection(ITest test, IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Register(test.Fixture.GetType(), serviceProvider);

        }

        public static IDataPreparation? GetTestCaseServiceData(ITest test, Type dataPreparationType)
        {
            if (dataPreparationType == null || test == null)
            {
                throw new Exception("Incorrect call for service");
            }
            var testProvider = GetRegistered(test.Fixture.GetType());
            if (testProvider == null)
            {
                Console.WriteLine($"Service provider for test {test.Fixture} not found.");
                return null;
            }

            if (testProvider.GetService(dataPreparationType) is not IDataPreparation dataPreparation)
            {
                Console.WriteLine($"Data preparation not found for {dataPreparationType.FullName} not found.");
                return null;
            }
            return dataPreparation;

        }

    }
}
