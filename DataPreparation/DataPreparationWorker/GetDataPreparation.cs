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
            try
            {
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
            catch (InvalidOperationException e)
            {
                testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation)).LogError(
                    e,
                    $"Prepared data with type {preparedDataType.FullName} not found.");
                throw new InvalidOperationException(
                    $"For prepared data with type {preparedDataType.FullName} not found suitable constructor, check Dependency Injection.", e);
            }
            catch (Exception e)
            {
                testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation)).LogError(
                    e,
                    $"Prepared data with type {preparedDataType.FullName} not found.");
                throw new Exception(
                    $"Prepared data with type {preparedDataType.FullName} not found.", e);
            }
        
            
          
        }
        
        /// <summary>
        /// Gets prepared data types for class/methods and retrieves prepared data for a test case according to the prepared data types.
        /// If prepared data for class not used, it will be null and methods.
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
            List<Type?> preparedDataTypes = new();
            var logger = testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation));
            
            if (useClassDataPreparation)
            {                     
                Type? preparedDataType = DataRelationStore.GetClassDataPreparationType(classType);
                if (preparedDataType == null)
                {
                    var e =  new InvalidOperationException(
                        $"Prepared data for class {classType.FullName} not registered.");
                    logger.LogError(e,
                        $"Data preparation failed.");
                    throw e;
                }

                preparedDataTypes.Add(preparedDataType);
            }
            else
            {
                logger.LogTrace(
                    $"Class data preparation not used for class {classType.FullName}.");
            }

            foreach (var methodName in methodsNames)
            {
                MethodInfo? methodInfo = classType.GetMethod(methodName);
                if (methodInfo == null)
                {
                    
                    var e =  new InvalidOperationException(
                        $"Method {methodName} not found in class {classType.FullName}.");
                    logger.LogError(e,
                        $"Data preparation failed.");
                    throw e;
                  
                }
                Type? preparedDataType = DataRelationStore.GetMethodDataPreparationType(methodInfo);
                if (preparedDataType == null)
                {
                    
                    var e =   new InvalidOperationException(
                        $"Prepared data for method {methodInfo} not registered.");
                    logger.LogError(e,
                        $"Data preparation failed.");
                    throw e;
                 
                }
                preparedDataTypes.Add(preparedDataType);
            
            }
            
            return GetPreparedData(testStore,preparedDataTypes.ToArray());
        }

        public static List<object> GetPreparedDataFromCode(TestStore testStore, bool useClassDataPreparation, Type classType, string? methodsNames)
        {
            return  GetPreparedDataFromCode(testStore, useClassDataPreparation, classType, methodsNames == null ? []:[methodsNames]);
        }


        public static (object[]?[], object[]?[]) FilterParams(bool useClassDataPreparation, string? methodName, object[]? classClassParamsUpData, object[]? methodParamsUpData, object[]? classParamsDownData, object[]? methodParamsDownData)
        {
            List<object[]?> paramsUpData = new();
            List<object[]?> paramsDown = new();
            if (useClassDataPreparation)
            {
                paramsUpData.Add(classClassParamsUpData);
                paramsDown.Add(classParamsDownData);
            }

            if (methodName != null)
            {
                paramsUpData.Add(methodParamsUpData);
                paramsDown.Add(methodParamsDownData);
            }
            return (paramsUpData.ToArray(), paramsDown.ToArray());
        }
    }
}