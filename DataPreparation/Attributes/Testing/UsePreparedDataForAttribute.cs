using DataPreparation.Data;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DataPreparation.Testing
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class UsePreparedDataForAttribute: Attribute, ITestAction
    {
        private List<IDataPreparation> _testData;
        private readonly Type _classType;
        private readonly string[] _methodsNames;
        private bool _useClassDataPreparation = false;
        public UsePreparedDataForAttribute(Type classType, params string[] methodsNames)
        {
            _classType = classType;
            _methodsNames = methodsNames;
        }
        public UsePreparedDataForAttribute(Type classType,bool useClassDataPreparation, params string[] methodsNames)
        {
            _classType = classType;
            _methodsNames = methodsNames;
            _useClassDataPreparation = useClassDataPreparation;
        }

        public void BeforeTest(ITest test)
        {

            _testData = new();
             
            var dataPreparationClassType = DataRegister.GetClassDataPreparationType(_classType);
            if (dataPreparationClassType == null)
            {
                //error handling 
            }
            else
            {
                var dataPreparationClass = DataRegister.GetTestCaseService(test, dataPreparationClassType);
                if (dataPreparationClass == null)
                {
                    //TODO
                }
                if (dataPreparationClass is IClassDataPreparation dataPreparationClassInstance)
                {
                   
                    _testData.Add(dataPreparationClassInstance);
                }
                else
                {
                    //TODO
                }
            }

           

            foreach (var methodName in _methodsNames)
            {
                var methodInfo = _classType.GetMethod(methodName);
                if (methodInfo != null)
                {
                    var dataPreparationMethodType = DataRegister.GetMethodDataPreparationType(methodInfo);
                    var dataPreparationMethodClass = DataRegister.GetTestCaseService(test, dataPreparationMethodType);
                    if (dataPreparationMethodClass == null )
                    {
                        //TODO
                    }

                    if (dataPreparationMethodClass is IMethodDataPreparation dataPreparationMethodInstance)
                    {
                        _testData.Add(dataPreparationMethodInstance);
                    }
                    else
                    {
                        //TODO
                    }
                }
            }

            TestDataHandler.DataUp(_testData);
          
          
        }

        public void AfterTest(ITest test)
        {
            TestDataHandler.DataDown(_testData);
        }

        public ActionTargets Targets => ActionTargets.Test;
    }
}
