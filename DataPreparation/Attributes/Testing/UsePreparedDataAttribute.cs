using DataPreparation.Data;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class UsePreparedDataAttribute : Attribute, ITestAction
    {
        public UsePreparedDataAttribute(params Type[] dataProviders)
        {
            _dataProviders = dataProviders;
        }

        public void BeforeTest(ITest test)
        {
            _preparedDataList = PrepareDataList(test, _dataProviders);

            TestDataHandler.DataUp(_preparedDataList);
        }


        public void AfterTest(ITest test)
        {
            //down data for the test
            TestDataHandler.DataDown(_preparedDataList);

        }

        private List<IDataPreparation> PrepareDataList(ITest test, Type[] dataProviders)
        {
            List<IDataPreparation> preparedDataList = new();
            foreach (var dataPreparationType in dataProviders)
            {
                var preparedData = DataRegister.GetTestCaseServiceData(test, dataPreparationType);
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
