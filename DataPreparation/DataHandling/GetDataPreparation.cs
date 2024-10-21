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
                var dataPreparationMethodType = DataRegister.GetMethodDataPreparationType(methodInfo);
                if (dataPreparationMethodType == null)
                {
                    //error handling 
                }
                else
                {
                    var dataPreparationMethodClass =
                        DataRegister.GetTestCaseServiceData(test, dataPreparationMethodType);
                    if (dataPreparationMethodClass == null)
                    {
                        //TODO
                    }

                    if (dataPreparationMethodClass is IMethodDataPreparation dataPreparationMethodInstance)
                    {
                        return dataPreparationMethodInstance;
                    }
                    else
                    {
                        //TODO
                    }
                }
            }
            return null;
        }

        internal static IClassDataPreparation? Class(ITest test, Type classType)
        {
            var dataPreparationClassType = DataRegister.GetClassDataPreparationType(classType);
            if (dataPreparationClassType == null)
            {
                //error handling 
            }
            else
            {
                var dataPreparationClass = DataRegister.GetTestCaseServiceData(test, dataPreparationClassType);
                if (dataPreparationClass == null)
                {
                    //TODO
                }
                if (dataPreparationClass is IClassDataPreparation dataPreparationClassInstance)
                {
                    return dataPreparationClassInstance;


                }
                else
                {
                    //TODO
                }
            }
            return null;
        }
    }
}
