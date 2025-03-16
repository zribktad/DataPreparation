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

    internal static class Store
    {
        //Store FixtureStore for each test fixture
        private static readonly ConcurrentDictionary<FixtureInfo, FixtureStore> FixtureStores = new();
        
        internal static bool CreateFixtureStore(FixtureInfo fixtureInfo,ILoggerFactory loggerFactory,IServiceCollection serviceCollection)
        {
            loggerFactory.CreateLogger(typeof(Store)).LogTrace("Creating FixtureStore for {0}", fixtureInfo);
            var ret =  FixtureStores.TryAdd(fixtureInfo, new(fixtureInfo,loggerFactory,serviceCollection));
            loggerFactory.CreateLogger(typeof(Store)).LogTrace("FixtureStore for {0} created", fixtureInfo);
            return ret;
        }

        private static FixtureStore GetFixtureStore(FixtureInfo fixtureInfo)
        {
            return FixtureStores[fixtureInfo];
        }
        
        internal static bool RemoveFixtureStore(FixtureInfo fixtureInfo)
        {
            return FixtureStores.TryRemove(fixtureInfo, out _);
        }
        
        #region Test Store
        internal static TestStore CreateTestStore(TestInfo testContextTestInfo, ILoggerFactory loggerFactory,IList<Attribute> dataPreparationAttributes)
        {
            var testLogger = loggerFactory.CreateLogger(typeof(Store));
            testLogger.LogTrace("Test data initialization for {0} started", testContextTestInfo);

            if (FixtureStores.TryGetValue(testContextTestInfo.FixtureInfo, out var fixtureStore))
            {
                var fixtureLogger = fixtureStore.LoggerFactory.CreateLogger(nameof(Store));
                fixtureLogger.LogTrace("Test data initialization for {0} started", testContextTestInfo);

                if (!fixtureStore.CreateTestStore(testContextTestInfo, loggerFactory,dataPreparationAttributes))
                {
                    LoggerHelper.Log(logger => logger.LogTrace("Test data initialization for {0} already exists", testContextTestInfo),
                        fixtureLogger,testLogger);
                }
            }
            else
            {
                testLogger.LogError("No {0} found for {1}.", typeof(DataPreparationFixtureAttribute), testContextTestInfo.FixtureInfo);
                throw new InvalidOperationException($"No {typeof(DataPreparationFixtureAttribute)} found for { testContextTestInfo.FixtureInfo}.");
            }
            LoggerHelper.Log(logger => logger.LogDebug("Test data initialization for {0} created", testContextTestInfo), 
                fixtureStore.LoggerFactory.CreateLogger(typeof(Store)),testLogger);
  
            return GetTestStore(testContextTestInfo)!;
        }

        internal static TestStore GetTestStore(ContextTestInfo testInfo)
        {
            foreach (var fixtureStore in FixtureStores.Values)
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
        

        public static TestStore? RemoveTestStore(TestInfo testInfo)
        {
            if (FixtureStores.TryGetValue(testInfo.FixtureInfo, out var fixtureStore))
            {
               return fixtureStore.RemoveTestStore(testInfo);
            }

            throw new InvalidOperationException($"No {typeof(DataPreparationFixtureAttribute)} found for {testInfo.FixtureInfo}.");

        }
        #endregion
    }
}
