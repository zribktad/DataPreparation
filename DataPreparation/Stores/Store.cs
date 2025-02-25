using DataPreparation.Data;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataPreparation.Helpers;
using DataPreparation.Models.Data;
using Microsoft.Extensions.Logging;

namespace DataPreparation.Testing
{

    internal abstract class Store
    {
        //Store FixtureStore for each test fixture
        private static readonly ConcurrentDictionary<FixtureInfo, FixtureStore> _fixtureStores = new();
        
        internal static bool CreateFixtureStore(FixtureInfo fixtureInfo,ILoggerFactory loggerFactory,IServiceCollection serviceCollection)
        {
            loggerFactory.CreateLogger<Store>().LogDebug("Creating FixtureStore for {0}", fixtureInfo);
            var ret =  _fixtureStores.TryAdd(fixtureInfo, new(fixtureInfo,loggerFactory,serviceCollection));
            loggerFactory.CreateLogger<Store>().LogDebug("FixtureStore for {0} created", fixtureInfo);
            return ret;
        }
        internal static FixtureStore GetFixtureStore(FixtureInfo fixtureInfo)
        {
            return _fixtureStores[fixtureInfo];
        }
        
        internal static bool RemoveFixtureStore(FixtureInfo fixtureInfo)
        {
            return _fixtureStores.TryRemove(fixtureInfo, out _);
        }
        
        #region PreparedData
        internal static TestStore CreateTestStore(TestInfo testContextTestInfo, ILoggerFactory loggerFactory,IList<Attribute> dataPreparationAttributes)
        {
            var testLogger = loggerFactory.CreateLogger<Store>();
            testLogger.LogDebug("Test data initialization for {0} started", testContextTestInfo);

            if (_fixtureStores.TryGetValue(testContextTestInfo.FixtureInfo, out var fixtureStore))
            {
                var fixtureLogger = fixtureStore.LoggerFactory.CreateLogger<Store>();
                fixtureLogger.LogDebug("Test data initialization for {0} started", testContextTestInfo);

                if (!fixtureStore.CreateTestStore(testContextTestInfo, loggerFactory,dataPreparationAttributes))
                {
                    LoggerHelper.Log(logger => logger.LogDebug("Test data initialization for {0} already exists", testContextTestInfo),
                        fixtureLogger,testLogger);
                }

                fixtureLogger.LogDebug("Test data initialization for {0} ended", testContextTestInfo);
            }
            else
            {
                testLogger.LogError("No {0} found for {1}.", typeof(DataPreparationFixtureAttribute), testContextTestInfo.FixtureInfo);
                throw new InvalidOperationException($"No {typeof(DataPreparationFixtureAttribute)} found for { testContextTestInfo.FixtureInfo}.");
            }
            LoggerHelper.Log(logger => logger.LogDebug("Test data initialization for {0} ended", testContextTestInfo), 
                fixtureStore.LoggerFactory.CreateLogger<Store>(),testLogger);
  
            return GetTestStore(testContextTestInfo)!;
        }

        internal static TestStore? GetTestStore(ContextTestInfo testInfo)
        {
            foreach (var fixtureStore in _fixtureStores.Values)
            {
                var testStore = fixtureStore.GetTestStore(testInfo);
                if (testStore !=  null)
                {
                    return testStore;
                }
            }

            throw new InvalidOperationException($"No {typeof(DataPreparationFixtureAttribute)} found for {testInfo}.");
        }
        internal static TestStore? GetTestStore(TestInfo testInfo)
        {
            var store =  GetFixtureStore(testInfo.FixtureInfo).GetTestStore(testInfo);
            return store;
        }
        

        public static TestStore RemoveTestStore(TestInfo testInfo)
        {
            if (_fixtureStores.TryGetValue(testInfo.FixtureInfo, out var fixtureStore))
            {
               return fixtureStore.RemoveTestStore(testInfo);
            }

            throw new InvalidOperationException($"No {typeof(DataPreparationFixtureAttribute)} found for {testInfo.FixtureInfo}.");

        }
        #endregion
    }
}
