using System.Reflection;
using DataPreparation.Data;
using DataPreparation.Models.Data;
using DataPreparation.Testing;
using NUnit.Framework.Interfaces;

namespace DataPreparation.DataHandling
{
    internal static class GetDataPreparation
    {
        internal static List<object?> GetPreparedData(TestStore testStore, Type[] dataProviders)
        {
            return dataProviders.Select(dataPreparationType => GetPreparedData(testStore, dataPreparationType)).ToList();
        }
        
        internal static List<object?> GetPreparedDataFromCode(TestStore testStore, bool useClassDataPreparation, Type classType,
            string[] methodsNames)
        {
            List<Type> preparedDataTypes = new();
            if (useClassDataPreparation)
            {                     
                Type? preparedDataType = DataRelationStore.GetClassDataPreparationType(classType);
                if (preparedDataType == null)
                {
                    Console.Error.WriteLine(
                        $"Prepared data type for class {classType.FullName} not registered.");
                }

                preparedDataTypes.Add(preparedDataType);
            }
            foreach (var methodName in methodsNames)
            {
                MethodInfo? methodInfo = classType.GetMethod(methodName);
                if (methodInfo == null)
                {
                    Console.Error.WriteLine(
                        $"Method {methodName} not found in class {classType.FullName}.");
                }
                Type? preparedDataType = DataRelationStore.GetMethodDataPreparationType(methodInfo);
                if (preparedDataType == null)
                {
                    Console.Error.WriteLine(
                        $"Prepared data type for class {classType.FullName} not registered.");
                }
                preparedDataTypes.Add(preparedDataType);
            
            }
            
            
            return GetPreparedData(testStore,preparedDataTypes.ToArray());
        }
        
        private static object? GetPreparedData(TestStore testStore, Type preparedDataType)
        {
            if (preparedDataType == null)
            {
                Console.Error.WriteLine(
                    $"Prepared data type for test {testStore} not registered.");
            }
            else
            {
                var preparedData = testStore.ServiceProvider.GetService(preparedDataType);
                if (preparedData == null)
                {
                    Console.Error.WriteLine(
                        $"Prepared data for type {preparedDataType.FullName} in test {testStore} not registered.");
                }

                return preparedData;
            }

            return null;
        }
        
        
    }
}