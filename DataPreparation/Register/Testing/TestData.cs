using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DataPreparation.Factory.Testing;
using DataPreparation.Helpers;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DataPreparation.Provider
{
    public static class TestData
    {
        
        public static IServiceProvider GetProvider()
        {
            var methodBase = TestMethodHelper.GetLatestTestMethod();
            
            return  TestStore.GetRegistered(methodBase?? throw new InvalidOperationException($"{nameof(GetProvider)} was used outside of Test method ")) ?? throw new InvalidOperationException($"No service provider found for {methodBase}.");
        }

 

        public static ISourceFactory GetFactory()
        {
            var methodBase = TestMethodHelper.GetLatestTestMethod();
            if(methodBase.GetCustomAttribute<FactoryTestAttribute>() == null)
            {
                throw new InvalidOperationException($"This method should be called from a test method with [{nameof(FactoryTestAttribute)}].");
            }
            
            return TestStore.GetOrCreateFactory(methodBase ?? throw new InvalidOperationException($"{nameof(GetFactory)} was used outside of Test method ")) ?? throw new InvalidOperationException($"No Factory found for {methodBase}.");
        }
      
    }
}
