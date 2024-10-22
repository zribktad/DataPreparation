using DataPreparation.Data;
using DataPreparation.Testing;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataPreparation.DataHandling
{
    internal static class GetDataPreparation
    {

        internal static IMethodDataPreparation? Method(ITest test, MethodInfo? methodInfo)
        {
            if (methodInfo != null)
            {
                var dataPreparationMethodType = DataTypeStore.GetMethodDataPreparationType(methodInfo);
                if (dataPreparationMethodType == null)
                {
                   Console.Error.WriteLine($"Prepared data method for method {methodInfo.Name} in test {test.MethodName} not registered.");
                }
                else
                {
                    var dataPreparationMethodClass =
                        DataRegister.GetTestCaseServiceData(test, dataPreparationMethodType);
                    if (dataPreparationMethodClass == null)
                    {
                        Console.Error.WriteLine($"Prepared data method class {dataPreparationMethodType.FullName} for method {methodInfo.Name} in test {test.MethodName} not registered.");
                    }

                    if (dataPreparationMethodClass is not IMethodDataPreparation dataPreparationMethodInstance)
                    {
                        Console.Error.WriteLine(
                            $"Prepared data method class {dataPreparationMethodType.FullName} for method {methodInfo.Name} in test {test.MethodName} has not IMethodDataPreparation interface .");
                    }
                    else
                    {
                        return dataPreparationMethodInstance;
                    }
                }
            }
            return null;
        }

        internal static IClassDataPreparation? Class(ITest test, Type classType)
        {
            var dataPreparationClassType = DataTypeStore.GetClassDataPreparationType(classType);
            if (dataPreparationClassType == null)
            {
                Console.Error.WriteLine($"Prepared data class for class {classType.Name} in test {test.MethodName} not registered.");
            }
            else
            {
                var dataPreparationClass = DataRegister.GetTestCaseServiceData(test, dataPreparationClassType);
                if (dataPreparationClass == null)
                {
                    Console.Error.WriteLine($"Prepared data method class {dataPreparationClassType.FullName} for class {classType.Name} in test {test.MethodName} not registered.");
                }
                if (dataPreparationClass is IClassDataPreparation dataPreparationClassInstance)
                {
                    return dataPreparationClassInstance;


                }
                else
                {
                    Console.Error.WriteLine(
                        $"Prepared data method class {dataPreparationClassType.FullName} for class {classType.Name} in test {test.MethodName} has not IClassDataPreparation interface .");
                }
            }
            return null;
        }
    }
}
