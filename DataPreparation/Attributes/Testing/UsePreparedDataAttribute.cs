using DataPreparation.Data;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class UsePreparedDataAttribute : NUnitAttribute, ITestAction
    {
        public UsePreparedDataAttribute(params Type[] dataProviders)
        {
            _dataProviders = dataProviders;
        }

        public void BeforeTest(ITest test)
        {
            DataPreparation.TestData.ServiceProvider = CaseProviderStore.GetRegistered(test.Fixture.GetType());
            _preparedDataList = PrepareDataList(test, _dataProviders);
            TestDataPreparationStore.AddDataPreparation(test.Method.MethodInfo, _preparedDataList);
            TestDataHandler.DataUp(test.Method.MethodInfo);
        }
        public void AfterTest(ITest test)
        {
            //down data for the test
            TestDataHandler.DataDown(test.Method.MethodInfo);

        }

        private List<IDataPreparation> PrepareDataList(ITest test, Type[] dataProviders)
        {
            List<IDataPreparation> preparedDataList = new();
            foreach (var dataPreparationType in dataProviders)
            {
                var preparedData = CaseProviderStore.GetTestCaseServiceData(test, dataPreparationType);
                if (preparedData == null)
                {
                    throw new Exception($"Prepared data for {dataPreparationType.FullName} not found");
                }
                preparedDataList.Add(preparedData);

            }
            return preparedDataList;
        }

        private List<IDataPreparation> _preparedDataList = new();
        private readonly Type[] _dataProviders;


        public ActionTargets Targets => ActionTargets.Test;
    }
}
