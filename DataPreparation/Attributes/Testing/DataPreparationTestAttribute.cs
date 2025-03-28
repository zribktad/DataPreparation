﻿using System.Reflection;
using DataPreparation.Data;
using DataPreparation.Helpers;
using DataPreparation.Models.Data;
using DataPreparation.Provider;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DataPreparation.Testing.Factory
{
    /// <summary>
    /// Attribute to specify that prepared data should be used for the test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method,Inherited = false)]
    public class DataPreparationTestAttribute : TestAttribute, ITestAction
    {
        private TestInfo _testInfo = null!;
        
        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public void BeforeTest(ITest test)
        {
            _testInfo = TestInfo.CreateTestInfo(test);
            TestStore.Initialize(_testInfo);
        }

   

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public void AfterTest(ITest test)
        {
            var testStore = TestStore.Get(_testInfo);
            TestStore.Deinitialize(testStore);
        }

    


        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public ActionTargets Targets => ActionTargets.Test;
        
    }
}
