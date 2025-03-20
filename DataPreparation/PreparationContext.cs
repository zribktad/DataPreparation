using DataPreparation.Data;
using DataPreparation.Models.Data;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
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
          
            var currentTestStore = TestStore.Get(new ContextTestInfo(TestContext.CurrentContext.Test));
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
            var currentTestStore = TestStore.Get(new ContextTestInfo (TestContext.CurrentContext.Test));
            if(currentTestStore == null)
            {
                throw new InvalidOperationException($"This method should be called from a test method context.");
            }

            return currentTestStore.SourceFactory;
        }
      
    }
}
