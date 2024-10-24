using System.Reflection;
using DataPreparation.Data;
using DataPreparation.DataHandling;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Attribute to specify that prepared data should be used for the test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class UsePreparedDataForAttribute : NUnitAttribute, ITestAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsePreparedDataForAttribute"/> class.
        /// </summary>
        /// <param name="classType">The type of the class containing the methods.</param>
        /// <param name="methodsNames">The names of the methods for which data preparation is required.</param>
        public UsePreparedDataForAttribute(Type classType, params string[] methodsNames)
        {
            _classType = classType;
            _methodsNames = methodsNames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsePreparedDataForAttribute"/> class.
        /// </summary>
        /// <param name="classType">The type of the class containing the methods.</param>
        /// <param name="useClassDataPreparation">Indicates whether to use class-level data preparation.</param>
        /// <param name="methodsNames">The names of the methods for which data preparation is required.</param>
        public UsePreparedDataForAttribute(Type classType, bool useClassDataPreparation, params string[] methodsNames)
        {
            _classType = classType;
            _methodsNames = methodsNames;
            _useClassDataPreparation = useClassDataPreparation;
        }

        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public void BeforeTest(ITest test)
        {
            TestData.ServiceProvider = CaseProviderStore.GetRegistered(test.Fixture.GetType());
            _preparedDataList = DataPreparations(test);
            TestDataPreparationStore.AddDataPreparation(test.Method.MethodInfo, _preparedDataList);
            TestDataHandler.DataUp(test.Method.MethodInfo);
        }

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public void AfterTest(ITest test)
        {
            TestDataHandler.DataDown(test.Method.MethodInfo);
        }

        /// <summary>
        /// Prepares the data list for the test.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        /// <returns>A list of prepared data.</returns>
        /// <exception cref="Exception">Thrown when prepared data for a class or method is not found.</exception>
        private List<IDataPreparation> DataPreparations(ITest test)
        {
            List<IDataPreparation> preparedDataList = new();
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

        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public ActionTargets Targets => ActionTargets.Test;

        private List<IDataPreparation> _preparedDataList;
        private readonly Type _classType;
        private readonly string[] _methodsNames;
        private readonly bool _useClassDataPreparation = false;
    }
}
