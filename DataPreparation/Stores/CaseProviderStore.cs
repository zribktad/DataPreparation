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
    /// <summary>
    /// Provides a store for service providers associated with test cases.
    /// </summary>
    internal  static class CaseProviderStore
    {
        //Store Service Provider for each test case
        private static readonly Dictionary<Type, IServiceProvider> providerDictionary = new();

        private static void Register(Type type, IServiceProvider serviceProvider)
        {
            providerDictionary[type] = serviceProvider;
        }
        public static IServiceProvider? GetRegistered(Type testCase)
        {
            return providerDictionary.GetValueOrDefault(testCase);
        }


        internal static void RegisterDataCollection(Type testCaseType, IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Register(testCaseType, serviceProvider);

        }

        public static object? GetTestCaseServiceData(ITest test, Type dataPreparationType)
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
