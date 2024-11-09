﻿using System.Reflection;
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
            TestData.ServiceProvider = CaseProviderStore.GetRegistered(test.Fixture!.GetType());
            // Prepare data for the test from attribute
            var preparedDataList = GetDataPreparation.DataPreparations(test, _useClassDataPreparation, _classType, _methodsNames); 
            // Add the prepared data to the store
            TestDataPreparationStore.AddDataPreparation(test.Method!.MethodInfo, preparedDataList);
            // Up data for the test if all data are prepared
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
        /// Gets the targets for the action.
        /// </summary>
        public ActionTargets Targets => ActionTargets.Test;
        
        private readonly Type _classType;
        private readonly string[] _methodsNames;
        private readonly bool _useClassDataPreparation = false;
    }
}
