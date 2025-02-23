using DataPreparation.Data;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DataPreparation.Testing
{

    public  static class FixtureStore
    {
       
        private static readonly ConcurrentDictionary<Type, ILoggerFactory> LoggerFactoryCollections = new();
        public static bool RegisterLoggerFactory(Type testFixtureType, ILoggerFactory loggerFactory)
        {
            return LoggerFactoryCollections.TryAdd(testFixtureType,loggerFactory);
        }
        public static ILoggerFactory? GetRegisteredLoggerFactory(Type testFixtureType)
        {
            return LoggerFactoryCollections.GetValueOrDefault(testFixtureType);
        }
        public static ILoggerFactory? RemoveLoggerFactory(Type testFixtureType)
        {
            
            LoggerFactoryCollections.TryRemove(testFixtureType,out var ret);
            return ret;
        }
        
        
        //Store Service Provider for each test case
        private static readonly ConcurrentDictionary<Type, IServiceCollection> ServiceCollections = new();
        public static bool RegisterService(Type testFixtureType, IServiceCollection serviceCollection)
        {
            return ServiceCollections.TryAdd(testFixtureType,serviceCollection);
        }
        public static IServiceCollection? GetRegisteredService(Type testFixtureType)
        {
            return ServiceCollections.GetValueOrDefault(testFixtureType);
        }
        
        public static IServiceCollection? RemoveService(Type testFixtureType)
        {
            
            ServiceCollections.TryRemove(testFixtureType,out var ret);
            return ret;
        }



    }
}
