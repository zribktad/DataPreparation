using System.Reflection;
using DataPreparation.Data;
using DataPreparation.Testing;
using NUnit.Framework.Interfaces;

namespace DataPreparation.DataHandling
{
    internal static class GetDataPreparation
    {
        /// <summary>
        /// Prepares the data list for the test.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        /// <param name="useClassDataPreparation"></param>
        /// <param name="classType"></param>
        /// <param name="methodsNames"></param>
        /// <returns>A list of prepared data.</returns>
        /// <exception cref="Exception">Thrown when prepared data for a class or method is not found.</exception>
        internal static List<object> DataPreparations(ITest test, bool useClassDataPreparation, Type classType,
            string[] methodsNames)
        {
            List<object> preparedDataList = new();
            if (useClassDataPreparation)
            {    
                preparedDataList.Add( GetClassDataPreparation(test, classType ));
            }
            foreach (var methodName in methodsNames)
            {
                preparedDataList.Add( GetMethodDataPreparation(test, classType, methodName ));
            }
            return preparedDataList;
        }


        internal static object GetMethodDataPreparation(ITest test, Type classType, string methodsName)
        {
            var methodInfo = classType.GetMethod(methodsName);
            var preparationData = Method(test, methodInfo);
            if (preparationData == null)
            {
                throw new Exception("Method data preparation not found");
            }

            return preparationData;
        }

        internal static object GetClassDataPreparation(ITest test, Type classType)
        {
            var preparationData = Class(test, classType);
            if (preparationData == null)
            {
                throw new Exception("Class data preparation not found");
            }

            return preparationData;
        }

        internal static object PrepareData(ITest test, Type dataPreparationType)
        {
            
            var preparedData = TestStore.GetTestFixtureServiceData((MethodBase)test.Method.MethodInfo, dataPreparationType);
            if (preparedData == null)
            {
                throw new Exception($"Prepared data for {dataPreparationType.FullName} not found");
            }

            return preparedData;
        }

        /// <summary>
        /// Prepares the data list for the test.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        /// <param name="dataProviders">The types of data providers to use for preparing data.</param>
        /// <returns>A list of prepared data.</returns>
        /// <exception cref="Exception">Thrown when prepared data for a data provider is not found.</exception>
        internal static List<object> PrepareDataList(ITest test, Type[] dataProviders)
        {
            return dataProviders.Select(dataPreparationType => PrepareData(test, dataPreparationType)).ToList();
        }

        private static object? Method(ITest test, MethodInfo? methodInfo)
        {
            if (methodInfo == null) return null;

            var dataPreparationMethodType = DataTypeStore.GetMethodDataPreparationType(methodInfo);
            if (dataPreparationMethodType == null)
            {
                Console.Error.WriteLine(
                    $"Prepared data method for method {methodInfo.Name} in test {test.MethodName} not registered.");
                
            }
            else
            {
                var dataPreparationMethodClass =
                    TestStore.GetTestFixtureServiceData((MethodBase)test.Method.MethodInfo, dataPreparationMethodType);
                if (dataPreparationMethodClass == null)
                {
                    Console.Error.WriteLine(
                        $"Prepared data method class {dataPreparationMethodType.FullName} for method {methodInfo.Name} in test {test.MethodName} not registered.");
                }

                return dataPreparationMethodClass;
            }

            return null;
        }

        private static object? Class(ITest test, Type classType)
        {
            var dataPreparationClassType = DataTypeStore.GetClassDataPreparationType(classType);
            if (dataPreparationClassType == null)
            {
                Console.Error.WriteLine(
                    $"Prepared data class for class {classType.Name} in test {test.MethodName} not registered.");
            }
            else
            {
                var dataPreparationClass = TestStore.GetTestFixtureServiceData((MethodBase)test.Method.MethodInfo, dataPreparationClassType);
                if (dataPreparationClass == null)
                {
                    Console.Error.WriteLine(
                        $"Prepared data method class {dataPreparationClassType.FullName} for class {classType.Name} in test {test.MethodName} not registered.");
                }

                return dataPreparationClass;
            }

            return null;
        }
    }
}