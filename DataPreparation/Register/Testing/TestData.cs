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
            
            return  TestStore.GetRegistered(methodBase) ?? throw new InvalidOperationException($"No service provider found.");
        }

 

        public static ISourceFactory GetFactory()
        {
            return null;
        }
      
    }
}
