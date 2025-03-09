using System.Reflection;
using DataPreparation.Models.Data;
using DataPreparation.Testing;
using Microsoft.Extensions.Logging;

namespace DataPreparation.DataHandlers
{
    /// <summary>
    /// Provides methods to get prepared data for test cases.
    /// </summary>
    internal static class GetDataPreparation
    {
        /// <summary>
        /// Gets prepared data for a test case according to the provided data types.
        /// </summary>
        /// <param name="testStore">The test store containing the service provider and logger factory.</param>
        /// <param name="dataProviders">An array of types representing the data providers.</param>
        /// <returns>A list of prepared data objects.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the prepared data is not found.</exception>

        internal static List<object> GetPreparedData(TestStore testStore, Type[] dataProviders)
        {
            return dataProviders.Select(dataPreparationType => GetPreparedData(testStore, dataPreparationType)).ToList();
        }
        /// <summary>
        /// Gets prepared data from the service provider according to the provided data type.
        /// </summary>
        /// <param name="testStore">The test store containing the service provider and logger factory.</param>
        /// <param name="preparedDataType">The type of the prepared data.</param>
        /// <returns>The prepared data object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the prepared data is not found.</exception>
        private static object GetPreparedData(TestStore testStore, Type preparedDataType)
        {
            testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation)).LogTrace(
                $"Getting prepared data with type {preparedDataType.FullName}.");
     
            var preparedData = testStore.ServiceProvider.GetService(preparedDataType);
            if (preparedData == null) 
            {
                testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation)).LogError(
                    $"Prepared data with type {preparedDataType.FullName} not found.");
                throw new InvalidOperationException(
                    $"Prepared data with type {preparedDataType.FullName} not found.");
            }
            testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation)).LogDebug(
                $"Prepared data with type {preparedDataType.FullName} found.");
            
            return preparedData;
        }
        
        /// <summary>
        /// Gets prepared data types for class/methods and retrieves prepared data for a test case according to the prepared data types.
        /// </summary>
        /// <param name="testStore">The test store containing the service provider and logger factory.</param>
        /// <param name="useClassDataPreparation">Indicates whether to use class-level data preparation.</param>
        /// <param name="classType">The type of the class.</param>
        /// <param name="methodsNames">An array of method names.</param>
        /// <returns>A list of prepared data objects.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the prepared data is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the prepared data or method is not found or not registered.</exception>
        internal static List<object> GetPreparedDataFromCode(TestStore testStore, bool useClassDataPreparation, Type classType,
            string[] methodsNames)
        {
            List<Type> preparedDataTypes = new();
            if (useClassDataPreparation)
            {                     
                Type? preparedDataType = DataRelationStore.GetClassDataPreparationType(classType);
                if (preparedDataType == null)
                {
                    
                    testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation)).LogError(
                        $"Prepared data for class {classType.FullName} not registered.");
                    throw new InvalidOperationException(
                        $"Prepared data for class {classType.FullName} not registered.");
                }

                preparedDataTypes.Add(preparedDataType);
            }
            foreach (var methodName in methodsNames)
            {
                MethodInfo? methodInfo = classType.GetMethod(methodName);
                if (methodInfo == null)
                {
                    testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation)).LogError(
                        $"Method {methodName} not found in class {classType.FullName}.");
                    throw new InvalidOperationException(
                        $"Method {methodName} not found in class {classType.FullName}.");
                  
                }
                Type? preparedDataType = DataRelationStore.GetMethodDataPreparationType(methodInfo);
                if (preparedDataType == null)
                {
                    testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation)).LogError(
                        $"Prepared data for method {methodInfo} not registered.");
                    throw new InvalidOperationException(
                        $"Prepared data for method {methodInfo} not registered.");
                }
                preparedDataTypes.Add(preparedDataType);
            
            }
            
            return GetPreparedData(testStore,preparedDataTypes.ToArray());
        }
        
     
        
        
    }
}