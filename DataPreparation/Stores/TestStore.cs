using System.Collections.Concurrent;
using System.Reflection;
using DataPreparation.Factory.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing;

internal static class TestStore
{
    private static readonly ConcurrentDictionary<MethodBase, IServiceProvider> TestProviders = new();
    private static readonly ConcurrentDictionary<MethodBase, ISourceFactory> TestSourceFactories = new();

    #region SourceFactory
    
    public static ISourceFactory GetFactory(MethodBase method) => TestSourceFactories.GetOrAdd(method,Create(method));

    internal static ISourceFactory? DeleteFactory(MethodBase method)
    {
        TestSourceFactories.TryRemove(method, out var factory);
        return factory;
    }

    private static SourceFactory Create(MethodBase method)
    {
        IServiceProvider serviceProvider = GetRegistered(method) ?? throw new InvalidOperationException($"No service provider found for {method}.");
        return new(serviceProvider);
    }

    #endregion
    #region Provider
    
    private static bool Register(MethodBase method, IServiceProvider serviceProvider) => TestProviders.TryAdd(method, serviceProvider);
    
    internal static IServiceProvider? DeleteProvider(MethodBase method)
    {
        TestProviders.TryRemove(method, out var provider);
        return provider;
    }

    public static IServiceProvider? GetRegistered(MethodBase method) => TestProviders.GetValueOrDefault(method);


    internal static bool RegisterDataCollection(MethodBase method, IServiceCollection serviceCollection) => Register(method, serviceCollection.BuildServiceProvider());

    public static object? GetTestFixtureServiceData(MethodBase method, Type dataPreparationType)
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
    #endregion
}