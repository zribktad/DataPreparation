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
    public static class PreparationContext
    {
        
        public static IServiceProvider GetProvider()
        {
          
            var currentTestStore = Store.GetTestStore(new ContextTestInfo(TestContext.CurrentContext.Test));
            if(currentTestStore == null)
            {
                throw new InvalidOperationException($"This method should be called from a test method context.");
            }
            return  currentTestStore.ServiceProvider;
        }

 

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
