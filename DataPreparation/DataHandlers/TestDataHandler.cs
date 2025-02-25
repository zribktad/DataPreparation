using DataPreparation.Models;
using DataPreparation.Models.Data;

namespace DataPreparation.Testing
{
    internal static class  TestDataHandler
    {
        internal static void DataUp(TestStore testStore)
        {
            try
            {
                testStore.AttributeUsing.IncrementAttrributeCountUp();
                if (!testStore.AttributeUsing.IsAllUpAttributesRun()) return;
                
                //analyse and results 
                //var analysisResult = MethodAnalyzer.Analyze(testMethodInfo);
                //MethodAnalyzer.Analyze(testMethodInfo);
                
                //analysisResult?.Print();
                
                var testData = testStore.PreparedData.GetAll();
                Ups(testData);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        internal static void DataDown(TestStore? testStore)
        {
            if(testStore == null) return;
            var testData = testStore.PreparedData.GetAll();
            Downs(testData);
        }
        
        
        public static void Ups(List<PreparedData> testData)
        {
            foreach (var data in testData)
            {
                data.RunUp().GetAwaiter().GetResult();
            }
        }

        public static void Downs(List<PreparedData> testData)
        {
            foreach (var data in testData)
            {
                data.RunDown().GetAwaiter().GetResult();
            }
        }

    }
}
