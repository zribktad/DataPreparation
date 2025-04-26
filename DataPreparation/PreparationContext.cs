using DataPreparation.Data;
using DataPreparation.Models.Data;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using NUnit.Framework;

namespace DataPreparation.Provider
{
    /// <summary>
    /// Provides a static entry point for accessing data preparation resources in a testing environment.
    /// This class serves as the main facade for test classes to interact with the DataPreparation framework.
    /// It allows tests to retrieve the configured ServiceProvider and SourceFactory instances.
    /// </summary>
    /// <remarks>
    /// This context is automatically initialized when tests decorated with DataPreparationFixture attribute run.
    /// All methods must be called from within a test method context, otherwise they will throw InvalidOperationException.
    /// </remarks>
    public static class PreparationContext
    {
        /// <summary>
        /// Returns the dependency injection service provider from the current test context.
        /// This provider can be used to resolve services registered with the test fixture.
        /// </summary>
        /// <returns>An IServiceProvider instance configured for the current test context.</returns>
        /// <exception cref="InvalidOperationException">Thrown when called outside of a test method context.</exception>
        /// <example>
        /// <code>
        /// [Test]
        /// public void MyTest()
        /// {
        ///     var orderService = PreparationContext.GetProvider().GetService&lt;IOrderService&gt;();
        ///     // Use orderService in your test
        /// }
        /// </code>
        /// </example>
        public static IServiceProvider GetProvider()
        {
            // Retrieve the test store associated with the current test context
            var currentTestStore = TestStore.Get(new ContextTestInfo(TestContext.CurrentContext.Test));
            if(currentTestStore == null)
            {
                throw new InvalidOperationException($"This method should be called from a test method context.");
            }
            return currentTestStore.ServiceProvider;
        }
        
        /// <summary>
        /// Returns the source factory from the current test context.
        /// The source factory provides methods for creating and managing test data.
        /// </summary>
        /// <returns>An ISourceFactory instance configured for the current test context.</returns>
        /// <exception cref="InvalidOperationException">Thrown when called outside of a test method context.</exception>
        /// <example>
        /// <code>
        /// [Test]
        /// public void MyTest()
        /// {
        ///     var factory = PreparationContext.GetFactory();
        ///     var customer = factory.New<CustomerFactory>();
        ///     // Use customer in your test
        /// }
        /// </code>
        /// </example>
        public static ISourceFactory GetFactory()
        {
            var currentTestStore = TestStore.Get(new ContextTestInfo(TestContext.CurrentContext.Test));
            if(currentTestStore == null)
            {
                throw new InvalidOperationException($"This method should be called from a test method context.");
            }

            return currentTestStore.SourceFactory;
        }
    }
}
