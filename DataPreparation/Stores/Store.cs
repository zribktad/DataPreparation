using DataPreparation.Data;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Interfaces;
using System;
using System.Collections;
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

        public static FixtureStore GetFixtureStore(FixtureInfo fixtureInfo)
        {
            return FixtureStores[fixtureInfo];
        }
        
        internal static bool RemoveFixtureStore(FixtureInfo fixtureInfo)
        {
            return FixtureStores.TryRemove(fixtureInfo, out _);
        }
        
        public static ICollection<FixtureStore> GetFixtureStores()
        {
            return FixtureStores.Values;
        }
    }
}
