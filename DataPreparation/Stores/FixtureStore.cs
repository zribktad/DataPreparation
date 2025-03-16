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

public class FixtureStore(
    FixtureInfo fixtureInfo,
    ILoggerFactory loggerFactoryFactory,
    IServiceCollection serviceCollection)
{
    public FixtureInfo FixtureInfo { get; init; } = fixtureInfo;

    private readonly ConcurrentDictionary<ContextTestInfo, TestStore> _testData = new ();
    public ILoggerFactory LoggerFactory { get; } = loggerFactoryFactory;

    internal bool CreateTestStore(TestInfo testInfo, ILoggerFactory loggerFactory,IList<Attribute> dataPreparationAttributes)
    {
       return _testData.TryAdd(testInfo, new(testInfo,loggerFactory, serviceCollection,dataPreparationAttributes));
    }
    
    internal  TestStore? RemoveTestStore(TestInfo testInfo)
    {
        return _testData.Remove(testInfo, out var data) ? data : null;
    }

    internal TestStore? GetTestStore(ContextTestInfo testInfo) => _testData.GetValueOrDefault(testInfo, null);
    
}