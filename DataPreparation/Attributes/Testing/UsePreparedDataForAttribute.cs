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
    public class UsePreparedDataForAttribute : UsePreparedAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsePreparedDataForAttribute"/> class.
        /// </summary>
        /// <param name="classType">The type of the class containing the methods.</param>
        /// <param name="methodsNames">The names of the methods for which data preparation is required.</param>
        public UsePreparedDataForAttribute(Type classType, params string[] methodsNames): this(classType, false, methodsNames)
        {
        }
    

        /// <summary>
        /// Initializes a new instance of the <see cref="UsePreparedDataForAttribute"/> class.
        /// </summary>
        /// <param name="classType">The type of the class containing the methods.</param>
        /// <param name="useClassDataPreparation">Indicates whether to use class-level data preparation.</param>
        /// <param name="methodsNames">The names of the methods for which data preparation is required.</param>
        public UsePreparedDataForAttribute(Type classType, bool useClassDataPreparation, params string[] methodsNames)
        {
            _classType = classType ?? throw new ArgumentNullException(nameof(classType));
            _methodsNames = methodsNames ?? throw new ArgumentNullException(nameof(methodsNames));
            _useClassDataPreparation = useClassDataPreparation;
        }

        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public override void BeforeTest(ITest test)
        {
            
            TestInfo testInfo = TestInfo.CreateTestInfo(test);
            var testStore = TestStore.InitializeTestStore(testInfo);
            // Prepare data for the test from attribute
            var preparedDataList = GetDataPreparation.GetPreparedDataFromCode(testStore, _useClassDataPreparation, _classType, _methodsNames); 
            // Add the prepared data to the store
            testStore.PreparedData.AddDataPreparation(preparedDataList);
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
            DataPreparationHandler.DataDown(testStore);
            TestStore.RemoveTestStore(testStore);
        }

       
        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public override ActionTargets Targets => ActionTargets.Test;
        
        private readonly Type _classType;
        private readonly string[] _methodsNames;
        private readonly bool _useClassDataPreparation;
    }
}
