using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DataPreparation.Data;
using DataPreparation.Factory.Testing;
using DataPreparation.Helpers;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DataPreparation.Provider
{
    /// <summary>
    /// Provides context for data preparation in a testing environment.
    /// </summary>
    public static class PreparationContext
    {
        
        /// <summary>
        /// Returns the service provider from the current test context.
        /// </summary>
        public static IServiceProvider GetProvider()
        {
          
            var currentTestStore = Store.GetTestStore(new ContextTestInfo(TestContext.CurrentContext.Test));
            if(currentTestStore == null)
            {
                throw new InvalidOperationException($"This method should be called from a test method context.");
            }
            return  currentTestStore.ServiceProvider;
        }
        
        /// <summary>
        /// Returns the source factory from the current test context.
        /// </summary>
        public static ISourceFactory GetFactory()
        {
            var currentTestStore = Store.GetTestStore(new ContextTestInfo (TestContext.CurrentContext.Test));
            if(currentTestStore == null)
            {
                throw new InvalidOperationException($"This method should be called from a test method context.");
            }

            return currentTestStore.SourceFactory;
        }
      
    }
}
