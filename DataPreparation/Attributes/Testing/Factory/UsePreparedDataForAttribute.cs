using System.Reflection;
using DataPreparation.Data;
using DataPreparation.DataHandling;
using DataPreparation.Provider;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DataPreparation.Testing.Factory
{
    /// <summary>
    /// Attribute to specify that prepared data should be used for the test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class FactoryTestAttribute : TestAttribute, ITestAction
    {
        public FactoryTestAttribute()
        {
        }
        
 
        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public void BeforeTest(ITest test)
        {
            var baseDataServiceCollection = FixtureStore.GetRegisteredService(test.Fixture.GetType());
            
            if (!TestStore.RegisterDataCollection(test.Method.MethodInfo, baseDataServiceCollection))
            {
                Console.Error.WriteLine($"Data preparation for {test.Method.MethodInfo.Name} failed.");
            }

        }

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public  void AfterTest(ITest test)
        {
            if (null == TestStore.DeleteProvider(test.Method.MethodInfo))
            {
                //TODO: Log
            }
            if(null == TestStore.DeleteFactory(test.Method.MethodInfo))
            {
                //TODO: Log
            }
        }

       
        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public ActionTargets Targets => ActionTargets.Test;
        
    }
}
