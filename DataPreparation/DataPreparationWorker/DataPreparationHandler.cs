using DataPreparation.Exceptions;
using DataPreparation.Models;
using DataPreparation.Models.Data;
using Microsoft.Extensions.Logging;

namespace DataPreparation.Testing
{
    internal static class  DataPreparationHandler
    {
        internal static void DataUp(TestStore testStore)
        {
          
            testStore.AttributeUsingCounter.IncrementAttrributeCountUp();
            if (!testStore.AttributeUsingCounter.IsAllUpAttributesRun()) return;
            
            //analyse and results 
            //var analysisResult = MethodAnalyzer.Analyze(testMethodInfo);
            //MethodAnalyzer.Analyze(testMethodInfo);
            
            //analysisResult?.Print();
            
            testStore.LoggerFactory.CreateLogger(typeof(DataPreparationHandler))
                .LogInformation($"Data preparation Up for before test {testStore.TestInfo} started.");
            var testData = testStore.PreparedData.GetPreparation();
            foreach (var data in testData)
            {
                try
                {
                    data.RunUp().GetAwaiter().GetResult();
                    testStore.PreparedData.PushProcessed(data);
                }
                catch (Exception e)
                {
                    testStore.LoggerFactory.CreateLogger(typeof(DataPreparationHandler))
                        .LogError(e, "Error while running data up.");
                    throw;
                }
               
            }
      
        }

        internal static async ValueTask DataDown(TestStore? testStore)
        {
            if(testStore == null || testStore.PreparedData.IsEmpty()) return; //after first is testStore removed so it can be null
            testStore.LoggerFactory.CreateLogger(typeof(DataPreparationHandler))
                .LogInformation($"Data preparation before test Down {testStore.TestInfo} started.");
            ExceptionAggregator exceptionAggregator = new();
            while (testStore.PreparedData.TryPopProcessed(out var data))
            {
                try
                {
                    if (data != null)
                        await data.RunDown().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    exceptionAggregator.Add(e);
                    throw;
                }
            }
            
            var exception = exceptionAggregator.Get();
            if (exception != null)
            {
                testStore.LoggerFactory.CreateLogger(typeof(DataPreparationHandler))
                    .LogError(exception, "Errors while running data down.");
                throw exception;
            }
            
        }

        
    }
}
