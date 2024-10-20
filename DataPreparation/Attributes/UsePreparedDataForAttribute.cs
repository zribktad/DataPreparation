using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataPreparation.Testing.Interfaces;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class UsePreparedDataForAttribute: Attribute, ITestAction
    {
        private Type ClassType { get; }
        private string[] MethodsNames { get; }
        public UsePreparedDataForAttribute(Type classType, params string[] methodsNames)
        {
            ClassType = classType;
            MethodsNames = methodsNames;
        }

        public void BeforeTest(ITest test)
        {
             
            var dataPreparationClassType = DataRegister.GetClassDataPreparationType(ClassType);
            var dataPreparationClass = DataRegister.GetTestCaseService(test, dataPreparationClassType);
            if (dataPreparationClass == null)
            {
                //TODO
            }

            if (dataPreparationClass is IClassDataPreparation dataPreparationClassInstance)
            {
                dataPreparationClassInstance.TestUpData();

            }
            else
            {
                //TODO
            }

            foreach (var methodName in MethodsNames)
            {
                var methodInfo = ClassType.GetMethod(methodName);
                if (methodInfo != null)
                {
                    var dataPreparationMethodType = DataRegister.GetMethodDataPreparationType(methodInfo);
                    var dataPreparationMethodClass = DataRegister.GetTestCaseService(test, dataPreparationMethodType);
                    if (dataPreparationMethodClass == null )
                    {
                        //TODO
                    }

                    if (dataPreparationMethodClass is IMethodDataPreparation dataPreparationMethodInstance)
                    {
                        dataPreparationMethodInstance.TestUpData();
                    }
                    else
                    {
                        //TODO
                    }
                }
            }
          
        }

        public void AfterTest(ITest test)
        {
           
        }

        public ActionTargets Targets => ActionTargets.Test;
    }
}
