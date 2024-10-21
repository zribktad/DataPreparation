using DataPreparation.Data;
using DataPreparation.Testing.Stores;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class UsePreparedDataAttribute(params Type[] dataProviders) : Attribute, ITestAction
    {
        //get from Data{Register} the data preparation for the test

        public void BeforeTest(ITest test)
        {

            foreach (var dataPreparationType in dataProviders)
            {
                var preparedData = DataRegister.GetTestCaseServiceData(test, dataPreparationType);
                if (preparedData == null)
                {
                    throw new Exception($"Prepared data for {dataPreparationType.FullName} not found");
                }
                _preparedDataList.Add(preparedData);
            
            }


            TestDataHandler.DataUp(_preparedDataList);
           
        }

        public void AfterTest(ITest test)
        {
            //down data for the test
            TestDataHandler.DataDown(_preparedDataList);

        }

        private readonly List<IDataPreparation> _preparedDataList = new();
        public ActionTargets Targets => ActionTargets.Test;
    }
}
