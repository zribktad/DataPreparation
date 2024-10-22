using System.Reflection;
using DataPreparation.Data;
using DataPreparation.DataHandling;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DataPreparation.Testing
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class UsePreparedDataForAttribute: Attribute, ITestAction
    {
     
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
          
            _preparedDataList = DataPreparations(test);
            TestDataPreparationStore.AddDataPreparation(test.Method.MethodInfo, _preparedDataList);
            TestDataHandler.DataUp(test.Method.MethodInfo);
        }


        public void AfterTest(ITest test)
        {
            Type testMethodType = test.Method.TypeInfo.Type;
            TestDataHandler.DataDown(test.Method.MethodInfo);
        }

        private List<IDataPreparation> DataPreparations(ITest test)
        {
            List<IDataPreparation> preparedDataList = [];
            if (_useClassDataPreparation)
            {
                var preparationData = GetDataPreparation.Class(test, _classType);
                if (preparationData == null)
                {
                    throw new Exception("Class data preparation not found");
                }
                preparedDataList.Add(preparationData);
            }

            foreach (var methodName in _methodsNames)
            {
                var methodInfo = _classType.GetMethod(methodName);
                var preparationData = GetDataPreparation.Method(test, methodInfo);
                if (preparationData == null)
                {
                    throw new Exception("Method data preparation not found");
                }
                preparedDataList.Add(preparationData);
            }

            return preparedDataList;
        }

        public ActionTargets Targets => ActionTargets.Test;
        private List<IDataPreparation> _preparedDataList;
        private readonly Type _classType;
        private readonly string[] _methodsNames;
        private readonly bool _useClassDataPreparation = false;
    }
}
