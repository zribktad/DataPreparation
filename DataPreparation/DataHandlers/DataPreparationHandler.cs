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
          
            testStore.AttributeUsing.IncrementAttrributeCountUp();
            if (!testStore.AttributeUsing.IsAllUpAttributesRun()) return;
            
            //analyse and results 
            //var analysisResult = MethodAnalyzer.Analyze(testMethodInfo);
            //MethodAnalyzer.Analyze(testMethodInfo);
            
            //analysisResult?.Print();
            
            testStore.LoggerFactory.CreateLogger(typeof(DataPreparationHandler))
                .LogInformation($"Data preparation Up for test {testStore.TestInfo} started.");
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

        internal static void DataDown(TestStore? testStore)
        {
            if(testStore == null) return; //after first is testStore removed so it can be null
            testStore.LoggerFactory.CreateLogger(typeof(DataPreparationHandler))
                .LogInformation($"Data preparation Down for test {testStore.TestInfo} started.");
            ExceptionAggregator exceptionAggregator = new();
            while (testStore.PreparedData.TryPopProcessed(out var data))
            {
                try
                {
                    data?.RunDown().GetAwaiter().GetResult();
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
