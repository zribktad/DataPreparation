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

            var dataPreparationClass = DataRegister.GetClassDataPreparationType(ClassType);
            if (dataPreparationClass == null)
            {
            }

            if (dataPreparationClass is IClassDataPreparation dataPreparationClassInstance)
            {
                dataPreparationClassInstance.TestUpData();

            }

            foreach (var methodName in MethodsNames)
            {
                var methodInfo = ClassType.GetMethod(methodName);
                if (methodInfo != null)
                {
                    var dataPreparationMethod = DataRegister.GetMethodDataPreparationType(methodInfo);
                    if (dataPreparationMethod == null )
                    {
                    }

                    if (dataPreparationMethod is IMethodDataPreparation dataPreparationMethod)
                    {
                        dataPreparationMethod.TestUpData();
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
