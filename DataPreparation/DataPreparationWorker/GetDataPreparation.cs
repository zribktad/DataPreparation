using System.Reflection;
using DataPreparation.Models.Data;
using DataPreparation.Testing;
using Microsoft.Extensions.Logging;

namespace DataPreparation.DataHandlers
{
    /// <summary>
    /// Core helper class that resolves and obtains data preparation instances from the dependency injection container.
    /// This class serves as a bridge between test attributes and data preparation classes.
    /// </summary>
    /// <remarks>
    /// GetDataPreparation has two primary functions:
    /// 1. Resolve data preparation instances directly from types specified in attributes like UsePreparedData
    /// 2. Resolve data preparation instances indirectly by looking up class/method associations in DataRelationStore
    /// 
    /// This enables both explicit data preparation via attributes and implicit data preparation via conventions.
    /// </remarks>
    internal static class GetDataPreparation
    {
        /// <summary>
        /// Gets prepared data instances for a test case by resolving each specified data provider type.
        /// </summary>
        /// <param name="testStore">The test store containing the DI service provider</param>
        /// <param name="dataProviders">Array of types representing the data provider classes</param>
        /// <returns>A list of instantiated data preparation objects ready for execution</returns>
        /// <remarks>
        /// This method is used by the UsePreparedData attribute to resolve the data preparation objects
        /// that will be used to set up and tear down test data before and after test execution.
        /// </remarks>
        internal static List<object> GetPreparedData(TestStore testStore, Type[] dataProviders)
        {
            return dataProviders.Select(dataPreparationType => GetPreparedData(testStore, dataPreparationType)).ToList();
        }
        
        /// <summary>
        /// Gets a single prepared data instance from the service provider by its type.
        /// </summary>
        /// <param name="testStore">The test store containing the service provider and logger factory</param>
        /// <param name="preparedDataType">The type of data preparation class to resolve</param>
        /// <returns>An instantiated data preparation object</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the data preparation type cannot be resolved from the dependency injection container,
        /// either because it's not registered or because its dependencies cannot be satisfied
        /// </exception>
        /// <remarks>
        /// This method uses dependency injection to create instances of data preparation classes,
        /// allowing those classes to have their own dependencies injected (like database connections).
        /// </remarks>
        private static object GetPreparedData(TestStore testStore, Type preparedDataType)
        {
            var logger = testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation));
            logger.LogTrace($"Getting prepared data with type {preparedDataType.FullName}.");
            
            try
            {
                // Attempt to resolve the data preparation type from the DI container
                var preparedData = testStore.ServiceProvider.GetService(preparedDataType);
                
                if (preparedData == null) 
                {
                    logger.LogError($"Prepared data with type {preparedDataType.FullName} not found.");
                    throw new InvalidOperationException(
                        $"Prepared data with type {preparedDataType.FullName} not found.");
                }
                
                logger.LogDebug($"Prepared data with type {preparedDataType.FullName} found.");
                return preparedData;
            }
            catch (InvalidOperationException e)
            {
                // This typically happens when the DI container can't satisfy the constructor dependencies
                logger.LogError(e, $"Prepared data with type {preparedDataType.FullName} not found.");
                throw new InvalidOperationException(
                    $"For prepared data with type {preparedDataType.FullName} not found suitable constructor, check Dependency Injection.", e);
            }
            catch (Exception e)
            {
                // Catch and rethrow any other exception that might occur during resolution
                logger.LogError(e, $"Prepared data with type {preparedDataType.FullName} not found.");
                throw new Exception(
                    $"Prepared data with type {preparedDataType.FullName} not found.", e);
            }
        }
        
        /// <summary>
        /// Gets prepared data instances by looking up data preparation classes associated with a specified
        /// class and/or methods in the DataRelationStore.
        /// </summary>
        /// <param name="testStore">The test store containing the service provider</param>
        /// <param name="useClassDataPreparation">Whether to use class-level data preparation</param>
        /// <param name="classType">The class type to get associated data preparation for</param>
        /// <param name="methodsNames">Names of methods to get associated data preparation for</param>
        /// <returns>A list of instantiated data preparation objects</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified class or methods don't have registered data preparation types,
        /// or when the methods cannot be found in the class
        /// </exception>
        /// <remarks>
        /// This method enables convention-based data preparation, where data preparation classes can be
        /// associated with test classes and methods using PreparationClassFor and PreparationMethodFor attributes.
        /// This allows for reusing data preparation classes across multiple test methods.
        /// </remarks>
        internal static List<object> GetPreparedDataFromCode(TestStore testStore, bool useClassDataPreparation, Type classType,
            string[] methodsNames)
        {
            List<Type?> preparedDataTypes = new();
            var logger = testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation));
            
            // If using class data preparation, look up the data preparation type for this class
            if (useClassDataPreparation)
            {                     
                Type? preparedDataType = DataRelationStore.GetClassDataPreparationType(classType);
                if (preparedDataType == null)
                {
                    var e = new InvalidOperationException(
                        $"Prepared data for class {classType.FullName} not registered.");
                    logger.LogError(e, $"Data preparation failed.");
                    throw e;
                }

                preparedDataTypes.Add(preparedDataType);
            }
            else
            {
                logger.LogTrace($"Class data preparation not used for class {classType.FullName}.");
            }

            // For each specified method, look up its data preparation type
            foreach (var methodName in methodsNames)
            {
                // Find the method by name
                MethodInfo? methodInfo = classType.GetMethod(methodName);
                if (methodInfo == null)
                {
                    var e = new InvalidOperationException(
                        $"Method {methodName} not found in class {classType.FullName}.");
                    logger.LogError(e, $"Data preparation failed.");
                    throw e;
                }
                
                // Look up data preparation type for this method
                Type? preparedDataType = DataRelationStore.GetMethodDataPreparationType(methodInfo);
                if (preparedDataType == null)
                {
                    var e = new InvalidOperationException(
                        $"Prepared data for method {methodInfo} not registered.");
                    logger.LogError(e, $"Data preparation failed.");
                    throw e;
                }
                
                preparedDataTypes.Add(preparedDataType);
            }
            
            // Resolve all the identified data preparation types
            return GetPreparedData(testStore, preparedDataTypes.ToArray());
        }

        /// <summary>
        /// Overload of GetPreparedDataFromCode that handles a single method name or null.
        /// </summary>
        /// <param name="testStore">The test store containing the service provider</param>
        /// <param name="useClassDataPreparation">Whether to use class-level data preparation</param>
        /// <param name="classType">The class type to get associated data preparation for</param>
        /// <param name="methodsNames">Optional single method name to get associated data preparation for</param>
        /// <returns>A list of instantiated data preparation objects</returns>
        public static List<object> GetPreparedDataFromCode(TestStore testStore, bool useClassDataPreparation, Type classType, string? methodsNames)
        {
            return GetPreparedDataFromCode(testStore, useClassDataPreparation, classType, methodsNames == null ? [] : [methodsNames]);
        }

        /// <summary>
        /// Filters and organizes parameter arrays for data preparation execution.
        /// </summary>
        /// <param name="useClassDataPreparation">Whether to use class-level data preparation</param>
        /// <param name="methodName">Optional method name</param>
        /// <param name="classClassParamsUpData">Parameters for class-level UpData methods</param>
        /// <param name="methodParamsUpData">Parameters for method-level UpData methods</param>
        /// <param name="classParamsDownData">Parameters for class-level DownData methods</param>
        /// <param name="methodParamsDownData">Parameters for method-level DownData methods</param>
        /// <returns>A tuple containing arrays of parameters for UpData and DownData methods</returns>
        /// <remarks>
        /// This utility method organizes parameters for data preparation execution based on whether
        /// class-level and/or method-level data preparation is being used.
        /// </remarks>
        public static (object[]?[], object[]?[]) FilterParams(
            bool useClassDataPreparation, 
            string? methodName, 
            object[]? classClassParamsUpData, 
            object[]? methodParamsUpData, 
            object[]? classParamsDownData, 
            object[]? methodParamsDownData)
        {
            List<object[]?> paramsUpData = new();
            List<object[]?> paramsDown = new();
            
            // Include class parameters if using class data preparation
            if (useClassDataPreparation)
            {
                paramsUpData.Add(classClassParamsUpData);
                paramsDown.Add(classParamsDownData);
            }

            // Include method parameters if a method name is provided
            if (methodName != null)
            {
                paramsUpData.Add(methodParamsUpData);
                paramsDown.Add(methodParamsDownData);
            }
            
            return (paramsUpData.ToArray(), paramsDown.ToArray());
        }
    }
}