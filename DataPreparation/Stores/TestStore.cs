using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing;

internal static class TestStore
{
    private static readonly ConcurrentDictionary<MethodBase, IServiceProvider> ProviderDictionary = new();

    private static void Register(MethodBase method, IServiceProvider serviceProvider)
    {
        ProviderDictionary[method] = serviceProvider;
    }

    public static IServiceProvider? GetRegistered(MethodBase method)
    {
        return ProviderDictionary.GetValueOrDefault(method);
    }


    internal static void RegisterDataCollection(MethodBase method, IServiceCollection serviceCollection)
    {
        var serviceProvider = serviceCollection.BuildServiceProvider();
        Register(method, serviceProvider);

    }

    public static object? GetTestCaseServiceData(MethodBase method, Type dataPreparationType)
    {
        if (dataPreparationType == null || method == null)
        {
            throw new Exception("Incorrect call for service");
        }
        var testProvider = GetRegistered(method);
        if (testProvider == null)
        {
            Console.WriteLine($"Service provider for test {method} not found.");
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