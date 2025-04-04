using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DataPreparation.Data;
using DataPreparation.DataHandlers;
using DataPreparation.Models.Data;
using DataPreparation.Provider;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Attribute to specify that prepared data should be used for the test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class UsePreparedDataParamsForAttribute : UsePreparedAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsePreparedDataParamsForAttribute"/> class.
        /// </summary>
        /// <param name="classType">The type of the class containing the method.</param>
        /// <param name="methodName">The name of the method to be tested.</param>
        /// <param name="paramsUpData">The parameters for the data preparation before the test.</param>
        /// <param name="paramsDownData">The parameters for the data preparation after the test.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are null.</exception>
        public UsePreparedDataParamsForAttribute(Type classType, string methodName, [NotNull] object[] paramsUpData, [NotNull] object[] paramsDownData)
        {
            _classType = classType ?? throw new ArgumentNullException(nameof(classType));
            _methodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            _useClassDataPreparation = false;
            _paramsUpData = paramsUpData ?? throw new ArgumentNullException(nameof(paramsUpData));
            _paramsDownData = paramsDownData ?? throw new ArgumentNullException(nameof(paramsDownData));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsePreparedDataParamsForAttribute"/> class with class-level data preparation.
        /// </summary>
        /// <param name="classType">The type of the class containing the method.</param>
        /// <param name="classParamsUpData">The parameters for the class-level data preparation before the test.</param>
        /// <param name="classParamsDownData">The parameters for the class-level data preparation after the test.</param>
        /// <param name="methodName">The name of the method to be tested.</param>
        /// <param name="paramsUpData">The parameters for the data preparation before the test.</param>
        /// <param name="paramsDownData">The parameters for the data preparation after the test.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are null.</exception>

        public UsePreparedDataParamsForAttribute(Type classType, [NotNull] object[] classParamsUpData, [NotNull]object[] classParamsDownData,  string methodName,  [NotNull] object[] paramsUpData, [NotNull]object[] paramsDownData):this(classType,methodName,paramsUpData,paramsDownData)
        {
            _useClassDataPreparation = true;
            _classParamsUpData = classParamsUpData ?? throw new ArgumentNullException(nameof(classParamsUpData));
            _classParamsDownData = classParamsDownData ?? throw new ArgumentNullException(nameof(classParamsDownData));
        }

        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public override void BeforeTest(ITest test)
        {
            
            TestInfo testInfo = TestInfo.CreateTestInfo(test);
            TestStore testStore = TestStore.Initialize(testInfo);
            
            // Prepare class data for the test from attribute
            var preparedData = GetDataPreparation.GetPreparedDataFromCode(testStore, _useClassDataPreparation, _classType, [_methodName]); 
            if(preparedData.Count == 0) return;
            //add data to store
            testStore.PreparedData.AddDataPreparation(preparedData[0],_paramsUpData,_paramsDownData);
            
            // Up data for the test if all data are prepared
            DataPreparationHandler.DataUp(testStore);
        }

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public override void AfterTest(ITest test)
        {
            TestInfo testInfo = TestInfo.CreateTestInfo(test);
            var testStore = TestStore.Get(testInfo);
         
            TestStore.Deinitialize(testStore);
        }

       
        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public override ActionTargets Targets => ActionTargets.Test;
        
        private readonly Type _classType;
        private readonly string _methodName;
        private readonly bool _useClassDataPreparation = false;
        private readonly object[] _classParamsUpData;
        private readonly object[] _classParamsDownData;
        private readonly object[] _paramsUpData;
        private readonly object[] _paramsDownData;
    }
}
