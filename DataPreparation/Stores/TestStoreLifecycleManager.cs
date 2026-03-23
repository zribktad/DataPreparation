using DataPreparation.Data;
using DataPreparation.Exceptions;
using DataPreparation.Factory.Testing;
using DataPreparation.Helpers;
using DataPreparation.Helpers;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataPreparation.Models.Data;

/// <summary>
/// Manages the lifecycle of <see cref="TestStore"/> instances — creation, initialization, 
/// cleanup, and disposal. Extracted from TestStore to satisfy SRP.
/// </summary>
internal static class TestStoreLifecycleManager
{
    /// <summary>
    /// Initializes a TestStore for the specified test.
    /// If a TestStore already exists for this test, returns it. Otherwise, creates a new one.
    /// </summary>
    /// <param name="testInfo">Information about the test requiring a TestStore</param>
    /// <returns>An initialized TestStore instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when no test fixture is found for this test</exception>
    internal static TestStore Initialize(TestInfo testInfo)
    {
        // Return existing test store if already initialized
        var existing = FindByContext(testInfo);
        if (existing != null) return existing;

        var loggerFactory = LoggerHelper.CreateOrNullLogger(testInfo.FixtureInfo);
        var dataPreparationAttributes = AttributeHelper.GetAttributes(
            testInfo.Method.MethodInfo, typeof(UsePreparedAttribute));

        var testStore = CreateInFixtureStore(testInfo, loggerFactory, dataPreparationAttributes);

        // Execute beforeTest hook if implemented by the fixture
        if (testInfo.FixtureInfo.Instance is IBeforeTest beforeTest)
        {
            beforeTest.BeforeTest(testStore.ServiceProvider);
        }

        return testStore;
    }

    /// <summary>
    /// Cleans up and deinitializes a TestStore after test execution.
    /// Uses ExceptionAggregator to ensure all cleanup steps run even if some fail.
    /// </summary>
    /// <param name="testStore">The TestStore to deinitialize (can be null)</param>
    /// <returns>null after successful deinitialization</returns>
    internal static TestStore? Deinitialize(TestStore? testStore)
    {
        if (testStore is null) return null;

        var errors = new ExceptionAggregator();

        errors.TryExecute(() =>
            testStore.SourceFactory.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult());

        errors.TryExecute(() =>
            DataPreparationHandler.DataDown(testStore).ConfigureAwait(false).GetAwaiter().GetResult());

        if (testStore.TestInfo.FixtureInfo.Instance is IAfterTest afterTest)
        {
            errors.TryExecute(() => afterTest.AfterTest(testStore.ServiceProvider));
        }

        errors.TryExecute(() =>
        {
            var fixtureStore = Store.GetFixtureStore(testStore.TestInfo.FixtureInfo)
                ?? throw new InvalidOperationException(
                    $"No {typeof(DataPreparationFixtureAttribute)} found for {testStore.TestInfo.FixtureInfo}.");
            var removed = fixtureStore.RemoveTestStore(testStore.TestInfo);
            removed?.ServiceScope.Dispose();
        });

        errors.ThrowIfAny();
        return null;
    }

    /// <summary>
    /// Finds an existing TestStore by searching all fixture stores.
    /// </summary>
    internal static TestStore? FindByContext(ContextTestInfo testInfo)
    {
        foreach (var fixtureStore in Store.GetFixtureStores())
        {
            var testStore = fixtureStore.GetTestStore(testInfo);
            if (testStore != null) return testStore;
        }
        return null;
    }

    /// <summary>
    /// Creates a new TestStore within the appropriate FixtureStore.
    /// </summary>
    private static TestStore CreateInFixtureStore(TestInfo testInfo, ILoggerFactory loggerFactory,
        IList<Attribute> dataPreparationAttributes)
    {
        var testLogger = loggerFactory.CreateLogger(typeof(Store));
        testLogger.LogTrace("Test data initialization for {TestInfo} started", testInfo);

        var fixtureStore = Store.GetFixtureStore(testInfo.FixtureInfo);
        if (fixtureStore is null)
        {
            testLogger.LogError("No {AttributeType} found for {FixtureInfo}.",
                typeof(DataPreparationFixtureAttribute), testInfo.FixtureInfo);
            throw new InvalidOperationException(
                $"No {typeof(DataPreparationFixtureAttribute)} found for {testInfo.FixtureInfo}.");
        }

        var fixtureLogger = fixtureStore.LoggerFactory.CreateLogger(nameof(Store));
        if (!fixtureStore.CreateTestStore(testInfo, loggerFactory, dataPreparationAttributes))
        {
            LoggerHelper.Log(
                l => l.LogTrace("Test data initialization for {TestInfo} already exists", testInfo),
                fixtureLogger, testLogger);
        }

        LoggerHelper.Log(
            l => l.LogDebug("Test data initialization for {TestInfo} created", testInfo),
            fixtureStore.LoggerFactory.CreateLogger(typeof(Store)), testLogger);

        return fixtureStore.GetTestStore(testInfo)!;
    }
}
