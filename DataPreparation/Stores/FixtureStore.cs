using System.Collections.Concurrent;
using System.Reflection;
using DataPreparation.Data;
using DataPreparation.Factory.Testing;
using DataPreparation.Models.Data;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DataPreparation.Testing;

public class FixtureStore
{
    public FixtureInfo _fixtureInfo { get; init; }
    
    private readonly ConcurrentDictionary<ContextTestInfo, TestStore> _testData = new();
    private readonly IServiceCollection _serviceCollection;
    public ILoggerFactory LoggerFactory { get; }
    public FixtureStore(FixtureInfo fixtureInfo, ILoggerFactory loggerFactoryFactory, IServiceCollection serviceCollection)
    {
        _fixtureInfo = fixtureInfo;
        _serviceCollection = serviceCollection;
        LoggerFactory = loggerFactoryFactory;
    }
    
    
    public bool CreateTestStore(TestInfo testInfo, ILoggerFactory loggerFactory,IList<Attribute> dataPreparationAttributes)
    {
       return _testData.TryAdd(testInfo, new(testInfo,loggerFactory, _serviceCollection,dataPreparationAttributes));
    }
    
    internal  TestStore? RemoveTestStore(TestInfo testInfo)
    {
        return _testData.Remove(testInfo, out var data) ? data : null;
    }

    internal TestStore? GetTestStore(ContextTestInfo testInfo) => _testData.GetValueOrDefault(testInfo, null);
    //
    // private static readonly ConcurrentDictionary<MethodBase, IServiceProvider> TestProviders = new();
    // private static readonly ConcurrentDictionary<MethodBase, ISourceFactory> TestSourceFactories = new();
    // private static readonly ConcurrentDictionary<MethodBase, ILoggerFactory> TestLoggerFactories = new();
    //
    // #region SourceFactory
    //
    // internal  ISourceFactory GetOrCreateFactory(MethodBase method) => TestSourceFactories.GetOrAdd(method,Create);
    //
    // internal  ISourceFactory? DeleteFactory(MethodBase method)
    // {
    //     
    //     TestSourceFactories.TryRemove(method, out var factory);
    //     factory?.Dispose();
    //     return factory;
    // }
    //
    // private  ISourceFactory Create(MethodBase method)
    // {
    //     
    //     IServiceProvider serviceProvider = GetRegistered(method) ?? throw new InvalidOperationException($"No service provider found for {method}.");
    //     ILoggerFactory loggerFactory = GetLoggerFactory((MethodInfo)method) ??  NullLoggerFactory.Instance;
    //     return new SourceFactory(serviceProvider,loggerFactory.CreateLogger<ISourceFactory>());
    // }
    //
    // #endregion
    // #region Provider
    //
    // private  bool Register(MethodBase method, IServiceProvider serviceProvider) => TestProviders.TryAdd(method, serviceProvider);
    //
    // internal  IServiceProvider? DeleteProvider(MethodBase method)
    // {
    //     TestProviders.TryRemove(method, out var provider);
    //     return provider;
    // }
    //
    // internal  IServiceProvider? GetRegistered(MethodBase method) => TestProviders.GetValueOrDefault(method);
    //
    //
    // internal  bool RegisterDataCollection(MethodBase method, IServiceCollection serviceCollection) => Register(method, serviceCollection.BuildServiceProvider());
    //
    // internal  object? GetTestFixtureServiceData(MethodBase method, Type dataPreparationType)
    // {
    //     if (dataPreparationType == null || method == null)
    //     {
    //         throw new Exception("Incorrect call for service");
    //     }
    //     var testProvider = GetRegistered(method);
    //     if (testProvider == null)
    //     {
    //         Console.WriteLine($"Service provider for test {method} not found.");
    //         return null;
    //     }
    //
    //     if (testProvider.GetService(dataPreparationType) is var preparedData && preparedData == null)
    //     {
    //         Console.WriteLine($"Data preparation not found for {dataPreparationType.FullName}.");
    //         return null;
    //     }
    //     
    //     return preparedData;
    //
    // }
    // #endregion
    //
    // #region LoggerFactory
    //
    // private static bool RegisterLoggerFactory(MethodInfo methodMethodInfo, ILoggerFactory loggerFactory)
    // {
    //   return  TestLoggerFactories.TryAdd(methodMethodInfo, loggerFactory);
    // }
    // private static ILoggerFactory? GetLoggerFactory(MethodInfo methodMethodInfo)
    // {
    //     TestLoggerFactories.TryGetValue(methodMethodInfo, out var loggerFactory);
    //     return loggerFactory;
    // }
    //
    // #endregion


    
}