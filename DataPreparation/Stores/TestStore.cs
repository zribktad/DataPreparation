using System.Collections.Concurrent;
using System.Reflection;
using DataPreparation.Factory.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing;

public class TestStore
{
    private static readonly ConcurrentDictionary<MethodBase, IServiceProvider> TestProviders = new();
    private static readonly ConcurrentDictionary<MethodBase, ISourceFactory> TestSourceFactories = new();
    private static readonly ConcurrentDictionary<MethodBase, ILoggerFactory> TestLoggerFactories = new();

    #region SourceFactory
    
    internal static ISourceFactory GetOrCreateFactory(MethodBase method) => TestSourceFactories.GetOrAdd(method,Create);

    internal static ISourceFactory? DeleteFactory(MethodBase method)
    {
        
        TestSourceFactories.TryRemove(method, out var factory);
        factory?.Dispose();
        return factory;
    }

    private static ISourceFactory Create(MethodBase method)
    {
        
        IServiceProvider serviceProvider = GetRegistered(method) ?? throw new InvalidOperationException($"No service provider found for {method}.");
        ILoggerFactory loggerFactory = GetLoggerFactory((MethodInfo)method) ??  NullLoggerFactory.Instance;
        return new SourceFactory(serviceProvider,loggerFactory.CreateLogger<ISourceFactory>());
    }

    #endregion
    #region Provider
    
    private static bool Register(MethodBase method, IServiceProvider serviceProvider) => TestProviders.TryAdd(method, serviceProvider);
    
    internal static IServiceProvider? DeleteProvider(MethodBase method)
    {
        TestProviders.TryRemove(method, out var provider);
        return provider;
    }

    internal static IServiceProvider? GetRegistered(MethodBase method) => TestProviders.GetValueOrDefault(method);


    internal static bool RegisterDataCollection(MethodBase method, IServiceCollection serviceCollection) => Register(method, serviceCollection.BuildServiceProvider());

    internal static object? GetTestFixtureServiceData(MethodBase method, Type dataPreparationType)
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

    #region LoggerFactory

    public static bool RegisterLoggerFactory(MethodInfo methodMethodInfo, ILoggerFactory loggerFactory)
    {
      return  TestLoggerFactories.TryAdd(methodMethodInfo, loggerFactory);
    }
    public static ILoggerFactory? GetLoggerFactory(MethodInfo methodMethodInfo)
    {
        TestLoggerFactories.TryGetValue(methodMethodInfo, out var loggerFactory);
        return loggerFactory;
    }

    #endregion

    
}