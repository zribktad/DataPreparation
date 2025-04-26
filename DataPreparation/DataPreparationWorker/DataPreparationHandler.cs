using DataPreparation.Exceptions;
using DataPreparation.Models;
using DataPreparation.Models.Data;
using Microsoft.Extensions.Logging;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Handles the execution lifecycle of data preparation operations for test cases.
    /// Responsible for setting up test data before tests run and cleaning it up after tests complete.
    /// </summary>
    /// <remarks>
    /// This class orchestrates the execution of data preparation methods that are marked with
    /// appropriate attributes (UpData and DownData). It supports both synchronous and asynchronous
    /// preparation methods.
    /// </remarks>
    internal static class DataPreparationHandler
    {
        /// <summary>
        /// Synchronously executes the data preparation "Up" phase for a test.
        /// This method sets up all required test data for the test to run.
        /// </summary>
        /// <param name="testStore">The test store containing all preparation data for the current test</param>
        internal static void DataUp(TestStore testStore)
        {
            DataUpTask(testStore).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously executes the data preparation "Up" phase for a test.
        /// This sets up all the required test data by invoking each preparatory method in sequence.
        /// </summary>
        /// <param name="testStore">The test store containing all preparation data for the current test</param>
        /// <returns>A ValueTask representing the asynchronous operation</returns>
        private static async ValueTask DataUpTask(TestStore testStore)
        {
            // Increment the attribute counter and check if all attributes have been processed
            testStore.AttributeUsingCounter.IncrementAttrributeCountUp();
            if (!testStore.AttributeUsingCounter.IsAllUpAttributesRun()) return;
            
            // NOTE: Analysis code is commented out, but could be used to analyze method calls
            //analyse and results 
            //var analysisResult = MethodAnalyzer.Analyze(testMethodInfo);
            //MethodAnalyzer.Analyze(testMethodInfo);
            //analysisResult?.Print();
            
            // Log that we're starting the data preparation process
            testStore.LoggerFactory.CreateLogger(typeof(DataPreparationHandler))
                .LogInformation($"Data preparation Up for before test {testStore.TestInfo} started.");
            
            // Retrieve all preparation data items to be executed
            var testData = testStore.PreparedData.GetPreparation();
            
            // Execute each preparation method in sequence
            foreach (var data in testData)
            {
                try
                {
                    // Handle async and sync methods differently
                    if (data.IsRunUpASync())
                    {
                        // Execute async preparation method and await its completion
                        await data.RunUpAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        // Execute synchronous preparation method
                        data.RunUp();
                    }

                    // After successful execution, track that this data was processed
                    // so we can clean it up later in the right order (LIFO)
                    testStore.PreparedData.PushProcessed(data);
                }
                catch (Exception e)
                {
                    // Log any errors that occur during data preparation
                    testStore.LoggerFactory.CreateLogger(typeof(DataPreparationHandler))
                        .LogError(e, "Error while running data up.");
                    throw; // Rethrow to fail the test
                }
            }
        }

        /// <summary>
        /// Asynchronously executes the data preparation "Down" phase after a test completes.
        /// This cleans up all data that was created during the "Up" phase in reverse order (LIFO).
        /// </summary>
        /// <param name="testStore">The test store containing all processed preparation data</param>
        /// <returns>A ValueTask representing the asynchronous operation</returns>
        internal static async ValueTask DataDown(TestStore? testStore)
        {
            // Skip if there's no test store or no data to clean up
            if(testStore == null || testStore.PreparedData.IsEmpty()) return; //after first is testStore removed so it can be null
            
            // Log the start of the data teardown process
            testStore.LoggerFactory.CreateLogger(typeof(DataPreparationHandler))
                .LogInformation($"Data preparation before test Down {testStore.TestInfo} started.");
            
            // Use an exception aggregator to collect all exceptions during teardown
            // so we can continue cleanup even if some operations fail
            ExceptionAggregator exceptionAggregator = new();
            
            // Process all data items in reverse order (LIFO)
            while (testStore.PreparedData.TryPopProcessed(out var data))
            {
                try
                {
                    if (data != null)
                    {
                        // Handle async and sync methods differently
                        if (data.IsRunDownASync())
                        {
                            // Execute async cleanup method and await its completion
                            await data.RunDownAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            // Execute synchronous cleanup method
                            data.RunDown();
                        }
                    }
                }
                catch (Exception e)
                {
                    // Collect exceptions but continue with cleanup
                    exceptionAggregator.Add(e);
                    throw;
                }
            }
            
            // After all cleanup operations, check if any exceptions occurred
            var exception = exceptionAggregator.Get();
            if (exception != null)
            {
                // Log aggregated exceptions that occurred during cleanup
                testStore.LoggerFactory.CreateLogger(typeof(DataPreparationHandler))
                    .LogError(exception, "Errors while running data down.");
                throw exception; // Rethrow to fail the test
            }
        }
    }
}
