using DataPreparation.Data;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Provides a store for service providers associated with test cases.
    /// </summary>
    public  static class FixtureProviderStoreOff
    {
        //Store Service Provider for each test case
        private static readonly ConcurrentDictionary<Type, IServiceProvider> ProviderDictionary = new();

        private static void Register(Type type, IServiceProvider serviceProvider)
        {
            ProviderDictionary[type] = serviceProvider;
        }
        public static IServiceProvider? GetRegistered(Type testFixture)
        {
            return ProviderDictionary.GetValueOrDefault(testFixture);
        }


        internal static void RegisterDataCollection(Type testFixtureType, IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Register(testFixtureType, serviceProvider);

        }

        public static object? GetTestFixtureServiceData(ITest test, Type dataPreparationType)
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

            if (testProvider.GetService(dataPreparationType) is var preparedData && preparedData == null)
            {
                Console.WriteLine($"Data preparation not found for {dataPreparationType.FullName}.");
                return null;
            }
        
            return preparedData;

        }

    }
}
